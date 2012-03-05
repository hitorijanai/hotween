// 
// Sequence.cs
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
using Holoville.HOTween.Plugins.Core;

namespace Holoville.HOTween
{
	/// <summary>
	/// Sequence component. Manager for sequence of Tweeners or other nested Sequences.
	/// <para>Author: Daniele Giardini (http://www.holoville.com)</para>
	/// </summary>
	public class Sequence : ABSTweenComponent
	{
		// VARS ///////////////////////////////////////////////////

		private		int						prevCompletedLoops = 0; // Stored only during Incremental loop type.
		
		// REFERENCES /////////////////////////////////////////////
		
		private		List<HOTSeqItem>		items;
		
		
		// ***********************************************************************************
		// CONSTRUCTOR
		// ***********************************************************************************
		
		/// <summary>
		/// Creates a new Sequence.
		/// </summary>
		/// <param name="p_parms">
		/// A <see cref="SequenceParms"/> representing the Sequence parameters.
		/// You can pass an existing one, or create a new one inline via method chaining,
		/// like <c>new SequenceParms().Id("sequence1").Loops(2).OnComplete(myFunction)</c>
		/// </param>
		public Sequence ( SequenceParms p_parms )
		{
			p_parms.InitializeSequence( this );
			
			// Automatically pause the sequence.
			_isPaused = true;
			
			// Add this sequence to HOTWeen tweens.
			HOTween.AddSequence( this );
		}
		
		// ===================================================================================
		// SEQUENCE METHODS ------------------------------------------------------------------
		
		/// <summary>
		/// Appends an interval to the right of the sequence,
		/// and returns the new Sequence total time length (loops excluded).
		/// </summary>
		/// <param name="p_duration">
		/// The duration of the interval.
		/// </param>
		/// <returns>
		/// The new Sequence total time length (loops excluded).
		/// </returns>
		public float AppendInterval( float p_duration ) { return Append( null, p_duration ); }
		/// <summary>
		/// Adds the given <see cref="IHOTweenComponent"/> to the right of the sequence,
		/// and returns the new Sequence total time length (loops excluded).
		/// </summary>
		/// <param name="p_twMember">
		/// The <see cref="IHOTweenComponent"/> to append.
		/// </param>
		/// <returns>
		/// The new Sequence total time length (loops excluded).
		/// </returns>
		public float Append( IHOTweenComponent p_twMember ) { return Append( p_twMember, 0 ); }
		private float Append( IHOTweenComponent p_twMember, float p_duration )
		{
			if ( items == null )							return ( p_twMember != null ? Insert( 0, p_twMember ) : Insert( 0, null, p_duration ) );
			
			if ( p_twMember != null )						HOTween.Kill( p_twMember );
			
			HOTSeqItem newItem = ( p_twMember != null ? new HOTSeqItem( _duration, p_twMember as ABSTweenComponent ) : new HOTSeqItem( _duration, p_duration ) );
			items.Add( newItem );
			_duration += newItem.duration;
			
			SetFullDuration();
			return _duration;
		}
		
		/// <summary>
		/// Prepends an interval to the left of the sequence,
		/// and returns the new Sequence total time length (loops excluded).
		/// </summary>
		/// <param name="p_duration">
		/// The duration of the interval.
		/// </param>
		/// <returns>
		/// The new Sequence total time length (loops excluded).
		/// </returns>
		public float PrependInterval( float p_duration ) { return Prepend( null, p_duration ); }
		/// <summary>
		/// Adds the given <see cref="IHOTweenComponent"/> to the left of the sequence,
		/// moving all the existing sequence elements to the right,
		/// and returns the new Sequence total time length (loops excluded).
		/// </summary>
		/// <param name="p_twMember">
		/// The <see cref="IHOTweenComponent"/> to prepend.
		/// </param>
		/// <returns>
		/// The new Sequence total time length (loops excluded).
		/// </returns>
		public float Prepend( IHOTweenComponent p_twMember ) { return Prepend( p_twMember, 0 ); }
		private float Prepend( IHOTweenComponent p_twMember, float p_duration )
		{
			if ( items == null )							return Insert( 0, p_twMember );
			
			if ( p_twMember != null )						HOTween.Kill( p_twMember );
			
			HOTSeqItem newItem = ( p_twMember != null ? new HOTSeqItem( 0, p_twMember as ABSTweenComponent ) : new HOTSeqItem( 0, p_duration ) );
			float itemDur = newItem.duration;
			for ( int i = 0; i < items.Count; ++i )		items[i].startTime += itemDur;
			items.Insert( 0, newItem );
			_duration += itemDur;
			
			SetFullDuration();
			return _duration;
		}
		
		/// <summary>
		/// Inserts the given <see cref="IHOTweenComponent"/> at the given time,
		/// and returns the new Sequence total time length (loops excluded).
		/// </summary>
		/// <param name="p_time">
		/// The time at which the element must be placed.
		/// </param>
		/// <param name="p_twMember">
		/// The <see cref="IHOTweenComponent"/> to insert.
		/// </param>
		/// <returns>
		/// The new Sequence total time length (loops excluded).
		/// </returns>
		public float Insert( float p_time, IHOTweenComponent p_twMember ) { return Insert( p_time, p_twMember, 0 ); }
		private float Insert( float p_time, IHOTweenComponent p_twMember, float p_duration )
		{
			if ( p_twMember != null )						HOTween.Kill( p_twMember );
			
			HOTSeqItem newItem = ( p_twMember != null ? new HOTSeqItem( p_time, p_twMember as ABSTweenComponent ) : new HOTSeqItem( p_time, p_duration ) );
			
			if ( items == null ) {
				items = new List<HOTSeqItem>();
				items.Add( newItem );
				_duration = newItem.startTime + newItem.duration;
				SetFullDuration();
				return _duration;
			}
			
			bool placed = false;
			for ( int i = 0; i < items.Count; ++i ) {
				if ( items[i].startTime >= p_time ) {
					items.Insert( i, newItem );
					placed = true;
					break;
				}
			}
			if ( !placed )		items.Add( newItem );
			_duration = Mathf.Max( newItem.startTime + newItem.duration, _duration );
			
			SetFullDuration();
			return _duration;
		}
		
		// ===================================================================================
		// METHODS ---------------------------------------------------------------------------
		
		/// <summary>
		/// Kills this Sequence and cleans it.
		/// </summary>
		/// <param name="p_autoRemoveFromHOTween">
		/// If <c>true</c> also calls <c>HOTween.Kill(this)</c> to remove it from HOTween.
		/// Set internally to <c>false</c> when I already know that HOTween is going to remove it.
		/// </param>
		override internal void Kill( bool p_autoRemoveFromHOTween )
		{
			if ( _destroyed )					return;
			
			items = null;
			
			base.Kill( p_autoRemoveFromHOTween );
		}
		
		/// <summary>
		/// Rewinds this Sequence (loops included), and pauses it.
		/// </summary>
		override public void Rewind() { Rewind( false ); }
		
		/// <summary>
		/// Restarts this Sequence from the beginning (loops included).
		/// </summary>
		override public void Restart() { Rewind( true ); }
		
		/// <summary>
		/// Returns <c>true</c> if the given target is currently involved in a running tween of this Sequence (taking into account also nested tweens).
		/// Returns <c>false</c> both if the given target is not inside any of this Sequence tweens, than if the relative tween is paused.
		/// To simply check if the target is attached to a tween of this Sequence, use <c>IsLinkedTo( target )</c> instead.
		/// </summary>
		/// <param name="p_target">
		/// The target to check.
		/// </param>
		/// <returns>
		/// A value of <c>true</c> if the given target is currently involved in a running tween of this Sequence (taking into account also nested tweens).
		/// </returns>
		override public bool IsTweening( object p_target )
		{
			if ( !_enabled || items == null )		return false;
			
			HOTSeqItem item;
			for ( int i = 0; i < items.Count; ++i ) {
				item = items[i];
				if ( item.twMember != null && item.twMember.IsTweening( p_target ) )
					return true;
			}
			
			return false;
		}
		
		/// <summary>
		/// Returns <c>true</c> if the given target is linked to a tween of this Sequence (running or not, taking into account also nested tweens).
		/// </summary>
		/// <param name="p_target">
		/// The target to check.
		/// </param>
		/// <returns>
		/// A value of <c>true</c> if the given target is linked to a tween of this Sequence (running or not, taking into account also nested tweens).
		/// </returns>
		override public bool IsLinkedTo( object p_target )
		{
			if ( items == null )			return false;
			
			HOTSeqItem item;
			for ( int i = 0; i < items.Count; ++i ) {
				item = items[i];
				if ( item.twMember != null && item.twMember.IsLinkedTo( p_target ) )
					return true;
			}
			
			return false;
		}
		
		// ===================================================================================
		// INTERNAL METHODS ------------------------------------------------------------------
		
		/// <summary>
		/// Completes this Sequence.
		/// Where a loop was involved, the Sequence completes at the position where it would actually be after the set number of loops.
		/// If there were infinite loops, this method will have no effect.
		/// </summary>
		override internal void Complete( bool p_autoRemoveFromHOTween )
		{
			if ( !_enabled )						return;
			if ( items == null || _loops < 0 )		return;
			
			_fullElapsed = _fullDuration;
			Update( 0, true );
			if ( _autoKillOnComplete )				Kill( p_autoRemoveFromHOTween );
		}
		
		/// <summary>
		/// Updates the Sequence by the given elapsed time,
		/// and returns a value of <c>true</c> if the Sequence is complete.
		/// </summary>
		/// <param name="p_shortElapsed">
		/// The elapsed time since the last update.
		/// </param>
		/// <param name="p_forceUpdate">
		/// If <c>true</c> forces the update even if the Sequence is complete or paused,
		/// but ignores onUpdate, and sends onComplete and onStepComplete calls only if the Sequence wasn't complete before this call.
		/// </param>
		/// <param name="p_isStartupIteration">
		/// If <c>true</c> means the update is due to a startup iteration (managed by Sequence OnStart),
		/// and all callbacks will be ignored (except onStart).
		/// </param>
		/// <returns>
		/// A value of <c>true</c> if the Sequence is not reversed and is complete (or all the Sequence tween targets don't exist anymore), otherwise <c>false</c>.
		/// </returns>
		override internal bool Update( float p_shortElapsed, bool p_forceUpdate, bool p_isStartupIteration )
		{
			if ( _destroyed )											return true;
			if ( items == null )										return true;
			if ( !_enabled )											return false;
			if ( _isComplete && !_isReversed && !p_forceUpdate )		return true;
			if ( _fullElapsed == 0 && _isReversed && !p_forceUpdate )	return false;
			if ( _isPaused && !p_forceUpdate )							return false;
			
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
			
			// Manage eventual OnStart.
			if ( !_hasStarted )								OnStart();
			
			ignoreCallbacks = p_isStartupIteration;
			
			// Set all elapsed and loops values.
			bool wasComplete = _isComplete;
			bool stepComplete = ( !_isReversed && !wasComplete && _elapsed >= _duration );
			SetLoops();
			SetElapsed();
			_isComplete = ( !_isReversed && _loops >= 0 && _completedLoops >= _loops );
			bool complete = ( !wasComplete && _isComplete ? true : false );
			
			// Manage Incremental loops.
			if ( _loopType == LoopType.Incremental ) {
				// prevCompleteLoops is stored only during Incremental loops,
				// so that if the loop type is changed while the tween is running,
				// the tween will change and update correctly.
				if ( prevCompletedLoops != _completedLoops ) {
					int currLoops = _completedLoops;
					if ( currLoops >= _loops )		--currLoops; // Avoid to calculate completion loop increment
					int diff = currLoops - prevCompletedLoops;
					if ( diff != 0 ) {
						SetIncremental( diff );
						prevCompletedLoops = currLoops;
					}
				}
			} else if ( prevCompletedLoops != 0 ) {
				// Readapt to non incremental loop type.
				SetIncremental( -prevCompletedLoops );
				prevCompletedLoops = 0;
			}
			
			// Update the elements...
			HOTSeqItem item;
			float twElapsed = ( !_isLoopingBack ? _elapsed : _duration - _elapsed );
			for ( int i = items.Count - 1; i > -1; --i ) {
				item = items[i];
				if ( item.twMember != null && item.startTime > twElapsed ) {
					item.twMember.GoTo( twElapsed - item.startTime, p_forceUpdate );
				}
			}
			for ( int i = 0; i < items.Count; ++i ) {
				item = items[i];
				if ( item.twMember != null && item.startTime <= twElapsed ) {
					item.twMember.GoTo( twElapsed - item.startTime, p_forceUpdate );
				}
			}
			
			// Manage eventual pause, complete, update, and stepComplete.
			if ( _fullElapsed != prevFullElapsed )		OnUpdate();
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
			HOTSeqItem item;
			for ( int i = 0; i < items.Count; ++i ) {
				item = items[i];
				if ( item.twMember == null )	continue;
				item.twMember.SetIncremental( p_diffIncr );
			}
		}
		
		// ===================================================================================
		// PRIVATE METHODS -------------------------------------------------------------------
		
		/// <summary>
		/// Sends the sequence to the given time (taking also loops into account) and eventually plays it.
		/// If the time is bigger than the total sequence duration, it goes to the end.
		/// </summary>
		/// <returns>
		/// Returns <c>true</c> if the sequence reached its end and was completed.
		/// </returns>
		override protected bool GoTo( float p_time, bool p_play, bool p_forceUpdate )
		{
			if ( !_enabled )									return false;
			
			if ( p_time > _fullDuration )
				p_time = _fullDuration;
			else if ( p_time < 0 )
				p_time = 0;
			if ( _fullElapsed == p_time && !p_forceUpdate )		return _isComplete;
			
			_fullElapsed = p_time;
			Update( 0, true );
			if ( !_isComplete && p_play )		Play();
			
			return _isComplete;
		}
		
		private void Rewind( bool p_play )
		{
			if ( !_enabled )							return;
			if ( items == null )						return;
			
			if ( !_hasStarted )							OnStart();
			
			_isComplete = false;
			_isLoopingBack = false;
			_completedLoops = 0;
			_fullElapsed = _elapsed = 0;
			
			HOTSeqItem item;
			for ( int i = items.Count - 1; i > - 1; --i ) {
				item = items[i];
				if ( item.twMember != null )				item.twMember.Rewind();
			}
			
			if ( p_play ) {
				Play();
			} else {
				Pause();
			}
		}
		
		/// <summary>
		/// Iterates through all the elements in order, to startup the plugins correctly.
		/// </summary>
		private void TweenStartupIteration()
		{
			// OPTIMIZE Find way to speed this up (by applying values directly instead than animating to them?)
			HOTSeqItem item;
			for ( int i = 0; i < items.Count; ++i ) {
				item = items[i];
				if ( item.twMember == null )			continue;
				item.twMember.Update( item.twMember.duration, true, true );
			}
			for ( int i = items.Count - 1; i > - 1; --i ) {
				item = items[i];
				if ( item.twMember != null )				item.twMember.Rewind();
			}
		}
		
		/// <summary>
		/// Manages on first start behaviour.
		/// </summary>
		override protected void OnStart()
		{
			// Move through all the elements in order, so the initial values are initialized.
			TweenStartupIteration();
			
			base.OnStart();
		}
		
		// ===================================================================================
		// HELPERS ---------------------------------------------------------------------------
		
		/// <summary>
		/// Fills the given list with all the plugins inside this sequence tween,
		/// while also looking for them recursively through inner sequences.
		/// Used by <c>HOTween.GetPlugins</c>.
		/// </summary>
		override internal void FillPluginsList( List<ABSTweenPlugin> p_plugs )
		{
			if ( items == null )				return;
			
			HOTSeqItem itm;
			for ( int i = 0; i < items.Count; ++i ) {
				itm = items[i];
				if ( itm.twMember == null )		continue;
				if ( itm.twMember is Sequence )
					( itm.twMember as Sequence ).FillPluginsList( p_plugs );
				else
					( itm.twMember as Tweener ).FillPluginsList( p_plugs );
			}
		}
		
		// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
		// ||| INTERNAL CLASSES ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
		// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
		
		/// <summary>
		/// Single sequencer item.
		/// Tween value can be null (in case this is simply used as a spacer).
		/// </summary>
		private class HOTSeqItem
		{
			// VARS ///////////////////////////////////////////////////
			
			public		float				startTime;
			
			private		float				_duration;
			
			// REFERENCES /////////////////////////////////////////////
			
			public		ABSTweenComponent	twMember;
			
			// READ-ONLY GETS /////////////////////////////////////////
			
			public		float				duration
			{
				get {
					if ( twMember == null )	return _duration;
					return twMember.duration;
				}
			}
			
			
			// ***********************************************************************************
			// CONSTRUCTOR
			// ***********************************************************************************
			
			public HOTSeqItem( float p_startTime, ABSTweenComponent p_twMember )
			{
				startTime = p_startTime;
				twMember = p_twMember;
			}
			public HOTSeqItem( float p_startTime, float p_duration )
			{
				startTime = p_startTime;
				_duration = p_duration;
			}
		}
	}
}

