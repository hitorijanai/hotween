//
// ABSTweenComponent.cs
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

using System;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween.Plugins.Core;
using UnityEngine;

namespace Holoville.HOTween.Core
{
    /// <summary>
    /// Base class for all HOTween members
    /// (<see cref="Tweener"/> and <see cref="Sequence"/>).
    /// </summary>
    public abstract class ABSTweenComponent : IHOTweenComponent
    {
        // VARS ///////////////////////////////////////////////////

        internal string _id = "";
        internal int _intId = -1;
        internal bool _autoKillOnComplete = true;
        internal bool _enabled = true;
        internal float _timeScale = HOTween.defTimeScale;
        internal int _loops = 1;
        internal LoopType _loopType = HOTween.defLoopType;

        internal UpdateType _updateType = HOTween.defUpdateType;
        internal bool _isPaused;

        /// <summary>
        /// Always set to TRUE by Update(), if isStartupIteration is true,
        /// and reset to FALSE in the last line of Update().
        /// Can also be set to TRUE by Sequence.TweenStartupIteration,
        /// and then immediately reset to FALSE.
        /// </summary>
        internal bool ignoreCallbacks;

        internal Sequence contSequence; // Eventual sequence containing this tween.
        internal bool startupDone;

        internal TweenDelegate.TweenCallback onStart;
        internal TweenDelegate.TweenCallbackWParms onStartWParms;
        internal object[] onStartParms;

        internal TweenDelegate.TweenCallback onUpdate;
        internal TweenDelegate.TweenCallbackWParms onUpdateWParms;
        internal object[] onUpdateParms;

        internal TweenDelegate.TweenCallback onPause;
        internal TweenDelegate.TweenCallbackWParms onPauseWParms;
        internal object[] onPauseParms;

        internal TweenDelegate.TweenCallback onPlay;
        internal TweenDelegate.TweenCallbackWParms onPlayWParms;
        internal object[] onPlayParms;

        internal TweenDelegate.TweenCallback onRewinded;
        internal TweenDelegate.TweenCallbackWParms onRewindedWParms;
        internal object[] onRewindedParms;

        internal TweenDelegate.TweenCallback onStepComplete;
        internal TweenDelegate.TweenCallbackWParms onStepCompleteWParms;
        internal object[] onStepCompleteParms;

        internal TweenDelegate.TweenCallback onComplete;
        internal TweenDelegate.TweenCallbackWParms onCompleteWParms;
        internal object[] onCompleteParms;

        /// <summary>
        /// Completed loops.
        /// </summary>
        protected int _completedLoops;

        /// <summary>
        /// Duration.
        /// </summary>
        protected float _duration;

        /// <summary>
        /// Memorized when a partial tween is applied.
        /// </summary>
        protected float _originalDuration;

        /// <summary>
        /// Full duration.
        /// </summary>
        protected float _fullDuration;

        /// <summary>
        /// Elapsed.
        /// </summary>
        protected float _elapsed;

        /// <summary>
        /// Full elapsed.
        /// </summary>
        protected float _fullElapsed;

        /// <summary>
        /// Destroyed.
        /// </summary>
        protected bool _destroyed;

        /// <summary>
        /// Is empty.
        /// </summary>
        protected bool _isEmpty = true;

        /// <summary>
        /// Running backwards.
        /// </summary>
        protected bool _isReversed;

        /// <summary>
        /// Yoyo looping back.
        /// </summary>
        protected bool _isLoopingBack;

        /// <summary>
        /// Has started.
        /// </summary>
        protected bool _hasStarted;

        /// <summary>
        /// Is complete.
        /// </summary>
        protected bool _isComplete;

        /// <summary>
        /// Used to determine if OnUpdate callbacks should be called.
        /// </summary>
        protected float prevFullElapsed;

        // GETS/SETS //////////////////////////////////////////////

        /// <summary>
        /// Eventual string ID of this Tweener/Sequence
        /// (more than one Tweener/Sequence can share the same ID, thus allowing for grouped operations).
        /// You can also use <c>intId</c> instead of <c>id</c> for faster operations.
        /// </summary>
        public string id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Eventual int ID of this Tweener/Sequence
        /// (more than one Tweener/Sequence can share the same intId, thus allowing for grouped operations).
        /// The main difference from <c>id</c> is that while <c>id</c> is more legible, <c>intId</c> allows for faster operations.
        /// </summary>
        public int intId
        {
            get
            {
                return _intId;
            }
            set
            {
                _intId = value;
            }
        }

        /// <summary>
        /// Default is <c>true</c>, which means this Tweener/Sequence will be killed and removed from HOTween as soon as it's completed.
        /// If <c>false</c> doesn't remove this Tweener/Sequence from HOTween when it is completed,
        /// and you will need to call an <c>HOTween.Kill</c> to remove this Tweener/Sequence.
        /// </summary>
        public bool autoKillOnComplete
        {
            get
            {
                return _autoKillOnComplete;
            }
            set
            {
                _autoKillOnComplete = value;
            }
        }

        /// <summary>
        /// Default is <c>true</c>.
        /// If set to <c>false</c>, this Tweener/Sequence will not be updated,
        /// and any use of animation methods (Play/Pause/Rewind/etc) will be ignored
        /// (both if called directly via this instance, than if using HOTween.Play/Pause/Rewind/etc.).
        /// </summary>
        public bool enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
            }
        }

        /// <summary>
        /// Time scale that will be used by this Tweener/Sequence.
        /// </summary>
        public float timeScale
        {
            get
            {
                return _timeScale;
            }
            set
            {
                _timeScale = value;
            }
        }

        /// <summary>
        /// Number of times the Tweener/Sequence will run (<c>-1</c> means the tween has infinite loops).
        /// </summary>
        public int loops
        {
            get
            {
                return _loops;
            }
            set
            {
                _loops = value;
                SetFullDuration();
            }
        }

        /// <summary>
        /// Type of loop for this Tweener/Sequence, in case <see cref="loops"/> is greater than 1 (or infinite).
        /// </summary>
        public LoopType loopType
        {
            get
            {
                return _loopType;
            }
            set
            {
                _loopType = value;
            }
        }

        /// <summary>
        /// Gets and sets the time position of the Tweener/Sequence (loops are included when not infinite, delay is not).
        /// </summary>
        public float position
        {
            get
            {
                return (_loops < 1 ? _elapsed : _fullElapsed);
            }
            set
            {
                GoTo(value, !_isPaused);
            }
        }

        // READ-ONLY GETS /////////////////////////////////////////

        /// <summary>
        /// Duration of this Tweener/Sequence, loops and tween delay excluded.
        /// </summary>
        public float duration
        {
            get
            {
                return _duration;
            }
        }

        /// <summary>
        /// Full duration of this Tweener/Sequence, loops included (when not infinite) but tween delay excluded.
        /// </summary>
        public float fullDuration
        {
            get
            {
                return _fullDuration;
            }
        }

        /// <summary>
        /// Elapsed time within the current loop (tween delay excluded).
        /// </summary>
        public float elapsed
        {
            get
            {
                return _elapsed;
            }
        }

        /// <summary>
        /// Full elapsed time including loops (but without considering tween delay).
        /// </summary>
        public float fullElapsed
        {
            get
            {
                return _fullElapsed;
            }
        }

        /// <summary>
        /// The update type for this Tweener/Sequence.
        /// </summary>
        public UpdateType updateType
        {
            get
            {
                return _updateType;
            }
        }

        /// <summary>
        /// Number of loops that have been executed.
        /// </summary>
        public int completedLoops
        {
            get
            {
                return _completedLoops;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> if this Tweener/Sequence was destroyed
        /// (either because it was manually destroyed, because it was completed, or because its target was destroyed).
        /// </summary>
        public bool destroyed
        {
            get
            {
                return _destroyed;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> if this Tweener/Sequence contains no tweens
        /// (if this is a tween, it means that no valid property to tween was set;
        /// if this is a sequence, it means no valid <see cref="Tweener"/> was yet added).
        /// </summary>
        public bool isEmpty
        {
            get
            {
                return _isEmpty;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> if this Tweener/Sequence is set to go backwards (because of a call to <c>Reverse</c>.
        /// </summary>
        public bool isReversed
        {
            get
            {
                return _isReversed;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> when this Tweener/Sequence is in the "going backwards" part of a Yoyo loop.
        /// </summary>
        public bool isLoopingBack
        {
            get
            {
                return _isLoopingBack;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> if this Tweener/Sequence is paused.
        /// </summary>
        public bool isPaused
        {
            get
            {
                return _isPaused;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> after this Tweener/Sequence was started the first time,
        /// or if a call to <c>GoTo</c> or <c>GoToAndPlay</c> was executed.
        /// </summary>
        public bool hasStarted
        {
            get
            {
                return _hasStarted;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> when this Tweener/Sequence is complete.
        /// </summary>
        public bool isComplete
        {
            get
            {
                return _isComplete;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> if this Tweener/Sequence was added to a Sequence.
        /// </summary>
        public bool isSequenced
        {
            get
            {
                return contSequence != null;
            }
        }


        // ===================================================================================
        // METHODS ---------------------------------------------------------------------------

        /// <summary>
        /// Kills this Tweener/Sequence, removes it from HOTween, and cleans it.
        /// </summary>
        public void Kill()
        {
            Kill(true);
        }

        /// <summary>
        /// Kills this Tweener/Sequence and cleans it.
        /// </summary>
        /// <param name="p_autoRemoveFromHOTween">
        /// If <c>true</c> also calls <c>HOTween.Kill(this)</c> to remove it from HOTween.
        /// Set internally to <c>false</c> when I already know that HOTween is going to remove it.
        /// </param>
        internal virtual void Kill(bool p_autoRemoveFromHOTween)
        {
            if (_destroyed)
            {
                return;
            }

            _destroyed = _isEmpty = true;
            if (p_autoRemoveFromHOTween)
            {
                HOTween.Kill(this);
            }
        }

        /// <summary>
        /// Resumes this Tweener/Sequence (tween delay included).
        /// </summary>
        public void Play()
        {
            if (_enabled)
            {
                PlayIfPaused();
            }
        }

        void PlayIfPaused()
        {
            if (_isPaused)
            {
                _isPaused = false;
                OnPlay();
            }
        }

        /// <summary>
        /// Resumes this Tweener/Sequence (tween delay included) and plays it forward.
        /// </summary>
        public void PlayForward()
        {
            if (_enabled)
            {
                _isReversed = false;
                PlayIfPaused();
            }
        }

        /// <summary>
        /// Resumes this Tweener/Sequence and plays it backwards.
        /// </summary>
        public void PlayBackwards()
        {
            if (_enabled)
            {
                _isReversed = true;
                PlayIfPaused();
            }
        }

        /// <summary>
        /// Pauses this Tweener/Sequence.
        /// </summary>
        public void Pause()
        {
            if (_enabled && !_isPaused)
            {
                _isPaused = true;
                OnPause();
            }
        }

        /// <summary>
        /// Rewinds this Tweener/Sequence (loops and tween delay included), and pauses it.
        /// </summary>
        public abstract void Rewind();

        /// <summary>
        /// Restarts this Tweener/Sequence from the beginning (loops and tween delay included).
        /// </summary>
        public abstract void Restart();

        /// <summary>
        /// Reverses this Tweener/Sequence,
        /// animating it backwards from its curren position.
        /// </summary>
        public void Reverse()
        {
            if (_enabled)
            {
                _isReversed = !_isReversed;
            }
        }

        /// <summary>
        /// Completes this Tweener/Sequence.
        /// Where a loop was involved, the Tweener/Sequence completes at the position where it would actually be after the set number of loops.
        /// If there were infinite loops, this method will have no effect.
        /// </summary>
        public void Complete()
        {
            Complete(true);
        }

        /// <summary>
        /// Sends the Tweener/Sequence to the given time (taking also loops into account).
        /// If the time is bigger than the total Tweener/Sequence duration, it goes to the end.
        /// </summary>
        /// <param name="p_time">
        /// The time where the Tweener/Sequence should be sent.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the Tweener/Sequence reached its end and was completed.
        /// </returns>
        public bool GoTo(float p_time)
        {
            return GoTo(p_time, false, false);
        }

        /// <summary>
        /// Sends the Tweener/Sequence to the given time (taking also loops into account).
        /// If the time is bigger than the total Tweener/Sequence duration, it goes to the end.
        /// </summary>
        /// <param name="p_time">
        /// The time where the Tweener/Sequence should be sent.
        /// </param>
        /// <param name="p_forceUpdate">
        /// By default, if a Tweener/Sequence is already at the exact given time, it will not be refreshed.
        /// Setting this to <c>true</c> will force it to refresh
        /// (useful only if you want to be sure that any changes you made to the tweened property,
        /// outside of HOTween, are reset).
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the Tweener/Sequence reached its end and was completed.
        /// </returns>
        public bool GoTo(float p_time, bool p_forceUpdate)
        {
            return GoTo(p_time, false, p_forceUpdate);
        }

        /// <summary>
        /// Sends the Tweener/Sequence to the given time (taking also loops into account) and plays it.
        /// If the time is bigger than the total Tweener/Sequence duration, it goes to the end.
        /// </summary>
        /// <param name="p_time">
        /// The time where the Tweener/Sequence should be sent.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the Tweener/Sequence reached its end and was completed.
        /// </returns>
        public bool GoToAndPlay(float p_time)
        {
            return GoTo(p_time, true, false);
        }

        /// <summary>
        /// Sends the Tweener/Sequence to the given time (taking also loops into account) and plays it.
        /// If the time is bigger than the total Tweener/Sequence duration, it goes to the end.
        /// </summary>
        /// <param name="p_time">
        /// The time where the Tweener/Sequence should be sent.
        /// </param>
        /// <param name="p_forceUpdate">
        /// By default, if a Tweener/Sequence is already at the exact given time, it will not be refreshed.
        /// Setting this to <c>true</c> will force it to refresh
        /// (useful only if you want to be sure that any changes you made to the tweened property,
        /// outside of HOTween, are reset).
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the Tweener/Sequence reached its end and was completed.
        /// </returns>
        public bool GoToAndPlay(float p_time, bool p_forceUpdate)
        {
            return GoTo(p_time, true, p_forceUpdate);
        }

        /// <summary>
        /// A coroutine that waits until the Tweener/Sequence is complete (delays and loops included).
        /// You can use it inside a coroutin as a yield. Ex:
        /// yield return StartCoroutine( myTweenComponent.WaitForCompletion() );
        /// </summary>
        public IEnumerator WaitForCompletion()
        {
            while (!_isComplete)
            {
                yield return 0;
            }
            yield break;
        }

        /// <summary>
        /// Returns <c>true</c> if the given target is currently involved in a running tween or sequence.
        /// Returns <c>false</c> both if the given target is not inside a tween, than if the relative tween is paused.
        /// To simply check if the target is attached to a tween or sequence, use <c>IsLinkedTo( target )</c> instead.
        /// </summary>
        /// <param name="p_target">
        /// The target to check.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the given target is currently involved in a running tween or sequence.
        /// </returns>
        public abstract bool IsTweening(object p_target);

        /// <summary>
        /// Returns <c>true</c> if the given target is linked to a tween or sequence (running or not).
        /// </summary>
        /// <param name="p_target">
        /// The target to check.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the given target is linked to a tween or sequence (running or not).
        /// </returns>
        public abstract bool IsLinkedTo(object p_target);

        /// <summary>
        /// Returns a list of all the targets of this tween, or NULL if there are none.
        /// </summary>
        /// <returns>A list of all the targets of this tween, or NULL if there are none.</returns>
        public abstract List<object> GetTweenTargets();

        // ===================================================================================
        // INTERNAL METHODS ------------------------------------------------------------------

        /// <summary>
        /// Used internally by HOTween, to avoid having the tween calling a kill while HOTween will already be killing it.
        /// </summary>
        internal abstract void Complete(bool p_doAutoKill);

        /// <summary>
        /// Updates the Tweener/Sequence by the given elapsed time,
        /// and returns a value of <c>true</c> if the Tweener/Sequence is complete.
        /// </summary>
        /// <param name="p_elapsed">
        /// The elapsed time since the last update.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the tween is not reversed and is complete (or the tween target doesn't exist anymore), otherwise <c>false</c>.
        /// </returns>
        internal bool Update(float p_elapsed)
        {
            return Update(p_elapsed, false, false);
        }

        /// <summary>
        /// Updates the Tweener/Sequence by the given elapsed time,
        /// and returns a value of <c>true</c> if the Tweener/Sequence is complete.
        /// </summary>
        /// <param name="p_elapsed">
        /// The elapsed time since the last update.
        /// </param>
        /// <param name="p_forceUpdate">
        /// If <c>true</c> forces the update even if the Tweener/Sequence is complete or paused,
        /// but ignores onUpdate, and sends onComplete and onStepComplete calls only if the Tweener/Sequence wasn't complete before this call.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the tween is not reversed and complete (or the tween target doesn't exist anymore), otherwise <c>false</c>.
        /// </returns>
        internal bool Update(float p_elapsed, bool p_forceUpdate)
        {
            return Update(p_elapsed, p_forceUpdate, false);
        }

        /// <summary>
        /// Updates the Tweener/Sequence by the given elapsed time,
        /// and returns a value of <c>true</c> if the Tweener/Sequence is complete.
        /// </summary>
        /// <param name="p_elapsed">
        /// The elapsed time since the last update.
        /// </param>
        /// <param name="p_forceUpdate">
        /// If <c>true</c> forces the update even if the Tweener/Sequence is complete or paused,
        /// but ignores onUpdate, and sends onComplete and onStepComplete calls only if the Tweener/Sequence wasn't complete before this call.
        /// </param>
        /// <param name="p_isStartupIteration">
        /// If <c>true</c> means the update is due to a startup iteration (managed by Sequence Startup),
        /// and all callbacks will be ignored.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the tween is not reversed and complete (or the tween target doesn't exist anymore), otherwise <c>false</c>.
        /// </returns>
        internal abstract bool Update(float p_elapsed, bool p_forceUpdate, bool p_isStartupIteration);

        /// <summary>
        /// Applies the correct Incremental Sequence loop value.
        /// Called by Sequences when they need to change the increment value of nested Sequences/Tweeners.
        /// </summary>
        /// <param name="p_diffIncr">
        /// The difference from the previous main Sequence loop increment.
        /// </param>
        internal abstract void SetIncremental(int p_diffIncr);

        // ===================================================================================
        // PRIVATE METHODS -------------------------------------------------------------------

        /// <summary>
        /// Sends the Tweener/Sequence to the given time (taking also loops into account) and plays it.
        /// If the time is bigger than the total Tweener/Sequence duration, it goes to the end.
        /// </summary>
        protected abstract bool GoTo(float p_time, bool p_play, bool p_forceUpdate);

        /// <summary>
        /// Startup this tween
        /// (might or might not call OnStart, depending if the tween is in a Sequence or not).
        /// Can be executed only once per tween.
        /// </summary>
        protected virtual void Startup()
        {
            startupDone = true;
        }

        /// <summary>
        /// Manages on first start behaviour.
        /// </summary>
        protected virtual void OnStart()
        {
            if (ignoreCallbacks)
            {
                return;
            }
            _hasStarted = true;
            if (onStart != null)
            {
                onStart();
            }
            else if (onStartWParms != null)
            {
                onStartWParms(new TweenEvent(this, onStartParms));
            }
        }

        /// <summary>
        /// Manages on update behaviour.
        /// </summary>
        protected void OnUpdate()
        {
            if (ignoreCallbacks)
            {
                return;
            }
            if (onUpdate != null)
            {
                onUpdate();
            }
            else if (onUpdateWParms != null)
            {
                onUpdateWParms(new TweenEvent(this, onUpdateParms));
            }
        }

        /// <summary>
        /// Manages on pause behaviour.
        /// </summary>
        protected void OnPause()
        {
            if (ignoreCallbacks)
            {
                return;
            }
            if (onPause != null)
            {
                onPause();
            }
            else if (onPauseWParms != null)
            {
                onPauseWParms(new TweenEvent(this, onPauseParms));
            }
        }

        /// <summary>
        /// Manages on resume behaviour.
        /// </summary>
        protected void OnPlay()
        {
            if (ignoreCallbacks)
            {
                return;
            }
            if (onPlay != null)
            {
                onPlay();
            }
            else if (onPlayWParms != null)
            {
                onPlayWParms(new TweenEvent(this, onPlayParms));
            }
        }

        /// <summary>
        /// Manages on rewinded behaviour.
        /// </summary>
        protected void OnRewinded()
        {
            if (ignoreCallbacks)
            {
                return;
            }
            if (onRewinded != null)
            {
                onRewinded();
            }
            else if (onRewindedWParms != null)
            {
                onRewindedWParms(new TweenEvent(this, onRewindedParms));
            }
        }

        /// <summary>
        /// Manages step on complete behaviour.
        /// </summary>
        protected void OnStepComplete()
        {
            if (ignoreCallbacks)
            {
                return;
            }
            if (onStepComplete != null)
            {
                onStepComplete();
            }
            else if (onStepCompleteWParms != null)
            {
                onStepCompleteWParms(new TweenEvent(this, onStepCompleteParms));
            }
        }

        /// <summary>
        /// Manages on complete behaviour.
        /// </summary>
        protected void OnComplete()
        {
            _isComplete = true;
            OnStepComplete();
            if (ignoreCallbacks)
            {
                return;
            }
            if (onComplete != null || onCompleteWParms != null)
            {
                HOTween.onCompletes.Add(this); // delegate to HOTween which will call OnCompleteDispatch after this tween is eventually destroyed.
            }
        }

        /// <summary>
        /// Called by HOTween if this tween was placed inside its onCompletes list during this.OnComplete().
        /// </summary>
        internal void OnCompleteDispatch()
        {
            if (onComplete != null)
            {
                onComplete();
            }
            else if (onCompleteWParms != null)
            {
                onCompleteWParms(new TweenEvent(this, onCompleteParms));
            }
        }

        /// <summary>
        /// Sets the current <c>fullDuration</c>, based on the current <c>duration</c> and <c>loops</c> values.
        /// Remember to call this method each time you change the duration or loops of a tween.
        /// </summary>
        protected void SetFullDuration()
        {
            _fullDuration = (_loops < 0 ? float.PositiveInfinity : _duration*_loops);
        }

        /// <summary>
        /// Sets the current <c>elapsed</c> time, based on the current <c>fullElapsed</c> and <c>completedLoops</c> values.
        /// Remember to call this method each time you set fullElapsed (after changing the eventual loops count where needed).
        /// </summary>
        protected void SetElapsed()
        {
            if (_duration == 0 || _loops >= 0 && _completedLoops >= _loops)
            {
                _elapsed = _duration;
            }
            else if (_fullElapsed < _duration)
            {
                _elapsed = _fullElapsed;
            }
            else
            {
                _elapsed = _fullElapsed%_duration;
            }
        }

        /// <summary>
        /// Sets <c>completedLoops</c> and <c>isLoopingBack</c>, based on the current <c>fullElapsed</c> value.
        /// </summary>
        protected void SetLoops()
        {
            _completedLoops = (int)Math.Floor(_fullElapsed/_duration); // OPTIMIZE can't use (int) or tilde to Floor number because they're imprecise, but I should find a quicker solution.
            _isLoopingBack = (_loopType != LoopType.Restart && _loopType != LoopType.Incremental &&
                              (_loops > 0 && (_completedLoops < _loops && _completedLoops%2 != 0 || _completedLoops >= _loops && _completedLoops%2 == 0)
                               || _loops < 0 && _completedLoops%2 != 0));
        }

        // ===================================================================================
        // HELPERS ---------------------------------------------------------------------------

        /// <summary>
        /// Fills the given list with all the plugins inside this sequence tween,
        /// while also looking for them recursively through inner sequences.
        /// Used by <c>HOTween.GetPlugins</c>.
        /// </summary>
        internal abstract void FillPluginsList(List<ABSTweenPlugin> p_plugs);
    }
}
