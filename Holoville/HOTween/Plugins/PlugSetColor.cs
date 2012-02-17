// 
// PlugSetColor.cs
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
using Holoville.HOTween.Plugins.Core;

namespace Holoville.HOTween.Plugins
{
	/// <summary>
	/// Plugin for the tweening of the color of your choice.
	/// Used for changing material colors different from the default one (like _SpecColor or _Emission).
	/// Target for this tween must be of type <see cref="Material"/>.
	/// </summary>
	public class PlugSetColor : ABSTweenPlugin
	{
		// ENUMS //////////////////////////////////////////////////
		
		/// <summary>
		/// Enumeration of color properties names.
		/// </summary>
		public		enum ColorName 
		{
			/// <summary>
			/// Main color of a material.
			/// </summary>
			_Color,
			/// <summary>
			/// Specular color of a material (used in specular/glossy/vertexlit shaders).
			/// </summary>
			_SpecColor,
			/// <summary>
			/// Emissive color of a material (used in vertexlit shaders).
			/// </summary>
			_Emission,
			/// <summary>
			/// Reflection color of a material (used in reflective shaders).
			/// </summary>
			_ReflectColor
		}
		
		// VARS ///////////////////////////////////////////////////
		
		static internal	Type[]			validTargetTypes = { typeof(Material) };
		static internal Type[]			validPropTypes = { typeof(Color) };
		static internal	Type[]			validValueTypes = { typeof(Color) };
		
		private		Color				typedStartVal;
		private		Color				typedEndVal;
		private		float				changeVal;
		private		Color				diffChangeVal; // Used for incremental loops.
		private		string				colorName;
		
		// GETS/SETS //////////////////////////////////////////////
		
		/// <summary>
		/// Gets the untyped start value,
		/// sets both the untyped and the typed start value.
		/// </summary>
		override protected	object		startVal {
			get { return _startVal; }
			set {
				if ( tweenObj.isFrom && isRelative ) {
					_startVal = typedStartVal = typedEndVal + (Color)value;
				} else {
					_startVal = typedStartVal = (Color)value;
				}
			}
		}
		
		/// <summary>
		/// Gets the untyped end value,
		/// sets both the untyped and the typed end value.
		/// </summary>
		override protected	object		endVal {
			get { return _endVal; }
			set { _endVal = typedEndVal = (Color)value; }
		}
		
		
		// ***********************************************************************************
		// CONSTRUCTOR
		// ***********************************************************************************
		
		/// <summary>
		/// Creates a new instance of this plugin using the main ease type.
		/// </summary>
		/// <param name="p_endVal">
		/// The <see cref="Color"/> value to tween to.
		/// </param>
		public PlugSetColor( Color p_endVal ) : this( p_endVal, false ) {}
		/// <summary>
		/// Creates a new instance of this plugin.
		/// </summary>
		/// <param name="p_endVal">
		/// The <see cref="Color"/> value to tween to.
		/// </param>
		/// <param name="p_easeType">
		/// The <see cref="EaseType"/> to use.
		/// </param>
		public PlugSetColor( Color p_endVal, EaseType p_easeType ) : this( p_endVal, p_easeType, false ) {}
		/// <summary>
		/// Creates a new instance of this plugin using the main ease type.
		/// </summary>
		/// <param name="p_endVal">
		/// The <see cref="Color"/> value to tween to.
		/// </param>
		/// <param name="p_isRelative">
		/// If <c>true</c>, the given end value is considered relative instead than absolute.
		/// </param>
		public PlugSetColor( Color p_endVal, bool p_isRelative ) : base( p_endVal, p_isRelative )
		{
			ignoreAccessor = true;
		}
		/// <summary>
		/// Creates a new instance of this plugin.
		/// </summary>
		/// <param name="p_endVal">
		/// The <see cref="Color"/> value to tween to.
		/// </param>
		/// <param name="p_easeType">
		/// The <see cref="EaseType"/> to use.
		/// </param>
		/// <param name="p_isRelative">
		/// If <c>true</c>, the given end value is considered relative instead than absolute.
		/// </param>
		public PlugSetColor( Color p_endVal, EaseType p_easeType, bool p_isRelative ) : base( p_endVal, p_easeType, p_isRelative )
		{
			ignoreAccessor = true;
		}
		
		// ===================================================================================
		// PARAMETERS ------------------------------------------------------------------------
		
		/// <summary>
		/// Selects the color property to change.
		/// </summary>
		/// <param name="p_colorName">
		/// The propertyName/colorName to change (see Unity's <see cref="Material.SetColor"/> if you don't know how it works),
		/// set via the <see cref="PlugSetColor.ColorName"/> enum.
		/// </param>
		public PlugSetColor Property( ColorName p_colorName ) { return Property( p_colorName.ToString() ); }
		/// <summary>
		/// Selects the color property to change.
		/// </summary>
		/// <param name="p_propertyName">
		/// The propertyName/colorName to change (see Unity's <see cref="Material.SetColor"/> if you don't know how it works).
		/// </param>
		public PlugSetColor Property( string p_propertyName )
		{
			colorName = p_propertyName;
			return this;
		}
		
		// ===================================================================================
		// INTERNAL METHODS ------------------------------------------------------------------
		
		/// <summary>
		/// Overridden by plugins that need a specific type of target, to check it and validate it.
		/// Returns <c>true</c> if the tween target is valid.
		/// </summary>
		override internal bool ValidateTarget( object p_target )
		{
			return ( p_target is Material );
		}
		
		/// <summary>
		/// Updates the tween.
		/// </summary>
		/// <param name="p_totElapsed">
		/// The total elapsed time since startup.
		/// </param>
		override protected void DoUpdate ( float p_totElapsed )
		{
			float val = ease( p_totElapsed, 0, changeVal, _duration );
			
			SetValue( Color.Lerp( typedStartVal, typedEndVal, val ) );
		}
		
		// ===================================================================================
		// PRIVATE METHODS -------------------------------------------------------------------
		
		/// <summary>
		/// Returns the speed-based duration based on the given speed x second.
		/// </summary>
		override protected float GetSpeedBasedDuration( float p_speed )
		{
			float speedDur = changeVal / p_speed;
			if ( speedDur < 0 )		speedDur = -speedDur;
			return speedDur;
		}
		
		/// <summary>
		/// Sets the typed changeVal based on the current startVal and endVal.
		/// </summary>
		override protected void SetChangeVal()
		{
			changeVal = 1;
			
			if ( isRelative && !tweenObj.isFrom )
				typedEndVal = typedStartVal + typedEndVal;
			
			diffChangeVal = typedEndVal - typedStartVal;
		}
		
		/// <summary>
		/// Sets the correct values in case of Incremental loop type.
		/// </summary>
		/// <param name="p_diffIncr">
		/// The difference from the previous loop increment.
		/// </param>
		override protected void SetIncremental( int p_diffIncr )
		{
			typedStartVal += diffChangeVal * p_diffIncr;
			typedEndVal += diffChangeVal * p_diffIncr;
		}
		
		/// <summary>
		/// Sets the value of the controlled property.
		/// Some plugins (like PlugSetColor) might override this to get values from different properties.
		/// </summary>
		/// <param name="p_value">
		/// The new value.
		/// </param>
		override protected void SetValue( object p_value )
		{
			( tweenObj.target as Material ).SetColor( colorName, (Color)p_value );
		}
		
		/// <summary>
		/// Gets the current value of the controlled property.
		/// Some plugins (like PlugSetColor) might override this to set values on different properties.
		/// </summary>
		override protected object GetValue()
		{
			return ( tweenObj.target as Material ).GetColor( colorName );
		}
	}
}

