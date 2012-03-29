// 
// HOTween.cs
//  
// Author: Daniele Giardini
// 
// Copyright (c) 2012 Daniele Giardini - Holoville - http://www.holoville.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// Created: 2011/12/13
// Last update: 2012/03/29

using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;
using Holoville.HOTween.Plugins.Core;
using UnityEngine;

namespace Holoville.HOTween
{
	/// <summary>
	/// Main tween manager.
	/// Controls all tween types (<see cref="Tweener"/> and <see cref="Sequence"/>),
	/// and is used to directly create Tweeners (to create Sequences, directly create a new <see cref="Sequence"/> instead).
	/// <para>Author: Daniele Giardini (http://www.holoville.com)</para>
	/// <para>Version: 0.9.003</para>
	/// </summary>
	public class HOTween : MonoBehaviour
	{
		// SETTINGS ///////////////////////////////////////////////
		
		/// <summary>
		/// HOTween version.
		/// </summary>
		public	const		string							VERSION = "0.9.003";
		/// <summary>
		/// HOTween author - me! :P
		/// </summary>
		public	const		string							AUTHOR = "Daniele Giardini - Holoville";
		
		private	const		string							GAMEOBJNAME = "HOTween";
		
		// DEFAULTS ///////////////////////////////////////////////
		
		/// <summary>
		/// Default <see cref="UpdateType"/> that will be used by any new Tweener/Sequence that doesn't implement a specific ease
		/// (default = <c>EaseType.easeOutQuad</c>)
		/// </summary>
		static	public		UpdateType						defUpdateType = UpdateType.Update;
		/// <summary>
		/// Default time scale that will be used by any new Tweener/Sequence that doesn't implement a specific timeScale
		/// (default = <c>1</c>).
		/// </summary>
		static	public		float							defTimeScale = 1;
		/// <summary>
		/// Default <see cref="EaseType"/> that will be used by any new Tweener/Sequence that doesn't implement a specific ease
		/// (default = <c>EaseType.easeOutQuad</c>).
		/// </summary>
		static	public		EaseType						defEaseType = EaseType.EaseOutQuad;
		/// <summary>
		/// Default <see cref="LoopType"/> that will be used by any Tweener/Sequence that doesn't implement a specific loopType
		/// (default = <c>LoopType.Restart</c>).
		/// </summary>
		static	public		LoopType						defLoopType = LoopType.Restart;
		
		// VARS ///////////////////////////////////////////////////
		
		/// <summary>
		/// If <c>true</c>, shows the eventual paths in use by <see cref="PlugVector3Path"/>
		/// while playing inside Unity's Editor (and if the Editor's Gizmos button is on).
		/// </summary>
		static	public		bool							showPathGizmos = false;
		/// <summary>
		/// Level of message output in case an error is encountered.
		/// Warnings are logged when HOTween encounters an error, and automatically resolves it without throwing any exception
		/// (like if you try to tween an unexisting property, in which case the tween simply won't be generated,
		/// and an eventual warning will appear in the output window).
		/// </summary>
		static	public		WarningLevel					warningLevel = WarningLevel.Verbose;
		
		/// <summary>
		/// <c>true</c> if the current player is iOS (iPhone).
		/// Used so simple Reflection instead than unsupported MemberAccessorCacher will be applyed
		/// (iOS doesn't support <c>Reflection.Emit</c>).
		/// </summary>
		static	internal	bool							isIOS;
		/// <summary>
		/// <c>true</c> if the current player is running in the Editor.
		/// </summary>
		static	internal	bool							isEditor;
		/// <summary>
		/// Filled by tweens that are completed, so that their onCompleteDispatch method can be called AFTER HOTween has eventually removed them
		/// (otherwise a Kill + To on the same target won't work).
		/// This field is emptied as soon as all onCompletes are called.
		/// </summary>
		static	internal	List<ABSTweenComponent>			onCompletes = new List<ABSTweenComponent>();
		
		static	private		bool							initialized;
		static	private		bool							isPermanent; // If TRUE doesn't destroy HOTween when all tweens are killed.
		static	private		bool							renameInstToCountTw; // If TRUE renames HOTween's instance to show running tweens.
		static	private		float							time;
		
		// REFERENCES /////////////////////////////////////////////
		
		/// <summary>
		/// Reference to overwrite manager (if in use).
		/// </summary>
		static	internal	OverwriteManager				overwriteMngr;
		
		static	private		List<ABSTweenComponent>			tweens; // Contains both Tweeners than Sequences
		static	private		GameObject						tweenGOInstance;
		static	private		HOTween							it;
		
		// READ-ONLY GETS /////////////////////////////////////////
		
		/// <summary>
		/// Total number of tweeners/sequences (paused and delayed ones are included).
		/// Tweeners and sequences contained into other sequences don't count:
		/// for example, if there's only one sequence that contains 2 tweeners, <c>totTweens</c> will be 1.
		/// </summary>
		static	public		int								totTweens
		{
			get { if ( tweens == null ) return 0; return tweens.Count; }
		}
		
		
		// ***********************************************************************************
		// INIT
		// ***********************************************************************************
		
		/// <summary>
		/// Initializes <see cref="HOTween"/> and sets it as non-permanent
		/// (meaning HOTween instance will be destroyed when all tweens are killed,
		/// and re-created when needed).
		/// Call this method once when your application starts up,
		/// to avoid auto-initialization when the first tween is started or created,
		/// and to set options.
		/// </summary>
		static public void Init() { Init( false, true ); }
		/// <summary>
		/// Initializes <see cref="HOTween"/>.
		/// Call this method once when your application starts up,
		/// to avoid auto-initialization when the first tween is started or created,
		/// and to set options.
		/// </summary>
		/// <param name="p_permanentInstance">
		/// If set to <c>true</c>, doesn't destroy HOTween manager when no tween is present,
		/// otherwise the manager is destroyed when all tweens have been killed,
		/// and re-created when needed.
		/// </param>
		static public void Init( bool p_permanentInstance ) { Init( p_permanentInstance, true ); }
		/// <summary>
		/// Initializes <see cref="HOTween"/>.
		/// Call this method once when your application starts up,
		/// to avoid auto-initialization when the first tween is started or created,
		/// and to set options.
		/// </summary>
		/// <param name="p_permanentInstance">
		/// If set to <c>true</c>, doesn't destroy HOTween manager when no tween is present,
		/// otherwise the manager is destroyed when all tweens have been killed,
		/// and re-created when needed.
		/// </param>
		/// <param name="p_renameInstanceToCountTweens">
		/// If <c>true</c>, renames HOTween's instance to show
		/// the current number of running tweens (only while in the Editor).
		/// </param>
		static public void Init( bool p_permanentInstance, bool p_renameInstanceToCountTweens )
		{
			if ( initialized )						return;
			
			initialized = true;
			
			isIOS = ( Application.platform == RuntimePlatform.IPhonePlayer );
			isEditor = Application.isEditor;
			isPermanent = p_permanentInstance;
			renameInstToCountTw = p_renameInstanceToCountTweens;
			overwriteMngr = new OverwriteManager();
			
			if ( isPermanent && tweenGOInstance == null ) {
				NewTweenInstance();
				SetGOName();
			}
		}
		
		// ===================================================================================
		// UNITY METHODS ---------------------------------------------------------------------
		
		private void OnDrawGizmos()
		{
			if ( tweens == null || !showPathGizmos )		return;
			
			// Get all existing plugins.
			List<ABSTweenPlugin> plugs = GetPlugins();
			
			// Find path plugins and draw paths.
			for ( int i = 0; i < plugs.Count; ++i ) {
				ABSTweenPlugin plug = plugs[i];
				if ( plug is PlugVector3Path ) {
					PlugVector3Path pathPlug = ( plug as PlugVector3Path );
					if ( pathPlug.path != null )
						pathPlug.path.GizmoDraw( pathPlug.pathPerc, false );
				}
			}
		}
		
		private void OnDestroy()
		{
			// Clear everything if this was the currenlty running HOTween.
			// HINT I can use OnDestroy also to check for scene changes, and instantiate another HOTween instance if I need to keep it running.
			// TODO For now HOTween is NOT destroyed when a scene is loaded, - add option to set it as destroyable?
			// (consider also isPermanent option if doing that).
			if ( this == it )			Clear();
		}
		
		// ===================================================================================
		// TWEEN METHODS ---------------------------------------------------------------------
		
		/// <summary>
		/// Called internally each time a new <see cref="Sequence"/> is created.
		/// Adds the given Sequence to the tween list.
		/// </summary>
		/// <param name="p_sequence">
		/// The <see cref="Sequence"/> to add.
		/// </param>
		static internal void AddSequence( Sequence p_sequence )
		{
			if ( !initialized )						Init();
			
			AddTween( p_sequence );
		}
		
		/// <summary>
		/// Creates a new absolute tween with default values, and returns the <see cref="Tweener"/> representing it,
		/// or <c>null</c> if the tween was invalid (no valid property to tween was given).
		/// </summary>
		/// <param name="p_target">
		/// The tweening target (must be the object containing the properties or fields to tween).
		/// </param>
		/// <param name="p_duration">
		/// The duration in seconds of the tween.
		/// </param>
		/// <param name="p_propName">
		/// The name of the property or field to tween.
		/// </param>
		/// <param name="p_endVal">
		/// The end value the property should reach with the tween.
		/// </param>
		/// <returns>
		/// The newly created <see cref="Tweener"/>,
		/// or <c>null</c> if the parameters were invalid.
		/// </returns>
		static public Tweener To ( object p_target, float p_duration, string p_propName, object p_endVal ) { return To( p_target, p_duration, new TweenParms().Prop( p_propName, p_endVal ) ); }
		/// <summary>
		/// Creates a new tween with default values, and returns the <see cref="Tweener"/> representing it,
		/// or <c>null</c> if the tween was invalid (no valid property to tween was given).
		/// </summary>
		/// <param name="p_target">
		/// The tweening target (must be the object containing the properties or fields to tween).
		/// </param>
		/// <param name="p_duration">
		/// The duration in seconds of the tween.
		/// </param>
		/// <param name="p_propName">
		/// The name of the property or field to tween.
		/// </param>
		/// <param name="p_endVal">
		/// The end value the property should reach with the tween.
		/// </param>
		/// <param name="p_isRelative">
		/// If <c>true</c> treats the end value as relative (tween BY instead than tween TO), otherwise as absolute.
		/// </param>
		/// <returns>
		/// The newly created <see cref="Tweener"/>,
		/// or <c>null</c> if the parameters were invalid.
		/// </returns>
		static public Tweener To ( object p_target, float p_duration, string p_propName, object p_endVal, bool p_isRelative ) { return To( p_target, p_duration, new TweenParms().Prop( p_propName, p_endVal, p_isRelative ) ); }
		/// <summary>
		/// Creates a new tween and returns the <see cref="Tweener"/> representing it,
		/// or <c>null</c> if the tween was invalid (no valid property to tween was given).
		/// </summary>
		/// <param name="p_target">
		/// The tweening target (must be the object containing the properties or fields to tween).
		/// </param>
		/// <param name="p_duration">
		/// The duration in seconds of the tween.
		/// </param>
		/// <param name="p_parms">
		/// A <see cref="TweenParms"/> representing the tween parameters.
		/// You can pass an existing one, or create a new one inline via method chaining,
		/// like <c>new TweenParms().Prop("x",10).Loops(2).OnComplete(myFunction)</c>
		/// </param>
		/// <returns>
		/// The newly created <see cref="Tweener"/>,
		/// or <c>null</c> if the parameters were invalid.
		/// </returns>
		static public Tweener To ( object p_target, float p_duration, TweenParms p_parms )
		{
			if ( !initialized )						Init();
			
			Tweener tw = new Tweener( p_target, p_duration, p_parms );
			
			// Check if tween is valid.
			if ( tw.isEmpty )		return null;
			
			AddTween( tw );
			return tw;
		}
		
		/// <summary>
		/// Creates a new absolute FROM tween with default values, and returns the <see cref="Tweener"/> representing it,
		/// or <c>null</c> if the tween was invalid (no valid property to tween was given).
		/// </summary>
		/// <param name="p_target">
		/// The tweening target (must be the object containing the properties or fields to tween).
		/// </param>
		/// <param name="p_duration">
		/// The duration in seconds of the tween.
		/// </param>
		/// <param name="p_propName">
		/// The name of the property or field to tween.
		/// </param>
		/// <param name="p_fromVal">
		/// The end value the property should reach with the tween.
		/// </param>
		/// <returns>
		/// The newly created <see cref="Tweener"/>,
		/// or <c>null</c> if the parameters were invalid.
		/// </returns>
		static public Tweener From ( object p_target, float p_duration, string p_propName, object p_fromVal ) { return From( p_target, p_duration, new TweenParms().Prop( p_propName, p_fromVal ) ); }
		/// <summary>
		/// Creates a new FROM tween with default values, and returns the <see cref="Tweener"/> representing it,
		/// or <c>null</c> if the tween was invalid (no valid property to tween was given).
		/// </summary>
		/// <param name="p_target">
		/// The tweening target (must be the object containing the properties or fields to tween).
		/// </param>
		/// <param name="p_duration">
		/// The duration in seconds of the tween.
		/// </param>
		/// <param name="p_propName">
		/// The name of the property or field to tween.
		/// </param>
		/// <param name="p_fromVal">
		/// The end value the property should reach with the tween.
		/// </param>
		/// <param name="p_isRelative">
		/// If <c>true</c> treats the end value as relative (tween BY instead than tween TO), otherwise as absolute.
		/// </param>
		/// <returns>
		/// The newly created <see cref="Tweener"/>,
		/// or <c>null</c> if the parameters were invalid.
		/// </returns>
		static public Tweener From ( object p_target, float p_duration, string p_propName, object p_fromVal, bool p_isRelative ) { return From( p_target, p_duration, new TweenParms().Prop( p_propName, p_fromVal, p_isRelative ) ); }
		/// <summary>
		/// Creates a new FROM tween and returns the <see cref="Tweener"/> representing it,
		/// or <c>null</c> if the tween was invalid (no valid property to tween was given).
		/// </summary>
		/// <param name="p_target">
		/// The tweening target (must be the object containing the properties or fields to tween).
		/// </param>
		/// <param name="p_duration">
		/// The duration in seconds of the tween.
		/// </param>
		/// <param name="p_parms">
		/// A <see cref="TweenParms"/> representing the tween parameters.
		/// You can pass an existing one, or create a new one inline via method chaining,
		/// like <c>new TweenParms().Prop("x",10).Loops(2).OnComplete(myFunction)</c>
		/// </param>
		/// <returns>
		/// The newly created <see cref="Tweener"/>,
		/// or <c>null</c> if the parameters were invalid.
		/// </returns>
		static public Tweener From ( object p_target, float p_duration, TweenParms p_parms )
		{
			if ( !initialized )						Init();
			
			p_parms = p_parms.IsFrom();
			Tweener tw = new Tweener( p_target, p_duration, p_parms );
			
			// Check if tween is valid.
			if ( tw.isEmpty )		return null;
			
			AddTween( tw );
			// Immediately jump to position 0 to avoid flickering of objects before they're punched to FROM position.
			// p_isStartupIteration is set to FALSE to ignore callbacks.
			if ( !tw._isPaused )		tw.Update( 0, true, true );
			return tw;
		}
		
		// ===================================================================================
		// UPDATE METHODS --------------------------------------------------------------------
		
		/// <summary>
		/// Updates normal tweens.
		/// </summary>
		private void Update()
		{
			if ( tweens == null )		return;
			
			// Update tweens.
			DoUpdate( UpdateType.Update, Time.deltaTime );
			
			CheckClear();
		}
		
		/// <summary>
		/// Updates lateUpdate tweens.
		/// </summary>
		private void LateUpdate()
		{
			if ( tweens == null )		return;
			
			// Update tweens.
			DoUpdate( UpdateType.LateUpdate, Time.deltaTime );
			
			CheckClear();
		}
		
		/// <summary>
		/// Updates fixedUpdate tweens.
		/// </summary>
		private void FixedUpdate()
		{
			if ( tweens == null )		return;
			
			// Update tweens.
			DoUpdate( UpdateType.FixedUpdate, Time.fixedDeltaTime );
			
			CheckClear();
		}
		
		/// <summary>
		/// Updates timeScaleIndependent tweens.
		/// </summary>
		private static IEnumerator TimeScaleIndependentUpdate()
		{
			while ( tweens != null )
			{
				float elapsed = Time.realtimeSinceStartup - time;
				time = Time.realtimeSinceStartup;
				
				// Update tweens.
				DoUpdate( UpdateType.TimeScaleIndependentUpdate, elapsed );
				
				if ( CheckClear() )		yield break;
				
				yield return null;
			}
		}
		
		// ===================================================================================
		// METHODS ---------------------------------------------------------------------------
		
		/// <summary>
		/// Enables the overwrite manager (disabled by default).
		/// </summary>
		static public void EnableOverwriteManager()
		{
			overwriteMngr.enabled = true;
		}
		
		/// <summary>
		/// Disables the overwrite manager (disabled by default).
		/// </summary>
		static public void DisableOverwriteManager()
		{
			overwriteMngr.enabled = false;
		}
		
		/// <summary>
		/// Pauses all the tweens for the given target, and returns the total number of paused Tweeners.
		/// </summary>
		/// <param name="p_target">
		/// The target whose tweens to pause.
		/// </param>
		/// <returns>
		/// The total number of paused Tweeners.
		/// </returns>
		static public int Pause( object p_target ) { return DoFilteredIteration( p_target, DoFilteredPause, false ); }
		/// <summary>
		/// Pauses all the Tweeners/Sequences with the given ID, and returns the total number of paused Tweeners/Sequences.
		/// </summary>
		/// <param name="p_id">
		/// The ID of the Tweeners/Sequences to pause.
		/// </param>
		/// <returns>
		/// The total number of paused Tweeners/Sequences.
		/// </returns>
		static public int Pause( string p_id ) { return DoFilteredIteration( p_id, DoFilteredPause, false ); }
		/// <summary>
		/// Pauses all the Tweeners/Sequences with the given intId, and returns the total number of paused Tweeners/Sequences.
		/// </summary>
		/// <param name="p_intId">
		/// The intId of the Tweeners/Sequences to pause.
		/// </param>
		/// <returns>
		/// The total number of paused Tweeners/Sequences.
		/// </returns>
		static public int Pause( int p_intId ) { return DoFilteredIteration( p_intId, DoFilteredPause, false ); }
		/// <summary>
		/// Pauses the given Tweener, and returns the total number of paused ones (1 if the Tweener existed, otherwise 0).
		/// </summary>
		/// <param name="p_tweener">
		/// The Tweener to pause.
		/// </param>
		/// <returns>
		/// The total number of paused Tweener (1 if the Tweener existed, otherwise 0).
		/// </returns>
		static public int Pause( Tweener p_tweener ) { return DoFilteredIteration( p_tweener, DoFilteredPause, false ); }
		/// <summary>
		/// Pauses the given Sequence, and returns the total number of paused ones (1 if the Sequence existed, otherwise 0).
		/// </summary>
		/// <param name="p_sequence">
		/// The Sequence to pause.
		/// </param>
		/// <returns>
		/// The total number of paused Sequence (1 if the sequence existed, otherwise 0).
		/// </returns>
		static public int Pause( Sequence p_sequence ) { return DoFilteredIteration( p_sequence, DoFilteredPause, false ); }
		/// <summary>
		/// Pauses all Tweeners/Sequences, and returns the total number of paused Tweeners/Sequences.
		/// </summary>
		/// <returns>
		/// The total number of paused Tweeners/Sequences.
		/// </returns>
		static public int Pause() { return DoFilteredIteration( null, DoFilteredPause, false ); }
		
		/// <summary>
		/// Resumes all the tweens (delays included) for the given target, and returns the total number of resumed Tweeners.
		/// </summary>
		/// <param name="p_target">
		/// The target whose tweens to resume.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners.
		/// </returns>
		static public int Play( object p_target ) { return Play( p_target, false ); }
		/// <summary>
		/// Resumes all the tweens for the given target, and returns the total number of resumed Tweeners.
		/// </summary>
		/// <param name="p_target">
		/// The target whose tweens to resume.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial delay.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners.
		/// </returns>
		static public int Play( object p_target, bool p_skipDelay ) { return DoFilteredIteration( p_target, DoFilteredPlay, false, p_skipDelay ); }
		/// <summary>
		/// Resumes all the Tweeners (delays included) and Sequences with the given ID, and returns the total number of resumed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_id">
		/// The ID of the Tweeners/Sequences to resume.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners/Sequences.
		/// </returns>
		static public int Play( string p_id ) { return Play( p_id, false ); }
		/// <summary>
		/// Resumes all the Tweeners/Sequences with the given ID, and returns the total number of resumed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_id">
		/// The ID of the Tweeners/Sequences to resume.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial tween delay.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners/Sequences.
		/// </returns>
		static public int Play( string p_id, bool p_skipDelay ) { return DoFilteredIteration( p_id, DoFilteredPlay, false, p_skipDelay ); }
		/// <summary>
		/// Resumes all the Tweeners (delays included) and Sequences with the given intId, and returns the total number of resumed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_intId">
		/// The intId of the Tweeners/Sequences to resume.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners/Sequences.
		/// </returns>
		static public int Play( int p_intId ) { return Play( p_intId, false ); }
		/// <summary>
		/// Resumes all the Tweeners/Sequences with the given intId, and returns the total number of resumed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_intId">
		/// The intId of the Tweeners/Sequences to resume.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial tween delay.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners/Sequences.
		/// </returns>
		static public int Play( int p_intId, bool p_skipDelay ) { return DoFilteredIteration( p_intId, DoFilteredPlay, false, p_skipDelay ); }
		/// <summary>
		/// Resumes the given Tweener (delays included), and returns the total number of resumed ones (1 if the Tweener existed, otherwise 0).
		/// </summary>
		/// <param name="p_tweener">
		/// The Tweener to resume.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners (1 if the Tweener existed, otherwise 0).
		/// </returns>
		static public int Play( Tweener p_tweener ) { return Play( p_tweener, false ); }
		/// <summary>
		/// Resumes the given Tweener, and returns the total number of resumed ones (1 if the Tweener existed, otherwise 0).
		/// </summary>
		/// <param name="p_tweener">
		/// The Tweener to resume.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial delay.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners (1 if the Tweener existed, otherwise 0).
		/// </returns>
		static public int Play( Tweener p_tweener, bool p_skipDelay ) { return DoFilteredIteration( p_tweener, DoFilteredPlay, false, p_skipDelay ); }
		/// <summary>
		/// Resumes the given Sequence, and returns the total number of resumed ones (1 if the Sequence existed, otherwise 0).
		/// </summary>
		/// <param name="p_sequence">
		/// The Sequence to resume.
		/// </param>
		/// <returns>
		/// The total number of resumed Sequences (1 if the Sequence existed, otherwise 0).
		/// </returns>
		static public int Play( Sequence p_sequence ) { return DoFilteredIteration( p_sequence, DoFilteredPlay, false ); }
		/// <summary>
		/// Resumes all Tweeners (delays included) and Sequences, and returns the total number of resumed Tweeners/Sequences.
		/// </summary>
		/// <returns>
		/// The total number of resumed Tweeners/Sequences.
		/// </returns>
		static public int Play() { return Play( false ); }
		/// <summary>
		/// Resumes all Tweeners/Sequences, and returns the total number of resumed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial tween delay.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners/Sequences.
		/// </returns>
		static public int Play( bool p_skipDelay ) { return DoFilteredIteration( null, DoFilteredPlay, false, p_skipDelay ); }
		
		/// <summary>
		/// Resumes all the tweens (delays included) for the given target,
		/// sets the tweens so that they move forward and not backwards,
		/// and returns the total number of resumed Tweeners.
		/// </summary>
		/// <param name="p_target">
		/// The target whose tweens to resume.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners.
		/// </returns>
		static public int PlayForward( object p_target ) { return PlayForward( p_target, false ); }
		/// <summary>
		/// Resumes all the tweens for the given target,
		/// sets the tweens so that they move forward and not backwards,
		/// and returns the total number of resumed Tweeners.
		/// </summary>
		/// <param name="p_target">
		/// The target whose tweens to resume.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial delay.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners.
		/// </returns>
		static public int PlayForward( object p_target, bool p_skipDelay ) { return DoFilteredIteration( p_target, DoFilteredPlayForward, false, p_skipDelay ); }
		/// <summary>
		/// Resumes all the Tweeners (delays included) and Sequences with the given ID,
		/// sets the tweens so that they move forward and not backwards,
		/// and returns the total number of resumed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_id">
		/// The ID of the Tweeners/Sequences to resume.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners/Sequences.
		/// </returns>
		static public int PlayForward( string p_id ) { return PlayForward( p_id, false ); }
		/// <summary>
		/// Resumes all the Tweeners/Sequences with the given ID,
		/// sets the tweens so that they move forward and not backwards,
		/// and returns the total number of resumed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_id">
		/// The ID of the Tweeners/Sequences to resume.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial tween delay.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners/Sequences.
		/// </returns>
		static public int PlayForward( string p_id, bool p_skipDelay ) { return DoFilteredIteration( p_id, DoFilteredPlayForward, false, p_skipDelay ); }
		/// <summary>
		/// Resumes all the Tweeners (delays included) and Sequences with the given intId,
		/// sets the tweens so that they move forward and not backwards,
		/// and returns the total number of resumed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_intId">
		/// The intId of the Tweeners/Sequences to resume.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners/Sequences.
		/// </returns>
		static public int PlayForward( int p_intId ) { return PlayForward( p_intId, false ); }
		/// <summary>
		/// Resumes all the Tweeners/Sequences with the given intId,
		/// sets the tweens so that they move forward and not backwards,
		/// and returns the total number of resumed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_intId">
		/// The intId of the Tweeners/Sequences to resume.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial tween delay.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners/Sequences.
		/// </returns>
		static public int PlayForward( int p_intId, bool p_skipDelay ) { return DoFilteredIteration( p_intId, DoFilteredPlayForward, false, p_skipDelay ); }
		/// <summary>
		/// Resumes the given Tweener (delays included),
		/// sets it so that it moves forward and not backwards,
		/// and returns the total number of resumed ones (1 if the Tweener existed, otherwise 0).
		/// </summary>
		/// <param name="p_tweener">
		/// The Tweener to resume.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners (1 if the Tweener existed, otherwise 0).
		/// </returns>
		static public int PlayForward( Tweener p_tweener ) { return PlayForward( p_tweener, false ); }
		/// <summary>
		/// Resumes the given Tweener,
		/// sets it so that it moves forward and not backwards,
		/// and returns the total number of resumed ones (1 if the Tweener existed, otherwise 0).
		/// </summary>
		/// <param name="p_tweener">
		/// The Tweener to resume.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial delay.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners (1 if the Tweener existed, otherwise 0).
		/// </returns>
		static public int PlayForward( Tweener p_tweener, bool p_skipDelay ) { return DoFilteredIteration( p_tweener, DoFilteredPlayForward, false, p_skipDelay ); }
		/// <summary>
		/// Resumes the given Sequence,
		/// sets it so that it moves forward and not backwards,
		/// and returns the total number of resumed ones (1 if the Sequence existed, otherwise 0).
		/// </summary>
		/// <param name="p_sequence">
		/// The Sequence to resume.
		/// </param>
		/// <returns>
		/// The total number of resumed Sequences (1 if the Sequence existed, otherwise 0).
		/// </returns>
		static public int PlayForward( Sequence p_sequence ) { return DoFilteredIteration( p_sequence, DoFilteredPlayForward, false ); }
		/// <summary>
		/// Resumes all Tweeners (delays included) and Sequences,
		/// sets the tweens so that they move forward and not backwards,
		/// and returns the total number of resumed Tweeners/Sequences.
		/// </summary>
		/// <returns>
		/// The total number of resumed Tweeners/Sequences.
		/// </returns>
		static public int PlayForward() { return PlayForward( false ); }
		/// <summary>
		/// Resumes all Tweeners/Sequences,
		/// sets the tweens so that they move forward and not backwards,
		/// and returns the total number of resumed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial tween delay.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners/Sequences.
		/// </returns>
		static public int PlayForward( bool p_skipDelay ) { return DoFilteredIteration( null, DoFilteredPlayForward, false, p_skipDelay ); }
		
		/// <summary>
		/// Resumes all the tweens for the given target,
		/// sets the tweens so that they move backwards instead than forward,
		/// and returns the total number of resumed Tweeners.
		/// </summary>
		/// <param name="p_target">
		/// The target whose tweens to resume.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners.
		/// </returns>
		static public int PlayBackwards( object p_target ) { return DoFilteredIteration( p_target, DoFilteredPlayBackwards, false ); }
		/// <summary>
		/// Resumes all the Tweeners/Sequences with the given ID,
		/// sets the tweens so that they move backwards instead than forward,
		/// and returns the total number of resumed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_id">
		/// The ID of the Tweeners/Sequences to resume.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners/Sequences.
		/// </returns>
		static public int PlayBackwards( string p_id ) { return DoFilteredIteration( p_id, DoFilteredPlayBackwards, false ); }
		/// <summary>
		/// Resumes all the Tweeners/Sequences with the given intId,
		/// sets the tweens so that they move backwards instead than forward,
		/// and returns the total number of resumed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_intId">
		/// The intId of the Tweeners/Sequences to resume.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners/Sequences.
		/// </returns>
		static public int PlayBackwards( int p_intId ) { return DoFilteredIteration( p_intId, DoFilteredPlayBackwards, false ); }
		/// <summary>
		/// Resumes the given Tweener,
		/// sets it so that it moves backwards instead than forward,
		/// and returns the total number of resumed ones (1 if the Tweener existed, otherwise 0).
		/// </summary>
		/// <param name="p_tweener">
		/// The Tweener to resume.
		/// </param>
		/// <returns>
		/// The total number of resumed Tweeners (1 if the Tweener existed, otherwise 0).
		/// </returns>
		static public int PlayBackwards( Tweener p_tweener ) { return DoFilteredIteration( p_tweener, DoFilteredPlayBackwards, false ); }
		/// <summary>
		/// Resumes the given Sequence,
		/// sets it so that it moves backwards instead than forward,
		/// and returns the total number of resumed ones (1 if the Sequence existed, otherwise 0).
		/// </summary>
		/// <param name="p_sequence">
		/// The Sequence to resume.
		/// </param>
		/// <returns>
		/// The total number of resumed Sequences (1 if the Sequence existed, otherwise 0).
		/// </returns>
		static public int PlayBackwards( Sequence p_sequence ) { return DoFilteredIteration( p_sequence, DoFilteredPlayBackwards, false ); }
		/// <summary>
		/// Resumes all Tweeners/Sequences,
		/// sets the tweens so that they move backwards instead than forward,
		/// and returns the total number of resumed Tweeners/Sequences.
		/// </summary>
		/// <returns>
		/// The total number of resumed Tweeners/Sequences.
		/// </returns>
		static public int PlayBackwards() { return DoFilteredIteration( null, DoFilteredPlayBackwards, false ); }
		
		/// <summary>
		/// Rewinds all the tweens (delays included) for the given target, and returns the total number of rewinded Tweeners.
		/// </summary>
		/// <param name="p_target">
		/// The target whose tweens to rewind.
		/// </param>
		/// <returns>
		/// The total number of rewinded Tweeners.
		/// </returns>
		static public int Rewind( object p_target ) { return Rewind( p_target, false ); }
		/// <summary>
		/// Rewinds all the tweens for the given target, and returns the total number of rewinded Tweeners.
		/// </summary>
		/// <param name="p_target">
		/// The target whose tweens to rewind.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial delay.
		/// </param>
		/// <returns>
		/// The total number of rewinded Tweeners.
		/// </returns>
		static public int Rewind( object p_target, bool p_skipDelay ) { return DoFilteredIteration( p_target, DoFilteredRewind, false, p_skipDelay ); }
		/// <summary>
		/// Rewinds all the Tweeners (delays included) and Sequences with the given ID, and returns the total number of rewinded Tweeners/Sequences.
		/// </summary>
		/// <param name="p_id">
		/// The ID of the Tweeners/Sequences to rewind.
		/// </param>
		/// <returns>
		/// The total number of rewinded Tweeners/Sequences.
		/// </returns>
		static public int Rewind( string p_id ) { return Rewind( p_id, false ); }
		/// <summary>
		/// Rewinds all the Tweeners/Sequences with the given ID, and returns the total number of rewinded Tweeners/Sequences.
		/// </summary>
		/// <param name="p_id">
		/// The ID of the Tweeners/Sequences to rewind.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial tween delay.
		/// </param>
		/// <returns>
		/// The total number of rewinded Tweeners/Sequences.
		/// </returns>
		static public int Rewind( string p_id, bool p_skipDelay ) { return DoFilteredIteration( p_id, DoFilteredRewind, false, p_skipDelay ); }
		/// <summary>
		/// Rewinds all the Tweeners (delays included) and Sequences with the given intId, and returns the total number of rewinded Tweeners/Sequences.
		/// </summary>
		/// <param name="p_intId">
		/// The intId of the Tweeners/Sequences to rewind.
		/// </param>
		/// <returns>
		/// The total number of rewinded Tweeners/Sequences.
		/// </returns>
		static public int Rewind( int p_intId ) { return Rewind( p_intId, false ); }
		/// <summary>
		/// Rewinds all the Tweeners/Sequences with the given intId, and returns the total number of rewinded Tweeners/Sequences.
		/// </summary>
		/// <param name="p_intId">
		/// The intId of the Tweeners/Sequences to rewind.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial tween delay.
		/// </param>
		/// <returns>
		/// The total number of rewinded Tweeners/Sequences.
		/// </returns>
		static public int Rewind( int p_intId, bool p_skipDelay ) { return DoFilteredIteration( p_intId, DoFilteredRewind, false, p_skipDelay ); }
		/// <summary>
		/// Rewinds the given Tweener (delays included), and returns the total number of rewinded ones (1 if the Tweener existed, otherwise 0).
		/// </summary>
		/// <param name="p_tweener">
		/// The Tweener to rewind.
		/// </param>
		/// <returns>
		/// The total number of rewinded Tweeners (1 if the Tweener existed, otherwise 0).
		/// </returns>
		static public int Rewind( Tweener p_tweener ) { return Rewind( p_tweener, false ); }
		/// <summary>
		/// Rewinds the given Tweener, and returns the total number of rewinded ones (1 if the Tweener existed, otherwise 0).
		/// </summary>
		/// <param name="p_tweener">
		/// The Tweener to rewind.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial delay.
		/// </param>
		/// <returns>
		/// The total number of rewinded Tweeners (1 if the Tweener existed, otherwise 0).
		/// </returns>
		static public int Rewind( Tweener p_tweener, bool p_skipDelay ) { return DoFilteredIteration( p_tweener, DoFilteredRewind, false, p_skipDelay ); }
		/// <summary>
		/// Rewinds the given Sequence, and returns the total number of rewinded ones (1 if the Sequence existed, otherwise 0).
		/// </summary>
		/// <param name="p_sequence">
		/// The Sequence to rewind.
		/// </param>
		/// <returns>
		/// The total number of rewinded Sequences (1 if the Sequence existed, otherwise 0).
		/// </returns>
		static public int Rewind( Sequence p_sequence ) { return DoFilteredIteration( p_sequence, DoFilteredRewind, false ); }
		/// <summary>
		/// Rewinds all Tweeners (delay included) and Sequences, and returns the total number of rewinded Tweeners/Sequences.
		/// </summary>
		/// <returns>
		/// The total number of rewinded Tweeners/Sequences.
		/// </returns>
		static public int Rewind() { return Rewind( false ); }
		/// <summary>
		/// Rewinds all Tweeners/Sequences, and returns the total number of rewinded Tweeners/Sequences.
		/// </summary>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial tween delay.
		/// </param>
		/// <returns>
		/// The total number of rewinded Tweeners/Sequences.
		/// </returns>
		static public int Rewind( bool p_skipDelay ) { return DoFilteredIteration( null, DoFilteredRewind, false, p_skipDelay ); }
		
		/// <summary>
		/// Restarts all the tweens (delays included) for the given target, and returns the total number of restarted Tweeners.
		/// </summary>
		/// <param name="p_target">
		/// The target whose tweens to restart.
		/// </param>
		/// <returns>
		/// The total number of restarted Tweeners.
		/// </returns>
		static public int Restart( object p_target ) { return Restart( p_target, false ); }
		/// <summary>
		/// Restarts all the tweens for the given target, and returns the total number of restarted Tweeners.
		/// </summary>
		/// <param name="p_target">
		/// The target whose tweens to restart.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial delay.
		/// </param>
		/// <returns>
		/// The total number of restarted Tweeners.
		/// </returns>
		static public int Restart( object p_target, bool p_skipDelay ) { return DoFilteredIteration( p_target, DoFilteredRestart, false, p_skipDelay ); }
		/// <summary>
		/// Restarts all the Tweeners (delays included) and Sequences with the given ID, and returns the total number of restarted Tweeners/Sequences.
		/// </summary>
		/// <param name="p_id">
		/// The ID of the Tweeners/Sequences to restart.
		/// </param>
		/// <returns>
		/// The total number of restarted Tweeners/Sequences.
		/// </returns>
		static public int Restart( string p_id ) { return Restart( p_id, false ); }
		/// <summary>
		/// Restarts all the Tweeners/Sequences with the given ID, and returns the total number of restarted Tweeners/Sequences.
		/// </summary>
		/// <param name="p_id">
		/// The ID of the Tweeners/Sequences to restart.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial tween delay.
		/// </param>
		/// <returns>
		/// The total number of restarted Tweeners/Sequences.
		/// </returns>
		static public int Restart( string p_id, bool p_skipDelay ) { return DoFilteredIteration( p_id, DoFilteredRestart, false, p_skipDelay ); }
		/// <summary>
		/// Restarts all the Tweeners (delays included) and Sequences with the given intId, and returns the total number of restarted Tweeners/Sequences.
		/// </summary>
		/// <param name="p_intId">
		/// The intId of the Tweeners/Sequences to restart.
		/// </param>
		/// <returns>
		/// The total number of restarted Tweeners/Sequences.
		/// </returns>
		static public int Restart( int p_intId ) { return Restart( p_intId, false ); }
		/// <summary>
		/// Restarts all the Tweeners/Sequences with the given intId, and returns the total number of restarted Tweeners/Sequences.
		/// </summary>
		/// <param name="p_intId">
		/// The intId of the Tweeners/Sequences to restart.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial tween delay.
		/// </param>
		/// <returns>
		/// The total number of restarted Tweeners/Sequences.
		/// </returns>
		static public int Restart( int p_intId, bool p_skipDelay ) { return DoFilteredIteration( p_intId, DoFilteredRestart, false, p_skipDelay ); }
		/// <summary>
		/// Restarts the given Tweener (delays included), and returns the total number of restarted ones (1 if the Tweener existed, otherwise 0).
		/// </summary>
		/// <param name="p_tweener">
		/// The Tweener to restart.
		/// </param>
		/// <returns>
		/// The total number of restarted Tweeners (1 if the Tweener existed, otherwise 0).
		/// </returns>
		static public int Restart( Tweener p_tweener ) { return Restart( p_tweener, false ); }
		/// <summary>
		/// Restarts the given Tweener, and returns the total number of restarted ones (1 if the Tweener existed, otherwise 0).
		/// </summary>
		/// <param name="p_tweener">
		/// The Tweener to restart.
		/// </param>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial delay.
		/// </param>
		/// <returns>
		/// The total number of restarted Tweeners (1 if the Tweener existed, otherwise 0).
		/// </returns>
		static public int Restart( Tweener p_tweener, bool p_skipDelay ) { return DoFilteredIteration( p_tweener, DoFilteredRestart, false, p_skipDelay ); }
		/// <summary>
		/// Restarts the given Sequence, and returns the total number of restarted ones (1 if the Sequence existed, otherwise 0).
		/// </summary>
		/// <param name="p_sequence">
		/// The Sequence to restart.
		/// </param>
		/// <returns>
		/// The total number of restarted Sequences (1 if the Sequence existed, otherwise 0).
		/// </returns>
		static public int Restart( Sequence p_sequence ) { return DoFilteredIteration( p_sequence, DoFilteredRestart, false ); }
		/// <summary>
		/// Restarts all Tweeners (delay included) and Sequences, and returns the total number of restarted Tweeners/Sequences.
		/// </summary>
		/// <returns>
		/// The total number of restarted Tweeners/Sequences.
		/// </returns>
		static public int Restart() { return Restart( false ); }
		/// <summary>
		/// Restarts all Tweeners/Sequences and returns the total number of restarted Tweeners/Sequences.
		/// </summary>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial tween delay.
		/// </param>
		/// <returns>
		/// The total number of restarted Tweeners/Sequences.
		/// </returns>
		static public int Restart( bool p_skipDelay ) { return DoFilteredIteration( null, DoFilteredRestart, false, p_skipDelay ); }
		
		/// <summary>
		/// Reverses all the tweens for the given target,
		/// animating them from their current value back to the starting one,
		/// and returns the total number of reversed Tweeners.
		/// </summary>
		/// <param name="p_target">
		/// The target whose tweens to reverse.
		/// </param>
		/// <returns>
		/// The total number of reversed Tweeners.
		/// </returns>
		static public int Reverse( object p_target ) { return DoFilteredIteration( p_target, DoFilteredReverse, false ); }
		/// <summary>
		/// Reverses all the Tweeners/Sequences with the given ID,
		/// animating them from their current value back to the starting one,
		/// and returns the total number of reversed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_id">
		/// The ID of the Tweeners/Sequences to reverse.
		/// </param>
		/// <returns>
		/// The total number of reversed Tweeners/Sequences.
		/// </returns>
		static public int Reverse( string p_id ) { return DoFilteredIteration( p_id, DoFilteredReverse, false ); }
		/// <summary>
		/// Reverses all the Tweeners/Sequences with the given intId,
		/// animating them from their current value back to the starting one,
		/// and returns the total number of reversed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_intId">
		/// The intId of the Tweeners/Sequences to reverse.
		/// </param>
		/// <returns>
		/// The total number of reversed Tweeners/Sequences.
		/// </returns>
		static public int Reverse( int p_intId ) { return DoFilteredIteration( p_intId, DoFilteredReverse, false ); }
		/// <summary>
		/// Reverses the given Tweener,
		/// animating it from its current value back to the starting one,
		/// and returns the total number of reversed Tweeners (1 if the Tweener existed, otherwise 0).
		/// </summary>
		/// <param name="p_tweener">
		/// The Tweener to reverse.
		/// </param>
		/// <returns>
		/// The total number of reversed Tweeners (1 if the Tweener existed, otherwise 0).
		/// </returns>
		static public int Reverse( Tweener p_tweener ) { return DoFilteredIteration( p_tweener, DoFilteredReverse, false ); }
		/// <summary>
		/// Reverses the given Sequence, and returns the total number of reversed ones (1 if the Sequence existed, otherwise 0).
		/// </summary>
		/// <param name="p_sequence">
		/// The Sequence to reverse.
		/// </param>
		/// <returns>
		/// The total number of reversed Sequences (1 if the Sequence existed, otherwise 0).
		/// </returns>
		static public int Reverse( Sequence p_sequence ) { return DoFilteredIteration( p_sequence, DoFilteredReverse, false ); }
		/// <summary>
		/// Reverses all Tweeners/Sequences,
		/// animating them from their current value back to the starting one,
		/// and returns the total number of reversed Tweeners/Sequences.
		/// </summary>
		/// <returns>
		/// The total number of reversed Tweeners/Sequences.
		/// </returns>
		static public int Reverse() { return DoFilteredIteration( null, DoFilteredReverse, false ); }
		
		/// <summary>
		/// Completes all the tweens for the given target, and returns the total number of completed Tweeners.
		/// Where a loop was involved and not infinite, the relative tween completes at the position where it would actually be after the set number of loops.
		/// If there were infinite loops, this method will have no effect.
		/// </summary>
		/// <param name="p_target">
		/// The target whose tweens to complete.
		/// </param>
		/// <returns>
		/// The total number of completed Tweeners.
		/// </returns>
		static public int Complete( object p_target ) { return DoFilteredIteration( p_target, DoFilteredComplete, true ); }
		/// <summary>
		/// Completes all the Tweeners/Sequences with the given ID, and returns the total number of completed Tweeners/Sequences.
		/// Where a loop was involved and not infinite, the relative Tweener/Sequence completes at the position where it would actually be after the set number of loops.
		/// If there were infinite loops, this method will have no effect.
		/// </summary>
		/// <param name="p_id">
		/// The ID of the Tweeners/Sequences to complete.
		/// </param>
		/// <returns>
		/// The total number of completed Tweeners/Sequences.
		/// </returns>
		static public int Complete( string p_id ) { return DoFilteredIteration( p_id, DoFilteredComplete, true ); }
		/// <summary>
		/// Completes all the Tweeners/Sequences with the given intId, and returns the total number of completed Tweeners/Sequences.
		/// Where a loop was involved and not infinite, the relative Tweener/Sequence completes at the position where it would actually be after the set number of loops.
		/// If there were infinite loops, this method will have no effect.
		/// </summary>
		/// <param name="p_intId">
		/// The intId of the Tweeners/Sequences to complete.
		/// </param>
		/// <returns>
		/// The total number of completed Tweeners/Sequences.
		/// </returns>
		static public int Complete( int p_intId ) { return DoFilteredIteration( p_intId, DoFilteredComplete, true ); }
		/// <summary>
		/// Completes the given Tweener, and returns the total number of completed ones (1 if the Tweener existed, otherwise 0).
		/// Where a loop was involved and not infinite, the relative Tweener completes at the position where it would actually be after the set number of loops.
		/// If there were infinite loops, this method will have no effect.
		/// </summary>
		/// <param name="p_tweener">
		/// The Tweener to complete.
		/// </param>
		/// <returns>
		/// The total number of completed Tweeners (1 if the Tweener existed, otherwise 0).
		/// </returns>
		static public int Complete( Tweener p_tweener ) { return DoFilteredIteration( p_tweener, DoFilteredComplete, true ); }
		/// <summary>
		/// Completes the given Sequence, and returns the total number of completed ones (1 if the Sequence existed, otherwise 0).
		/// Where a loop was involved and not infinite, the relative Sequence completes at the position where it would actually be after the set number of loops.
		/// If there were infinite loops, this method will have no effect.
		/// </summary>
		/// <param name="p_sequence">
		/// The Sequence to complete.
		/// </param>
		/// <returns>
		/// The total number of completed Sequences (1 if the Sequence existed, otherwise 0).
		/// </returns>
		static public int Complete( Sequence p_sequence ) { return DoFilteredIteration( p_sequence, DoFilteredComplete, true ); }
		/// <summary>
		/// Completes all Tweeners/Sequences, and returns the total number of completed Tweeners/Sequences.
		/// Where a loop was involved and not infinite, the relative Tweener/Sequence completes at the position where it would actually be after the set number of loops.
		/// If there were infinite loops, this method will have no effect.
		/// </summary>
		/// <returns>
		/// The total number of completed Tweeners/Sequences.
		/// </returns>
		static public int Complete() { return DoFilteredIteration( null, DoFilteredComplete, true ); }
		
		/// <summary>
		/// Kills all the tweens for the given target (unless they're were created inside a <see cref="Sequence"/>),
		/// and returns the total number of killed Tweeners.
		/// </summary>
		/// <param name="p_target">
		/// The target whose Tweeners to kill.
		/// </param>
		/// <returns>
		/// The total number of killed Tweeners.
		/// </returns>
		static public int Kill( object p_target ) { return DoFilteredIteration( p_target, DoFilteredKill, true ); }
		/// <summary>
		/// Kills all the Tweeners/Sequences with the given ID, and returns the total number of killed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_id">
		/// The ID of the Tweeners/Sequences to kill.
		/// </param>
		/// <returns>
		/// The total number of killed Tweeners/Sequences.
		/// </returns>
		static public int Kill( string p_id ) { return DoFilteredIteration( p_id, DoFilteredKill, true ); }
		/// <summary>
		/// Kills all the Tweeners/Sequences with the given intId, and returns the total number of killed Tweeners/Sequences.
		/// </summary>
		/// <param name="p_intId">
		/// The intId of the Tweeners/Sequences to kill.
		/// </param>
		/// <returns>
		/// The total number of killed Tweeners/Sequences.
		/// </returns>
		static public int Kill( int p_intId ) { return DoFilteredIteration( p_intId, DoFilteredKill, true ); }
		/// <summary>
		/// Kills the given Tweener, and returns the total number of killed ones (1 if the Tweener existed, otherwise 0).
		/// </summary>
		/// <param name="p_tweener">
		/// The Tweener to kill.
		/// </param>
		/// <returns>
		/// The total number of killed Tweeners (1 if the Tweener existed, otherwise 0).
		/// </returns>
		static public int Kill( Tweener p_tweener ) { return DoFilteredIteration( p_tweener, DoFilteredKill, true ); }
		/// <summary>
		/// Kills the given Sequence, and returns the total number of killed ones (1 if the Sequence existed, otherwise 0).
		/// </summary>
		/// <param name="p_sequence">
		/// The Sequence to kill.
		/// </param>
		/// <returns>
		/// The total number of killed Sequences (1 if the Sequence existed, otherwise 0).
		/// </returns>
		static public int Kill( Sequence p_sequence ) { return DoFilteredIteration( p_sequence, DoFilteredKill, true ); }
		/// <summary>
		/// Kills all Tweeners/Sequences, and returns the total number of killed Tweeners/Sequences.
		/// </summary>
		/// <returns>
		/// The total number of killed Tweeners/Sequences.
		/// </returns>
		static public int Kill() { return DoFilteredIteration( null, DoFilteredKill, true ); }
		
		/// <summary>
		/// Returns <c>true</c> if the given target is currently involved in any running Tweener or Sequence (taking into account also nested tweens).
		/// Returns <c>false</c> both if the given target is not inside a Tweener, than if the relative Tweener is paused.
		/// To simply check if the target is attached to a Tweener or Sequence use <see cref="IsLinkedTo"/> instead.
		/// </summary>
		/// <param name="p_target">
		/// The target to check.
		/// </param>
		/// <returns>
		/// A value of <c>true</c> if the given target is currently involved in any running Tweener or Sequence (taking into account also nested tweens).
		/// </returns>
		static public bool IsTweening( object p_target )
		{
			if ( tweens == null )						return false;

			for ( int i = 0; i < tweens.Count; ++i ) {
				ABSTweenComponent tw = tweens[i];
				if ( tw.IsTweening( p_target ) )		return true;
			}

			return false;
		}
		
		/// <summary>
		/// Returns <c>true</c> if the given target is linked to any Tweener or Sequence (running or not, taking into account also nested tweens).
		/// </summary>
		/// <param name="p_target">
		/// The target to check.
		/// </param>
		/// <returns>
		/// A value of <c>true</c> if the given target is linked to any Tweener or Sequence (running or not, taking into account also nested tweens).
		/// </returns>
		static public bool IsLinkedTo( object p_target )
		{
			if ( tweens == null )						return false;

			for ( int i = 0; i < tweens.Count; ++i ) {
				ABSTweenComponent tw = tweens[i];
				if ( tw.IsLinkedTo( p_target ) )		return true;
			}

			return false;
		}
		
		// ===================================================================================
		// PRIVATE METHODS -------------------------------------------------------------------
		
		static private void DoUpdate( UpdateType p_updateType, float p_elapsed )
		{
			for ( int i = tweens.Count - 1; i > -1; --i ) {
				ABSTweenComponent tw = tweens[i];
				if ( tw.updateType == p_updateType && tw.Update( p_elapsed * tw.timeScale ) ) {
					// Tween complete...
					if ( tw.destroyed || tw.autoKillOnComplete ) {
						// ...autoKill: remove it.
						tweens[i].Kill( false );
						tweens.RemoveAt( i );
					}
				}
			}
			// Dispatch eventual onCompletes.
			if ( onCompletes.Count > 0 ) {
				for ( int i = 0; i < onCompletes.Count; ++i )		onCompletes[i].OnCompleteDispatch();
				onCompletes = new List<ABSTweenComponent>();
			}
		}
		
		static private void DoFilteredKill( int p_index, bool p_optionalBool )
		{
			tweens.RemoveAt( p_index );
		}
		
		static private void DoFilteredPause( int p_index, bool p_optionalBool )
		{
			tweens[p_index].Pause();
		}
		
		static private void DoFilteredPlay( int p_index, bool p_skipDelay )
		{
			ABSTweenComponent tw = tweens[p_index];
			if ( tw is Tweener )
				( tw as Tweener ).Play( p_skipDelay );
			else
				tw.Play();
		}
		
		static private void DoFilteredPlayForward( int p_index, bool p_skipDelay )
		{
			ABSTweenComponent tw = tweens[p_index];
			if ( tw is Tweener )
				( tw as Tweener ).PlayForward( p_skipDelay );
			else
				tw.PlayForward();
		}
		
		static private void DoFilteredPlayBackwards( int p_index, bool p_optionalBool )
		{
			ABSTweenComponent tw = tweens[p_index];
			if ( tw is Tweener )
				( tw as Tweener ).PlayBackwards();
			else
				tw.PlayBackwards();
		}
		
		static private void DoFilteredRewind( int p_index, bool p_skipDelay )
		{
			ABSTweenComponent tw = tweens[p_index];
			if ( tw is Tweener ) {
				( tw as Tweener ).Rewind( p_skipDelay );
			} else
				tw.Rewind();
		}
		
		static private void DoFilteredRestart( int p_index, bool p_skipDelay )
		{
			ABSTweenComponent tw = tweens[p_index];
			if ( tw is Tweener )
				( tw as Tweener ).Restart( p_skipDelay );
			else
				tw.Restart();
		}
		
		static private void DoFilteredReverse( int p_index, bool p_optionalBool )
		{
			tweens[p_index].Reverse();
		}
		
		static private void DoFilteredComplete( int p_index, bool p_optionalBool )
		{
			tweens[p_index].Complete( false );
		}
		
		static private void AddTween( ABSTweenComponent p_tween )
		{
			if ( tweenGOInstance == null )			NewTweenInstance();
			if ( tweens == null ) {
				tweens = new List<ABSTweenComponent>();
				it.StartCoroutines();
			}
			tweens.Add( p_tween );
			SetGOName();
		}
		
		static private void NewTweenInstance()
		{
			tweenGOInstance = new GameObject( GAMEOBJNAME );
			it = tweenGOInstance.AddComponent<HOTween>();
			DontDestroyOnLoad( tweenGOInstance );
		}
		
		private void StartCoroutines()
		{
			time = Time.realtimeSinceStartup;
			StartCoroutine( StartCoroutines_StartTimeScaleIndependentUpdate() );
		}
		private IEnumerator StartCoroutines_StartTimeScaleIndependentUpdate()
		{
			yield return null;
			
			StartCoroutine( TimeScaleIndependentUpdate() );
			
			yield break;
		}
		
		static private void SetGOName()
		{
			if ( !isEditor || !renameInstToCountTw )		return;
			tweenGOInstance.name = GAMEOBJNAME + " : " + totTweens;
		}
		
		static private bool CheckClear()
		{
			if ( tweens == null || tweens.Count == 0 ) {
				Clear();
				if ( isPermanent )	SetGOName();
				return true;
			}
			
			SetGOName();
			return false;
		}
		
		static private void Clear()
		{
			if ( it != null )						it.StopAllCoroutines();
			
			tweens = null;
			
			if ( !isPermanent ) {
				if ( tweenGOInstance != null )		Destroy( tweenGOInstance );
				tweenGOInstance = null;
				it = null;
			}
		}
		
		// ===================================================================================
		// HELPERS ---------------------------------------------------------------------------
		
		/// <summary>
		/// Filter filters for:
		/// - ID if <see cref="string"/>
		/// - Tweener if <see cref="Tweener"/>
		/// - Sequence if <see cref="Sequence"/>
		/// - Tweener target if <see cref="object"/> (doesn't look inside sequence tweens)
		/// - Everything if null
		/// </summary>
		static private int DoFilteredIteration( object p_filter, TweenDelegate.FilterFunc p_operation, bool p_collectionChanger ) { return DoFilteredIteration( p_filter, p_operation, p_collectionChanger, false ); }
		static private int DoFilteredIteration( object p_filter, TweenDelegate.FilterFunc p_operation, bool p_collectionChanger, bool p_optionalBool )
		{
			if ( tweens == null )				return 0;
			
			int opCount = 0;
			
			if ( p_filter == null ) {
				// All
				for ( int i = tweens.Count - 1; i > -1; --i ) {
					p_operation( i, p_optionalBool );
					++opCount;
				}
			} else if ( p_filter is int ) {
				// Int ID
				int f = (int) p_filter;
				for ( int i = tweens.Count - 1; i > -1; --i ) {
					if ( tweens[i].intId == f ) {
						p_operation( i, p_optionalBool );
						++opCount;
					}
				}
			} else if ( p_filter is string ) {
				// ID
				string f = (string) p_filter;
				for ( int i = tweens.Count - 1; i > -1; --i ) {
					if ( tweens[i].id == f ) {
						p_operation( i, p_optionalBool );
						++opCount;
					}
				}
			} else if ( p_filter is Tweener ) {
				// Tweener
				Tweener f = p_filter as Tweener;
				for ( int i = tweens.Count - 1; i > -1; --i ) {
					if ( tweens[i] == f ) {
						p_operation( i, p_optionalBool );
						++opCount;
					}
				}
			} else if ( p_filter is Sequence ) {
				// Sequence
				Sequence f = p_filter as Sequence;
				for ( int i = tweens.Count - 1; i > -1; --i ) {
					if ( tweens[i] == f ) {
						p_operation( i, p_optionalBool );
						++opCount;
					}
				}
			} else {
				// Target
				ABSTweenComponent tw;
				for ( int i = tweens.Count - 1; i > -1; --i ) {
					tw = tweens[i];
					if ( tw is Tweener && ( tw as Tweener ).target == p_filter ) {
						p_operation( i, p_optionalBool );
						++opCount;
					}
				}
			}
			
			if ( p_collectionChanger )			CheckClear();
			
			return opCount;
		}
		
		/// <summary>
		/// Returns all the currently existing plugins involved in any tween, even if nested or paused,
		/// or <c>null</c> if there are none.
		/// </summary>
		static private List<ABSTweenPlugin> GetPlugins()
		{
			if ( tweens == null )			return null;
			
			List<ABSTweenPlugin> plugs = new List<ABSTweenPlugin>();
			for ( int i = 0; i < tweens.Count; ++i )
				tweens[i].FillPluginsList( plugs );
			
			return plugs;
		}
	}
}

