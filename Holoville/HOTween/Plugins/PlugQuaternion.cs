//
// PlugQuaternion.cs
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
using Holoville.HOTween.Plugins.Core;
using UnityEngine;

namespace Holoville.HOTween.Plugins
{
    /// <summary>
    /// Default plugin for the tweening of Quaternion objects.
    /// </summary>
    public class PlugQuaternion : ABSTweenPlugin
    {
        // VARS ///////////////////////////////////////////////////

        internal static Type[] validPropTypes = {typeof(Quaternion)};
        internal static Type[] validValueTypes = {typeof(Vector3), typeof(Quaternion)};

        Vector3 typedStartVal;
        Vector3 typedEndVal;
        Vector3 changeVal;
        bool beyond360;

        // GETS/SETS //////////////////////////////////////////////

        /// <summary>
        /// Gets the untyped start value,
        /// sets both the untyped and the typed start value.
        /// </summary>
        protected override object startVal
        {
            get
            {
                return _startVal;
            }
            set
            {
                if (tweenObj.isFrom && isRelative)
                {
                    typedStartVal = typedEndVal + (value is Quaternion ? ((Quaternion)value).eulerAngles : (Vector3)value);
                    _startVal = Quaternion.Euler(typedStartVal);
                }
                else
                {
                    _startVal = value;
                    typedStartVal = (value is Quaternion ? ((Quaternion)value).eulerAngles : (Vector3)value);
                }
            }
        }

        /// <summary>
        /// Gets the untyped end value,
        /// sets both the untyped and the typed end value.
        /// </summary>
        protected override object endVal
        {
            get
            {
                return _endVal;
            }
            set
            {
                _endVal = value;
                typedEndVal = (value is Quaternion ? ((Quaternion)value).eulerAngles : (Vector3)value);
            }
        }


        // ***********************************************************************************
        // CONSTRUCTOR
        // ***********************************************************************************

        /// <summary>
        /// Creates a new instance of this plugin using the main ease type.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="Quaternion"/> value to tween to.
        /// </param>
        public PlugQuaternion(Quaternion p_endVal)
            : base(p_endVal, false)
        {
        }

        /// <summary>
        /// Creates a new instance of this plugin.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="Quaternion"/> value to tween to.
        /// </param>
        /// <param name="p_easeType">
        /// The <see cref="EaseType"/> to use.
        /// </param>
        public PlugQuaternion(Quaternion p_endVal, EaseType p_easeType)
            : base(p_endVal, p_easeType, false)
        {
        }

        /// <summary>
        /// Creates a new instance of this plugin using the main ease type.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="Quaternion"/> value to tween to.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the given end value is considered relative instead than absolute.
        /// </param>
        public PlugQuaternion(Quaternion p_endVal, bool p_isRelative)
            : base(p_endVal, p_isRelative)
        {
        }

        /// <summary>
        /// Creates a new instance of this plugin.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="Quaternion"/> value to tween to.
        /// </param>
        /// <param name="p_easeType">
        /// The <see cref="EaseType"/> to use.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the given end value is considered relative instead than absolute.
        /// </param>
        public PlugQuaternion(Quaternion p_endVal, EaseType p_easeType, bool p_isRelative)
            : base(p_endVal, p_easeType, p_isRelative)
        {
        }

        /// <summary>
        /// Creates a new instance of this plugin using the main ease type.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="Vector3"/> euler angles to tween to.
        /// </param>
        public PlugQuaternion(Vector3 p_endVal)
            : base(p_endVal, false)
        {
        }

        /// <summary>
        /// Creates a new instance of this plugin.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="Vector3"/> euler angles to tween to.
        /// </param>
        /// <param name="p_easeType">
        /// The <see cref="EaseType"/> to use.
        /// </param>
        public PlugQuaternion(Vector3 p_endVal, EaseType p_easeType)
            : base(p_endVal, p_easeType, false)
        {
        }

        /// <summary>
        /// Creates a new instance of this plugin using the main ease type.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="Vector3"/> euler angles to tween to.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the given end value is considered relative instead than absolute.
        /// </param>
        public PlugQuaternion(Vector3 p_endVal, bool p_isRelative)
            : base(p_endVal, p_isRelative)
        {
        }

        /// <summary>
        /// Creates a new instance of this plugin.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="Vector3"/> euler angles to tween to.
        /// </param>
        /// <param name="p_easeType">
        /// The <see cref="EaseType"/> to use.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the given end value is considered relative instead than absolute.
        /// </param>
        public PlugQuaternion(Vector3 p_endVal, EaseType p_easeType, bool p_isRelative)
            : base(p_endVal, p_easeType, p_isRelative)
        {
        }

        // ===================================================================================
        // PARAMETERS ------------------------------------------------------------------------

        /// <summary>
        /// Parameter > Sets rotations to be calculated fully,
        /// and the end value will be reached using the full degrees of the given rotation, even if beyond 360 degrees.
        /// </summary>
        public PlugQuaternion Beyond360()
        {
            return Beyond360(true);
        }

        /// <summary>
        /// Parameter > Choose whether you want to calculate angles bigger than 360 degrees or not.
        /// In the first case, the end value will be reached using the full degrees of the given rotation.
        /// In the second case, the end value will be reached from the shortest direction.
        /// If the endValue is set as <c>relative</c>, this option will have no effect, and full beyond 360 rotations will always be used.
        /// </summary>
        /// <param name="p_beyond360">
        /// Set to <c>true</c> to use angles bigger than 360 degrees.
        /// </param>
        public PlugQuaternion Beyond360(bool p_beyond360)
        {
            beyond360 = p_beyond360;
            return this;
        }

        // ===================================================================================
        // METHODS ---------------------------------------------------------------------------

        /// <summary>
        /// Returns the speed-based duration based on the given speed x second.
        /// </summary>
        protected override float GetSpeedBasedDuration(float p_speed)
        {
            float speedDur = changeVal.magnitude/(p_speed*360);
            if (speedDur < 0)
            {
                speedDur = -speedDur;
            }
            return speedDur;
        }

        /// <summary>
        /// Sets the typed changeVal based on the current startVal and endVal.
        /// </summary>
        protected override void SetChangeVal()
        {
            if (isRelative && !tweenObj.isFrom)
            {
                changeVal = typedEndVal;
            }
            else
            {
                changeVal = new Vector3(typedEndVal.x - typedStartVal.x, typedEndVal.y - typedStartVal.y, typedEndVal.z - typedStartVal.z);

                if (beyond360)
                {
                    changeVal = typedEndVal - typedStartVal;
                }
                else
                {
                    Vector3 ev = typedEndVal;
                    if (ev.x > 360)
                    {
                        ev.x = ev.x%360;
                    }
                    if (ev.y > 360)
                    {
                        ev.y = ev.y%360;
                    }
                    if (ev.z > 360)
                    {
                        ev.z = ev.z%360;
                    }
                    changeVal = ev - typedStartVal;
                    float altX = 360 - typedStartVal.x + ev.x;
                    if (altX < (changeVal.x > 0 ? changeVal.x : -changeVal.x))
                    {
                        changeVal.x = altX;
                    }
                    float altY = 360 - typedStartVal.y + ev.y;
                    if (altY < (changeVal.y > 0 ? changeVal.y : -changeVal.y))
                    {
                        changeVal.y = altY;
                    }
                    float altZ = 360 - typedStartVal.z + ev.z;
                    if (altZ < (changeVal.z > 0 ? changeVal.z : -changeVal.z))
                    {
                        changeVal.z = altZ;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the correct values in case of Incremental loop type.
        /// </summary>
        /// <param name="p_diffIncr">
        /// The difference from the previous loop increment.
        /// </param>
        internal override void SetIncremental(int p_diffIncr)
        {
            typedStartVal += changeVal*p_diffIncr;
        }

        /// <summary>
        /// Updates the tween.
        /// </summary>
        /// <param name="p_totElapsed">
        /// The total elapsed time since startup.
        /// </param>
        protected override void DoUpdate(float p_totElapsed)
        {
            float x = ease(p_totElapsed, typedStartVal.x, changeVal.x, _duration);
            float y = ease(p_totElapsed, typedStartVal.y, changeVal.y, _duration);
            float z = ease(p_totElapsed, typedStartVal.z, changeVal.z, _duration);

            SetValue(Quaternion.Euler(new Vector3(x, y, z)));
        }
    }
}
