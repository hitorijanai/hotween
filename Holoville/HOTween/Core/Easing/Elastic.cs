// 
// Elastic.cs
//  
// Author: Daniele Giardini (C# port of the easing equations created by Robert Penner - http://robertpenner.com/easing)
// 
// TERMS OF USE - EASING EQUATIONS
//
// Open source under the BSD License. 
//
// Copyright © 2001 Robert Penner
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// 
// - Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// - Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
// - Neither the name of the author nor the names of contributors may be used to endorse
// or promote products derived from this software without specific prior written permission.
// - THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using UnityEngine;

namespace Holoville.HOTween.Core.Easing
{
	/// <summary>
	/// This class contains a C# port of the easing equations created by Robert Penner (http://http://robertpenner.com/easing).
	/// </summary>
	static public class Elastic
	{
		private	const	float	_2PI = Mathf.PI * 2;
		
		/// <summary>
		/// Tween. 
		/// </summary>
		/// <param name="t">
		/// Time.
		/// </param>
		/// <param name="b">
		/// Begin value.
		/// </param>
		/// <param name="c">
		/// Change value.
		/// </param>
		/// <param name="d">
		/// Duration.
		/// </param>
		/// <returns>
		/// A <see cref="System.Single"/>
		/// </returns>
		public static float EaseIn ( float t, float b, float c, float d ) { return EaseIn( t, b, c, d, 0, 0 ); }
		/// <summary>
		/// Tween. 
		/// </summary>
		/// <param name="t">
		/// Time.
		/// </param>
		/// <param name="b">
		/// Begin value.
		/// </param>
		/// <param name="c">
		/// Change value.
		/// </param>
		/// <param name="d">
		/// Duration.
		/// </param>
		/// <param name="a">
		/// Variable.
		/// </param>
		/// <param name="p">
		/// Variable.
		/// </param>
		/// <returns>
		/// A <see cref="System.Single"/>
		/// </returns>
		public static float EaseIn ( float t, float b, float c, float d, float a, float p )
		{
			float s;
			if (t==0) return b;  if ((t/=d)==1) return b+c;  if (p==0) p=d*0.3f;
			if (a==0 || (c > 0 && a < c) || (c < 0 && a < -c)) { a=c; s = p/4; }
			else s = p/_2PI * Mathf.Asin (c/a);
			return -(a*Mathf.Pow(2,10*(t-=1)) * Mathf.Sin( (t*d-s)*_2PI/p )) + b;
		}
		
		/// <summary>
		/// Tween. 
		/// </summary>
		/// <param name="t">
		/// Time.
		/// </param>
		/// <param name="b">
		/// Begin value.
		/// </param>
		/// <param name="c">
		/// Change value.
		/// </param>
		/// <param name="d">
		/// Duration.
		/// </param>
		/// <returns>
		/// A <see cref="System.Single"/>
		/// </returns>
		public static float EaseOut ( float t, float b, float c, float d ) { return EaseOut( t, b, c, d, 0, 0 ); }
		/// <summary>
		/// Tween. 
		/// </summary>
		/// <param name="t">
		/// Time.
		/// </param>
		/// <param name="b">
		/// Begin value.
		/// </param>
		/// <param name="c">
		/// Change value.
		/// </param>
		/// <param name="d">
		/// Duration.
		/// </param>
		/// <param name="a">
		/// Variable.
		/// </param>
		/// <param name="p">
		/// Variable.
		/// </param>
		/// <returns>
		/// A <see cref="System.Single"/>
		/// </returns>
		public static float EaseOut ( float t, float b, float c, float d, float a, float p )
		{
			float s;
			if (t==0) return b;  if ((t/=d)==1) return b+c;  if (p==0) p=d*0.3f;
			if (a==0 || (c > 0 && a < c) || (c < 0 && a < -c)) { a=c; s = p/4; }
			else s = p/_2PI * Mathf.Asin (c/a);
			return (a*Mathf.Pow(2,-10*t) * Mathf.Sin( (t*d-s)*_2PI/p ) + c + b);
		}
		
		/// <summary>
		/// Tween. 
		/// </summary>
		/// <param name="t">
		/// Time.
		/// </param>
		/// <param name="b">
		/// Begin value.
		/// </param>
		/// <param name="c">
		/// Change value.
		/// </param>
		/// <param name="d">
		/// Duration.
		/// </param>
		/// <returns>
		/// A <see cref="System.Single"/>
		/// </returns>
		public static float EaseInOut ( float t, float b, float c, float d ) { return EaseInOut( t, b, c, d, 0, 0 ); }
		/// <summary>
		/// Tween. 
		/// </summary>
		/// <param name="t">
		/// Time.
		/// </param>
		/// <param name="b">
		/// Begin value.
		/// </param>
		/// <param name="c">
		/// Change value.
		/// </param>
		/// <param name="d">
		/// Duration.
		/// </param>
		/// <param name="a">
		/// Variable.
		/// </param>
		/// <param name="p">
		/// Variable.
		/// </param>
		/// <returns>
		/// A <see cref="System.Single"/>
		/// </returns>
		public static float EaseInOut ( float t, float b, float c, float d, float a, float p )
		{
			float s;
			if (t==0) return b;  if ((t/=d*0.5f)==2) return b+c;  if (p==0) p=d*(0.3f*1.5f);
			if (a==0 || (c > 0 && a < c) || (c < 0 && a < -c)) { a=c; s = p/4; }
			else s = p/_2PI * Mathf.Asin (c/a);
			if (t < 1) return -0.5f*(a*Mathf.Pow(2,10*(t-=1)) * Mathf.Sin( (t*d-s)*_2PI/p )) + b;
			return a*Mathf.Pow(2,-10*(t-=1)) * Mathf.Sin( (t*d-s)*_2PI/p )*0.5f + c + b;
		}
	}
}

