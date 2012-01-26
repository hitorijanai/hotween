// 
// PlugVector3Z.cs
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
	/// Plugin for the tweening of only the Z value of Vector3 objects.
	/// </summary>
	public class PlugVector3Z : PlugVector3X
	{
		// GETS/SETS //////////////////////////////////////////////
		
		/// <summary>
		/// Gets the untyped start value,
		/// sets both the untyped and the typed start value.
		/// </summary>
		override protected	object		startVal {
			get { return _startVal; }
			set { _startVal = value; typedStartVal = ( (Vector3)( _startVal ) ).z; }
		}
		
		
		// ***********************************************************************************
		// CONSTRUCTOR
		// ***********************************************************************************
		
		/// <summary>
		/// Creates a new instance of this plugin using the main ease type.
		/// </summary>
		/// <param name="p_endVal">
		/// The value to tween to.
		/// </param>
		public PlugVector3Z( float p_endVal ) : base( p_endVal, false ) {}
		/// <summary>
		/// Creates a new instance of this plugin.
		/// </summary>
		/// <param name="p_endVal">
		/// The <see cref="Vector3"/> value to tween to.
		/// </param>
		/// <param name="p_easeType">
		/// The <see cref="EaseType"/> to use.
		/// </param>
		public PlugVector3Z( float p_endVal, EaseType p_easeType ) : base( p_endVal, p_easeType, false ) {}
		/// <summary>
		/// Creates a new instance of this plugin using the main ease type.
		/// </summary>
		/// <param name="p_endVal">
		/// The value to tween to.
		/// </param>
		/// <param name="p_isRelative">
		/// If <c>true</c>, the given end value is considered relative instead than absolute.
		/// </param>
		public PlugVector3Z( float p_endVal, bool p_isRelative ) : base( p_endVal, p_isRelative ) {}
		/// <summary>
		/// Creates a new instance of this plugin.
		/// </summary>
		/// <param name="p_endVal">
		/// The value to tween to.
		/// </param>
		/// <param name="p_easeType">
		/// The <see cref="EaseType"/> to use.
		/// </param>
		/// <param name="p_isRelative">
		/// If <c>true</c>, the given end value is considered relative instead than absolute.
		/// </param>
		public PlugVector3Z( float p_endVal, EaseType p_easeType, bool p_isRelative ) : base( p_endVal, p_easeType, p_isRelative ) {}
		
		// ===================================================================================
		// METHODS ---------------------------------------------------------------------------
		
		/// <summary>
		/// Rewinds the tween.
		/// Should be overriden by tweens that control only part of the property (like HOTPluginVector3X).
		/// </summary>
		override internal void Rewind()
		{
			Vector3 curV = (Vector3)( GetValue() );
			curV.z = typedStartVal;
			
			SetValue( curV );
		}
		
		/// <summary>
		/// Completes the tween.
		/// Should be overriden by tweens that control only part of the property (like HOTPluginVector3X).
		/// </summary>
		override internal void Complete()
		{
			Vector3 curV = (Vector3)( GetValue() );
			curV.z = typedEndVal;
			
			SetValue( curV );
		}
		
		/// <summary>
		/// Updates the tween.
		/// </summary>
		/// <param name="p_totElapsed">
		/// The total elapsed time since startup.
		/// </param>
		override protected void DoUpdate ( float p_totElapsed )
		{
			Vector3 curV = (Vector3)( GetValue() );
			curV.z = ease( p_totElapsed, typedStartVal, changeVal, _duration );
			
			SetValue( curV );
		}
	}
}

