// 
// Tweener.cs
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

using UnityEngine;
using System.Collections.Generic;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;
using Holoville.HOTween.Plugins.Core;

namespace Holoville.HOTween
{
	/// <summary>
	/// Tween component, created by HOTween for each separate tween.
	/// <para>Author: Daniele Giardini (http://www.holoville.com)</para>
	/// </summary>
	public class Tweener : ABSTweenComponent
	{
		// VARS ///////////////////////////////////////////////////
		
		private		float						_elapsedDelay = 0;
		
		internal	EaseType					_easeType = HOTween.defEaseType;
		
		internal	bool						_speedBased = false;
		internal	float						_delay = 0;
		
		internal	bool						isFrom = false; // Indicates whether this is a FROM or a TO tween.
		internal	float						delayCount = 0;
		
		// REFERENCES /////////////////////////////////////////////
		
		internal	List<ABSTweenPlugin>		plugins;
		
		private		object						_target;
		
		// GETS/SETS //////////////////////////////////////////////
		
		/// <summary>
		/// Ease type of this tweener
		/// (consider that the plugins you have set might have different ease types).
		/// Setting it will change the ease of all the plugins used by this tweener.
		/// </summary>
		public		EaseType					easeType
		{
			get { return _easeType; }
			set {
				_easeType = value;
				// Change ease type of all existing plugins.
				for ( int i = 0; i < plugins.Count; ++i )		plugins[i].SetEase( _easeType );
			}
		}
		
		// READ-ONLY GETS /////////////////////////////////////////
		
		/// <summary>
		/// Target of this tween.
		/// </summary>
		public		object						target
		{
			get { return _target; }
		}
		/// <summary>
		/// <c>true</c> if this tween is animated by speed instead than by duration.
		/// </summary>
		public		bool						speedBased
		{
			get { return _speedBased; }
		}
		/// <summary>
		/// The delay that was set for this tween.
		/// </summary>
		public		float						delay
		{
			get { return _delay; }
		}
		/// <summary>
		/// The currently elapsed delay time.
		/// </summary>
		public		float						elapsedDelay
		{
			get { return _elapsedDelay; }
		}
		
		
		// ***********************************************************************************
		// CONSTRUCTOR + PARMS CREATOR
		// ***********************************************************************************
		
		/// <summary>
		/// Called by HOTween each time a new tween is generated via <c>To</c> or similar methods.
		/// </summary>
		internal Tweener( object p_target, float p_duration, TweenParms p_parms )
		{
			_target = p_target;
			_duration = p_duration;
			
			p_parms.InitializeObject( this, _target );
			
			if ( plugins != null && plugins.Count > 0 ) {
				// Valid plugins were added: mark this as not empty anymore.
				_isEmpty = false;
			}
			
			SetFullDuration();
		}
		
		// ===================================================================================
		// METHODS ---------------------------------------------------------------------------
		
		/// <summary>
		/// Kills this Tweener and cleans it.
		/// </summary>
		/// <param name="p_autoRemoveFromHOTween">
		/// If <c>true</c> also calls <c>HOTween.Kill(this)</c> to remove it from HOTween.
		/// Set internally to <c>false</c> when I already know that HOTween is going to remove it.
		/// </param>
		override internal void Kill( bool p_autoRemoveFromHOTween )
		{
			if ( _destroyed )					return;
			
			plugins = null;
			_target = null;
			
			base.Kill( p_autoRemoveFromHOTween );
		}
		
		/// <summary>
		/// Resumes this Tweener.
		/// </summary>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial delay.
		/// </param>
		public void Play( bool p_skipDelay )
		{
			if ( !_enabled )			return;
			if ( p_skipDelay )			SkipDelay();
			base.Play();
		}
		
		/// <summary>
		/// Resumes this Tweener and plays it forward.
		/// </summary>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial delay.
		/// </param>
		public void PlayForward( bool p_skipDelay )
		{
			if ( !_enabled )			return;
			if ( p_skipDelay )			SkipDelay();
			base.PlayForward();
		}
		
		/// <summary>
		/// Rewinds this Tweener (loops and tween delay included), and pauses it.
		/// </summary>
		override public void Rewind() { Rewind( false ); }
		/// <summary>
		/// Rewinds this Tweener (loops included), and pauses it.
		/// </summary>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial delay.
		/// </param>
		public void Rewind( bool p_skipDelay ) { Rewind( false, p_skipDelay ); }
		
		/// <summary>
		/// Restarts this Tweener from the beginning (loops and tween delay included).
		/// </summary>
		override public void Restart() { Restart( false ); }
		/// <summary>
		/// Restarts this Tweener from the beginning (loops and tween delay included).
		/// </summary>
		/// <param name="p_skipDelay">
		/// If <c>true</c> skips any initial delay.
		/// </param>
		public void Restart( bool p_skipDelay ) { Rewind( true, p_skipDelay ); }
		
		/// <summary>
		/// Completes this Tweener.
		/// Where a loop was involved, the Tweener completes at the position where it would actually be after the set number of loops.
		/// If there were infinite loops, this method will have no effect.
		/// </summary>
		override internal void Complete( bool p_autoRemoveFromHOTween )
		{
			if ( !_enabled )				return;
			if ( _loops < 0 )				return;
			
			_fullElapsed = ( _fullDuration == Mathf.Infinity ? _duration : _fullDuration );
			Update( 0, true );
			if ( _autoKillOnComplete )		Kill( p_autoRemoveFromHOTween );
		}
		
		/// <summary>
		/// Returns <c>true</c> if the given target and this Tweener target are the same, and the Tweener is running.
		/// Returns <c>false</c> both if the given target is not the same as this Tweener's, than if this Tweener is paused.
		/// This method is here to uniform <see cref="Tweener"/> with <see cref="Sequence"/>.
		/// </summary>
		/// <param name="p_target">
		/// The target to check.
		/// </param>
		/// <returns>
		/// A value of <c>true</c> if the given target and this Tweener target are the same, and this Tweener is running.
		/// </returns>
		override public bool IsTweening( object p_target )
		{
			if ( !_enabled )				return false;
			if ( p_target == _target )		return !_isPaused;
			return false;
		}
		
		/// <summary>
		/// Returns <c>true</c> if the given target and this Tweener target are the same.
		/// This method is here to uniform <see cref="Tweener"/> with <see cref="Sequence"/>.
		/// </summary>
		/// <param name="p_target">
		/// The target to check.
		/// </param>
		/// <returns>
		/// A value of <c>true</c> if the given target and this Tweener target are the same.
		/// </returns>
		override public bool IsLinkedTo( object p_target )
		{
			return ( p_target == _target );
		}
		
		/// <summary>
		/// If this Tweener contains a <see cref="PlugVector3Path"/> tween,
		/// returns a point on the path at the given percentage (0 to 1).
		/// Returns a <c>zero Vector</c> if there's no path tween associated with this tween.
		/// Note that, if the tween wasn't started, the OnStart callback will be called
		/// the first time you call this method, because the tween needs to be initialized.
		/// </summary>
		/// <param name="t">
		/// The percentage (0 to 1) at which to get the point.
		/// </param>
		public Vector3 GetPointOnPath( float t )
		{
			if ( plugins == null )			return Vector3.zero;
			
			foreach ( ABSTweenPlugin plug in plugins ) {
				if ( plug is PlugVector3Path ) {
					if ( !hasStarted )		OnStart(); // Startup the tween to get the path data.
					return ( plug as PlugVector3Path ).GetConstPointOnPath( t );
				}
			}
			
			return Vector3.zero;
		}
		
		// ===================================================================================
		// INTERNAL METHODS ------------------------------------------------------------------
		
		/// <summary>
		/// Updates the Tweener by the given elapsed time,
		/// and returns a value of <c>true</c> if the Tweener is complete.
		/// </summary>
		/// <param name="p_shortElapsed">
		/// The elapsed time since the last update.
		/// </param>
		/// <param name="p_forceUpdate">
		/// If <c>true</c> forces the update even if the Tweener is complete or paused,
		/// but ignores onUpdate, and sends onComplete and onStepComplete calls only if the Tweener wasn't complete before this call.
		/// </param>
		/// <param name="p_isStartupIteration">
		/// If <c>true</c> means the update is due to a startup iteration (managed by Sequence OnStart or HOTween.From),
		/// and all callbacks will be ignored (except onStart).
		/// </param>
		/// <returns>
		/// A value of <c>true</c> if the Tweener is not reversed and is complete (or the tween target doesn't exist anymore), otherwise <c>false</c>.
		/// </returns>
		override internal bool Update( float p_shortElapsed, bool p_forceUpdate, bool p_isStartupIteration )
		{
			if ( _destroyed )											return true;
			if ( _target == null || _target.Equals( null ) ) {
				Kill( false );
				return true;
			}
			if ( !_enabled )											return false;
			if ( _isComplete && !_isReversed && !p_forceUpdate )		return true;
			if ( _fullElapsed == 0 && _isReversed && !p_forceUpdate )	return false;
			if ( _isPaused && !p_forceUpdate )							return false;
			
			if ( delayCount == 0 ) {
				if ( !_hasStarted )									OnStart();
				if ( !_isReversed ) {
					_fullElapsed += p_shortElapsed;
					_elapsed += p_shortElapsed;
				} else {
					_fullElapsed -= p_shortElapsed;
					_elapsed -= p_shortElapsed;
				}
				if ( _fullElapsed > _fullDuration )
					_fullElapsed = _fullDuration;
				else if ( _fullElapsed < 0 )
					_fullElapsed = 0;
			} else {
				// Manage delay (delay doesn't go backwards).
				if ( _timeScale != 0 )	_elapsedDelay += p_shortElapsed / _timeScale; // Calculate delay independently of timeScale
				if ( _elapsedDelay < delayCount ) {
					return false;
				} else {
					if ( _isReversed ) {
						_fullElapsed = _elapsed = 0;
					} else {
						_fullElapsed = _elapsed = _elapsedDelay - delayCount;
						if ( _fullElapsed > _fullDuration )		_fullElapsed = _fullDuration;
					}
					_elapsedDelay = delayCount;
					delayCount = 0;
					if ( !_hasStarted )								OnStart();
				}
			}
			
			ignoreCallbacks = p_isStartupIteration;
			
			// Set all elapsed and loops values.
			bool wasComplete = _isComplete;
			bool stepComplete = ( !_isReversed && !wasComplete && _elapsed >= _duration );
			SetLoops();
			SetElapsed();
			_isComplete = ( !_isReversed && _loops >= 0 && _completedLoops >= _loops );
			bool complete = ( !wasComplete && _isComplete ? true : false );
			
			// Update the plugins.
			float plugElapsed = ( !_isLoopingBack ? _elapsed : _duration - _elapsed );
			ABSTweenPlugin plug;
			for ( int i = 0; i < plugins.Count; ++i ) {
				plug = plugins[i];
				if ( !_isLoopingBack && plug.easeReversed || _isLoopingBack && _loopType == LoopType.YoyoInverse && !plug.easeReversed ) {
					plug.ReverseEase();
				}
				plug.Update( plugElapsed );
			}
			
			// Manage eventual pause, complete, update, rewinded, and stepComplete.
			if ( _fullElapsed != prevFullElapsed ) {
				OnUpdate();
				if ( _fullElapsed == 0 )		OnRewinded();
			}
			if ( complete ) {
				OnComplete();
			} else if ( stepComplete ) {
				OnStepComplete();
			}
			
			ignoreCallbacks = false;
			prevFullElapsed = _fullElapsed;
			
			return complete;
		}
		
		/// <summary>
		/// Sets the correct values in case of Incremental loop type.
		/// Also called by Tweener.ApplySequenceIncrement (used by Sequences during Incremental loops).
		/// </summary>
		/// <param name="p_diffIncr">
		/// The difference from the previous loop increment.
		/// </param>
		override internal void SetIncremental( int p_diffIncr )
		{
			if ( plugins == null )		return;
			for ( int i = 0; i < plugins.Count; ++i )		plugins[i].SetIncremental( p_diffIncr );
		}
		
		/// <summary>
		/// If speed based duration was not already set (meaning OnStart has not yet been called),
		/// calculates the duration and then resets the tween so that OnStart can be called from scratch.
		/// Used by Sequences when Appending/Prepending/Inserting speed based tweens.
		/// </summary>
		internal void ForceSetSpeedBasedDuration()
		{
			if ( !_speedBased || plugins == null )			return;
			
			for ( int i = 0; i < plugins.Count; ++i )		plugins[i].ForceSetSpeedBasedDuration();
			_duration = 0;
			foreach ( ABSTweenPlugin plug in plugins ) {
				if ( plug.duration > _duration )			_duration = plug.duration;
			}
			SetFullDuration();
		}
		
		// ===================================================================================
		// PRIVATE METHODS -------------------------------------------------------------------
		
		/// <summary>
		/// Sends the tween to the given time (taking also loops into account) and eventually plays it.
		/// If the time is bigger than the total tween duration, it goes to the end.
		/// </summary>
		/// <returns>
		/// Returns <c>true</c> if the tween reached its end and was completed.
		/// </returns>
		override protected bool GoTo( float p_time, bool p_play, bool p_forceUpdate )
		{
			if ( !_enabled )									return false;
			
			if ( p_time > _fullDuration )
				p_time = _fullDuration;
			else if ( p_time < 0 )
				p_time = 0;
			if ( !p_forceUpdate && _fullElapsed == p_time )		return _isComplete;
			
			_fullElapsed = p_time;
			delayCount = 0;
			_elapsedDelay = _delay;
			Update( 0, true );
			if ( !_isComplete && p_play )		Play();
			
			return _isComplete;
		}
		
		private void Rewind( bool p_play, bool p_skipDelay )
		{
			if ( !_enabled )					return;
			
			if ( !_hasStarted )					OnStart();
			
			_isComplete = false;
			_isLoopingBack = false;
			delayCount = ( p_skipDelay ? 0 : _delay );
			_elapsedDelay = ( p_skipDelay ? _delay : 0 );
			_completedLoops = 0;
			_fullElapsed = _elapsed = 0;
			
			ABSTweenPlugin plug;
			for ( int i = 0; i < plugins.Count; ++i ) {
				plug = plugins[i];
				if ( plug.easeReversed )		plug.ReverseEase();
				plug.Rewind();
			}
			
			// Manage OnUpdate and OnRewinded.
			if ( _fullElapsed != prevFullElapsed ) {
				OnUpdate();
				if ( _fullElapsed == 0 )		OnRewinded();
			}
			prevFullElapsed = _fullElapsed;
			
			if ( p_play ) Play(); else Pause();
		}
		
		private void SkipDelay()
		{
			if ( delayCount > 0 ) {
				delayCount = 0;
				_elapsedDelay = _delay;
				_elapsed = _fullElapsed = 0;
			}
		}
		
		/// <summary>
		/// Manages on first start behaviour.
		/// </summary>
		override protected void OnStart()
		{
			for ( int i = 0; i < plugins.Count; ++i )		plugins[i].Startup();
			if ( _speedBased ) {
				// Reset duration based on value changes and speed.
				// Can't be done sooner because it needs to startup the plugins first.
				_duration = 0;
				foreach ( ABSTweenPlugin plug in plugins ) {
					if ( plug.duration > _duration )		_duration = plug.duration;
				}
				SetFullDuration();
			}
			base.OnStart();
		}
		
		// ===================================================================================
		// HELPERS ---------------------------------------------------------------------------
		
		/// <summary>
		/// Fills the given list with all the plugins inside this tween.
		/// Used by <c>HOTween.GetPlugins</c>.
		/// </summary>
		override internal void FillPluginsList( List<ABSTweenPlugin> p_plugs )
		{
			if ( plugins == null )				return;
			
			for ( int i = 0; i < plugins.Count; ++i )		p_plugs.Add( plugins[i] );
		}
	}
}

