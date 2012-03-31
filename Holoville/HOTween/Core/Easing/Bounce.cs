//
// Bounce.cs
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

namespace Holoville.HOTween.Core.Easing
{
    /// <summary>
    /// This class contains a C# port of the easing equations created by Robert Penner (http://http://robertpenner.com/easing).
    /// </summary>
    static public class Bounce
    {
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
        public static float EaseIn ( float t, float b, float c, float d )
        {
            return c - EaseOut(d-t, 0, c, d) + b;
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
        public static float EaseOut ( float t, float b, float c, float d )
        {
            if ((t/=d) < (1/2.75f)) {
                return c*(7.5625f*t*t) + b;
            }
            if (t < (2/2.75f)) {
                return c*(7.5625f*(t-=(1.5f/2.75f))*t + 0.75f) + b;
            }
            if (t < (2.5f/2.75f)) {
                return c*(7.5625f*(t-=(2.25f/2.75f))*t + 0.9375f) + b;
            }
            return c*(7.5625f*(t-=(2.625f/2.75f))*t + 0.984375f) + b;
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
        public static float EaseInOut ( float t, float b, float c, float d )
        {
            if (t < d*0.5f) return EaseIn (t*2, 0, c, d) * 0.5f + b;
            return EaseOut (t*2-d, 0, c, d) * 0.5f + c*0.5f + b;
        }
    }
}

