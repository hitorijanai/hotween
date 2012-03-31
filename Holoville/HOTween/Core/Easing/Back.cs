//
// Back.cs
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
    /// This class contains a C# port of the easing equations created by Robert Penner (http://robertpenner.com/easing).
    /// </summary>
    public static class Back
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
        public static float EaseIn(float t, float b, float c, float d)
        {
            return EaseIn(t, b, c, d, 1.70158f);
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
        /// <param name="s">
        /// Back factor.
        /// </param>
        /// <returns>
        /// A <see cref="System.Single"/>
        /// </returns>
        public static float EaseIn(float t, float b, float c, float d, float s)
        {
            return c*(t /= d)*t*((s + 1)*t - s) + b;
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
        public static float EaseOut(float t, float b, float c, float d)
        {
            return EaseOut(t, b, c, d, 1.70158f);
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
        /// <param name="s">
        /// Back factor.
        /// </param>
        /// <returns>
        /// A <see cref="System.Single"/>
        /// </returns>
        public static float EaseOut(float t, float b, float c, float d, float s)
        {
            return c*((t = t/d - 1)*t*((s + 1)*t + s) + 1) + b;
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
        public static float EaseInOut(float t, float b, float c, float d)
        {
            return EaseInOut(t, b, c, d, 1.70158f);
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
        /// <param name="s">
        /// Back factor.
        /// </param>
        /// <returns>
        /// A <see cref="System.Single"/>
        /// </returns>
        public static float EaseInOut(float t, float b, float c, float d, float s)
        {
            if ((t /= d*0.5f) < 1)
            {
                return c*0.5f*(t*t*(((s *= (1.525f)) + 1)*t - s)) + b;
            }
            return c/2*((t -= 2)*t*(((s *= (1.525f)) + 1)*t + s) + 2) + b;
        }
    }
}
