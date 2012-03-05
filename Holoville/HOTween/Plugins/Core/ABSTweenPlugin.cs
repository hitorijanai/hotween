// 
// ABSTweenPlugin.cs
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
using System;
using System.Reflection;
using FastDynamicMemberAccessor;
using Holoville.HOTween.Core;

namespace Holoville.HOTween.Plugins.Core
{
	/// <summary>
	/// ABSTRACT base class for all <see cref="ABSTweenPlugin"/> classes.
	/// </summary>
	abstract public class ABSTweenPlugin
	{
		// VARS ///////////////////////////////////////////////////
		
		/// <summary>
		/// Untyped start value.
		/// </summary>
		protected	object									_startVal = null;
		/// <summary>
		/// Untyped end value.
		/// </summary>
		protected	object									_endVal = null;
		/// <summary>
		/// Stored so it can be set indipendently in case of speed-based tweens.
		/// </summary>
		protected	float									_duration;
		
		private		bool									_initialized;
		private		bool									_easeReversed;
		
		/// <summary>
		/// Ease type.
		/// </summary>
		protected	TweenDelegate.EaseFunc					ease;
		/// <summary>
		/// Indicates that the end value is relative instead than absolute.
		/// Default: <c>false</c>.
		/// </summary>
		protected	bool									isRelative;
		/// <summary>
		/// Some plugins (like PlugSetColor) may set this to <c>false</c> when instantiated,
		/// to prevent the creation of a useless valAccessor.
		/// </summary>
		protected	bool									ignoreAccessor;
		
		private		EaseType								easeType; // Store so instance can be cloned and ease can be changed while playing.
		private		EaseInfo								easeInfo;
		private		IMemberAccessor							valAccessor;
		private		bool									wasStarted;
		private		int										prevCompletedLoops = 0; // Stored only during Incremental loop type.
		
		// IOS-ONLY VARS //////////////////////////////////////////
		
		private		PropertyInfo							propInfo;
		private		FieldInfo								fieldInfo;
		
		// REFERENCES /////////////////////////////////////////////
		
		/// <summary>
		/// Reference to the Tweener controlling this plugin.
		/// </summary>
		protected	Tweener									tweenObj;
		
		// GETS/SETS //////////////////////////////////////////////
		
		/// <summary>
		/// Gets the untyped start value,
		/// sets both the untyped and the typed start value.
		/// </summary>
		abstract protected	object							startVal {
			get;
			set;
		}
		
		/// <summary>
		/// Gets the untyped end value,
		/// sets both the untyped and the typed end value.
		/// </summary>
		abstract protected	object							endVal {
			get;
			set;
		}
		
		// READ-ONLY GETS /////////////////////////////////////////
		
		/// <summary>
		/// Used by TweenParms to understand if this plugin was initialized with
		/// another Tweener, and thus clone it.
		/// </summary>
		internal	bool									initialized
		{
			get { return _initialized; }
		}
		
		internal	float									duration
		{
			get { return _duration; }
		}
		
		internal	bool									easeReversed
		{
			get { return _easeReversed; }
		}
		
		
		// ***********************************************************************************
		// CONSTRUCTOR
		// ***********************************************************************************
		
		/// <summary>
		/// Creates a new instance of this plugin with the given options.
		/// Used because easeType can't be null, and otherwise there's no way
		/// to understand if the ease was voluntarily set by the user or not.
		/// </summary>
		/// <param name="p_endVal">
		/// The <see cref="object"/> value to tween to.
		/// </param>
		/// <param name="p_isRelative">
		/// If <c>true</c>, the given end value is considered relative instead than absolute.
		/// </param>
		public ABSTweenPlugin( object p_endVal, bool p_isRelative )
		{
			isRelative = p_isRelative;
			_endVal = p_endVal;
		}
		/// <summary>
		/// Creates a new instance of this plugin with the given options.
		/// </summary>
		/// <param name="p_endVal">
		/// The <see cref="object"/> value to tween to.
		/// </param>
		/// <param name="p_easeType">
		/// The <see cref="EaseType"/> to use.
		/// </param>
		/// <param name="p_isRelative">
		/// If <c>true</c>, the given end value is considered relative instead than absolute.
		/// </param>
		public ABSTweenPlugin( object p_endVal, EaseType p_easeType, bool p_isRelative )
		{
			isRelative = p_isRelative;
			_endVal = p_endVal;
			easeType = p_easeType;
			easeInfo = EaseInfo.GetEaseInfo( p_easeType );
			ease = easeInfo.ease;
		}
		
		// ===================================================================================
		// INTERNAL METHODS ------------------------------------------------------------------
		
		/// <summary>
		/// Initializes the plugin after its instantiation.
		/// Called by Tweener after a property and plugin have been validated, and the plugin has to be set and added.
		/// Virtual because some classes (like PlugVector3Path) override it to avoid isRelative being TRUE.
		/// </summary>
		/// <param name="p_tweenObj">
		/// The <see cref="Tweener"/> to refer to.
		/// </param>
		/// <param name="p_propertyName">
		/// The name of the property to control.
		/// </param>
		/// <param name="p_easeType">
		/// The <see cref="EaseType"/> to use.
		/// </param>
		/// <param name="p_targetType">
		/// Directly passed from TweenParms to speed up MemberAccessor creation.
		/// </param>
		/// <param name="p_propertyInfo">
		/// Directly passed from TweenParms to speed up MemberAccessor creation.
		/// </param>
		/// <param name="p_fieldInfo">
		/// Directly passed from TweenParms to speed up MemberAccessor creation.
		/// </param>
		virtual internal void Init( Tweener p_tweenObj, string p_propertyName, EaseType p_easeType, Type p_targetType, PropertyInfo p_propertyInfo, FieldInfo p_fieldInfo )
		{
			_initialized = true;
			
			tweenObj = p_tweenObj;
			if ( easeInfo == null || tweenObj.speedBased ) {
				SetEase( p_easeType );
			}
			_duration = tweenObj.duration;
			
			if ( HOTween.isIOS ) {
				propInfo = p_propertyInfo;
				fieldInfo = p_fieldInfo;
			} else {
				if ( !ignoreAccessor )		valAccessor = MemberAccessorCacher.Make( p_targetType, p_propertyName, p_propertyInfo, p_fieldInfo );
			}
		}
		
		/// <summary>
		/// Starts up the plugin, getting the actual start and change values.
		/// Called by Tweener right before starting the effective animations.
		/// </summary>
		internal void Startup()
		{
			if ( wasStarted ) {
				TweenWarning.Log( "Startup() for plugin " + this + " (target: " + tweenObj.target + ") has already been called. Startup() won't execute twice." );
				return; // Startup can't be executed twice otherwise some typedEndVal (like for HOTPluginColor) will be set incorrectly
			}
			
			wasStarted = true;
			
			// Manage TO or FROM.
			if ( tweenObj.isFrom ) {
				// Order is fundamental (otherwise setters for isRelative get messed up).
				object prevEndVal = _endVal;
				endVal = GetValue();
				startVal = prevEndVal;
			} else {
				endVal = _endVal;
				startVal = GetValue();
			}
			// Set changeVal.
			SetChangeVal();
			
			if ( tweenObj.speedBased ) {
				// Get duration based on speed.
				// Can't be done earlier because it needs changeVal to be set.
				_duration = GetSpeedBasedDuration( _duration );
			}
		}
		
		/// <summary>
		/// Overridden by plugins that need a specific type of target, to check it and validate it.
		/// Returns <c>true</c> if the tween target is valid.
		/// </summary>
		virtual internal bool ValidateTarget( object p_target )
		{
			return true;
		}
		
		/// <summary>
		/// Updates the tween.
		/// </summary>
		/// <param name="p_totElapsed">
		/// The total elapsed time since startup (loops excluded).
		/// </param>
		internal void Update( float p_totElapsed )
		{
			if ( tweenObj.loopType == LoopType.Incremental ) {
				// prevCompleteLoops is stored only during Incremental loops,
				// so that if the loop type is changed while the tween is running,
				// the tween will change and update correctly.
				if ( prevCompletedLoops != tweenObj.completedLoops ) {
					SetIncremental( tweenObj.completedLoops - prevCompletedLoops );
					prevCompletedLoops = tweenObj.completedLoops;
				}
			} else if ( prevCompletedLoops != 0 ) {
				// Readapt to non incremental loop type.
				SetIncremental( -prevCompletedLoops );
				prevCompletedLoops = 0;
			}
			
			if ( p_totElapsed > _duration )		p_totElapsed = _duration;
			
			DoUpdate( p_totElapsed );
		}
		
		/// <summary>
		/// Updates the plugin.
		/// </summary>
		abstract protected void DoUpdate( float p_totElapsed );
		
		/// <summary>
		/// Rewinds the tween.
		/// Should be overriden by tweens that control only part of the property (like HOTPluginVector3X).
		/// </summary>
		virtual internal void Rewind()
		{
			SetValue( startVal );
		}
		
		/// <summary>
		/// Completes the tween.
		/// Should be overriden by tweens that control only part of the property (like HOTPluginVector3X).
		/// </summary>
		virtual internal void Complete()
		{
			SetValue( _endVal );
		}
		
		/// <summary>
		/// Reverses the ease of this plugin.
		/// </summary>
		internal void ReverseEase()
		{
			if ( easeInfo.inverseEase == null )		return; // No inverse for this ease.
			
			_easeReversed = !_easeReversed;
			ease = ( _easeReversed ? easeInfo.inverseEase : easeInfo.ease );
		}
		
		/// <summary>
		/// Sets the ease type (called during Init, but can also be called by Tweener to change easeType while playing).
		/// </summary>
		internal void SetEase( EaseType p_easeType )
		{
			easeType = p_easeType;
			easeInfo = EaseInfo.GetEaseInfo( easeType );
			ease = easeInfo.ease;
			
			if ( _easeReversed && easeInfo.inverseEase != null )	ease = easeInfo.inverseEase;
		}
		
		/// <summary>
		/// Returns the speed-based duration based on the given speed.
		/// </summary>
		abstract protected float GetSpeedBasedDuration( float p_speed );
		
		/// <summary>
		/// Returns a clone of the basic plugin
		/// (as it was at construction, without anything that was set during Init).
		/// </summary>
		internal ABSTweenPlugin CloneBasic()
		{
			// OPTIMIZE incredibly slow. Possible solutions...
			// - http://rogeralsing.com/2008/02/28/linq-expressions-creating-objects (but requires Linq, and thus System.Core, which I'd prefer to avoid)
			// - http://ayende.com/blog/3167/creating-objects-perf-implications (has to know the class to create, thus is useless)
			return Activator.CreateInstance( this.GetType(), ( tweenObj != null && tweenObj.isFrom ? _startVal : _endVal ), easeType, isRelative ) as ABSTweenPlugin;
		}
		
		// ===================================================================================
		// PRIVATE METHODS -------------------------------------------------------------------
		
		/// <summary>
		/// Sets the typed changeVal based on the current startVal and endVal.
		/// Can only be called once, otherwise some typedEndVal (like HOTPluginColor) will be set incorrectly.
		/// </summary>
		abstract protected void SetChangeVal();
		
		/// <summary>
		/// Sets the correct values in case of Incremental loop type.
		/// Also called by Tweener.ApplySequenceIncrement (used by Sequences during Incremental loops).
		/// </summary>
		/// <param name="p_diffIncr">
		/// The difference from the previous loop increment.
		/// </param>
		abstract internal void SetIncremental( int p_diffIncr );
		
		/// <summary>
		/// Sets the value of the controlled property.
		/// Some plugins (like PlugSetColor) might override this to get values from different properties.
		/// </summary>
		/// <param name="p_value">
		/// The new value.
		/// </param>
		virtual protected void SetValue( object p_value )
		{
			if ( HOTween.isIOS ) {
				if ( propInfo != null ) {
					try {
						propInfo.SetValue( tweenObj.target, p_value, null );
					} catch ( InvalidCastException ) {
						// This happens only if a float is being assigned to an int.
						propInfo.SetValue( tweenObj.target, Mathf.FloorToInt( (float)p_value ), null );
					} catch ( ArgumentException ) {
						// This happens only on iOS if a float is being assigned to an int.
						propInfo.SetValue( tweenObj.target, Mathf.FloorToInt( (float)p_value ), null );
					}
				} else {
					try {
						fieldInfo.SetValue( tweenObj.target, p_value );
					} catch ( InvalidCastException ) {
						// This happens only if a float is being assigned to an int.
						fieldInfo.SetValue( tweenObj.target, Mathf.FloorToInt( (float)p_value ) );
					} catch ( ArgumentException ) {
						// This happens only on iOS if a float is being assigned to an int.
						fieldInfo.SetValue( tweenObj.target, Mathf.FloorToInt( (float)p_value ) );
					}
				}
			} else {
				try {
					valAccessor.Set( tweenObj.target, p_value );
				} catch ( InvalidCastException ) {
					// This happens only if a float is being assigned to an int.
					valAccessor.Set( tweenObj.target, Mathf.FloorToInt( (float)p_value ) ); // OPTIMIZE store if it's int prior to this, so valAccessor doesn't even have to run to catch the error?
				} catch ( ArgumentException ) {
					// This happens only on iOS if a float is being assigned to an int, but is also here just to be sure.
					valAccessor.Set( tweenObj.target, Mathf.FloorToInt( (float)p_value ) );
				}
			}
		}
		
		/// <summary>
		/// Gets the current value of the controlled property.
		/// Some plugins (like PlugSetColor) might override this to set values on different properties.
		/// </summary>
		virtual protected object GetValue()
		{
			if ( HOTween.isIOS ) {
				if ( propInfo != null )		return propInfo.GetValue( tweenObj.target, null );
				return fieldInfo.GetValue( tweenObj.target );
			}
			return valAccessor.Get( tweenObj.target );
		}
	}
}

