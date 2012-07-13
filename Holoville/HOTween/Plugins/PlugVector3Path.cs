//
// PlugVector3Path.cs
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
using System.Collections.Generic;
using System.Reflection;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins.Core;
using UnityEngine;

namespace Holoville.HOTween.Plugins
{
    /// <summary>
    /// Plugin for the tweening of Vector3 objects along a Vector3 path.
    /// </summary>
    public class PlugVector3Path : ABSTweenPlugin
    {
        // ENUMS //////////////////////////////////////////////////

        enum OrientType
        {
            None,
            ToPath,
            LookAtTransform,
            LookAtPosition
        }

        // VARS ///////////////////////////////////////////////////

        internal static Type[] validPropTypes = {typeof(Vector3)};
        internal static Type[] validValueTypes = {typeof(Vector3[])};

        internal Path path; // Internal so that HOTween OnDrawGizmo can find it and draw the paths.
        internal float pathPerc; // Stores the current percentage of the path, so that HOTween's OnDrawGizmo can show its velocity.
        internal bool hasAdditionalStartingP; // True if the path was created with an additional starting point

        const float EPSILON = 0.001f; // Used for floating points comparison
        Vector3 typedStartVal;
        Vector3[] points;
        Vector3 diffChangeVal; // Used for incremental loops.
        bool isPartialPath;
        internal bool isClosedPath;
        OrientType orientType = OrientType.None;
        float lookAheadVal = 0.0001f;
        Axis lockAxis = Axis.None;
        Vector3 lockRot; // Stores initial rotation axis, so they can be locked.

        // REFERENCES /////////////////////////////////////////////

        Vector3 lookPos;
        Transform lookTrans;
        Transform orientTrans;

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
                if (tweenObj.isFrom)
                {
                    _endVal = value;
                    Vector3[] ps = (Vector3[])value;
                    points = new Vector3[ps.Length];
                    Array.Copy(ps, points, ps.Length);
                    Array.Reverse(points);
                }
                else
                {
                    _startVal = typedStartVal = (Vector3)value;
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
                if (tweenObj.isFrom)
                {
                    _startVal = typedStartVal = (Vector3)value;
                }
                else
                {
                    _endVal = value;
                    Vector3[] ps = (Vector3[])value;
                    points = new Vector3[ps.Length];
                    Array.Copy(ps, points, ps.Length);
                }
            }
        }


        // ***********************************************************************************
        // CONSTRUCTOR
        // ***********************************************************************************

        /// <summary>
        /// Creates a new instance of this plugin using the main ease type and an absolute path.
        /// </summary>
        /// <param name="p_path">
        /// The <see cref="Vector3"/> path to tween through.
        /// </param>
        public PlugVector3Path(Vector3[] p_path) : base(p_path, false) {}
        /// <summary>
        /// Creates a new instance of this plugin using an absolute path.
        /// </summary>
        /// <param name="p_path">
        /// The <see cref="Vector3"/> path to tween through.
        /// </param>
        /// <param name="p_easeType">
        /// The <see cref="EaseType"/> to use.
        /// </param>
        public PlugVector3Path(Vector3[] p_path, EaseType p_easeType) : base(p_path, p_easeType, false) {}
        /// <summary>
        /// Creates a new instance of this plugin using the main ease type.
        /// </summary>
        /// <param name="p_path">
        /// The <see cref="Vector3"/> path to tween through.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the path is considered relative to the starting value of the property, instead than absolute.
        /// Not compatible with <c>HOTween.From</c>.
        /// </param>
        public PlugVector3Path(Vector3[] p_path, bool p_isRelative) : base(p_path, p_isRelative) {}
        /// <summary>
        /// Creates a new instance of this plugin.
        /// </summary>
        /// <param name="p_path">
        /// The <see cref="Vector3"/> path to tween through.
        /// </param>
        /// <param name="p_easeType">
        /// The <see cref="EaseType"/> to use.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the path is considered relative to the starting value of the property, instead than absolute.
        /// Not compatible with <c>HOTween.From</c>.
        /// </param>
        public PlugVector3Path(Vector3[] p_path, EaseType p_easeType, bool p_isRelative) : base(p_path, p_easeType, p_isRelative) {}
        /// <summary>
        /// Creates a new instance of this plugin.
        /// </summary>
        /// <param name="p_path">
        /// The <see cref="Vector3"/> path to tween through.
        /// </param>
        /// <param name="p_easeType">
        /// The <see cref="EaseType"/> to use.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the path is considered relative to the starting value of the property, instead than absolute.
        /// Not compatible with <c>HOTween.From</c>.
        /// </param>
        /// <param name="p_isPartialPath">
        /// If TRUE indicates that the path is partial, and will be created accordingly,
        /// and the p_path array will be used as is.
        /// Used by <see cref="Tweener"/> when creating a new partial PlugVector3Path.
        /// </param>
        public PlugVector3Path(Vector3[] p_path, EaseType p_easeType, bool p_isRelative, bool p_isPartialPath) : base(p_path, p_easeType, p_isRelative)
        {
            isPartialPath = p_isPartialPath;
        }

        /// <summary>
        /// Init override.
        /// Used to check that isRelative is FALSE,
        /// and otherwise use the given parameters to send a decent warning message.
        /// </summary>
        internal override void Init(Tweener p_tweenObj, string p_propertyName, EaseType p_easeType, Type p_targetType, PropertyInfo p_propertyInfo, FieldInfo p_fieldInfo)
        {
            if (isRelative && p_tweenObj.isFrom)
            {
                isRelative = false;
                TweenWarning.Log("\"" + p_tweenObj.target + "." + p_propertyName + "\": PlugVector3Path \"isRelative\" parameter is incompatible with HOTween.From. The tween will be treated as absolute.");
            }

            base.Init(p_tweenObj, p_propertyName, p_easeType, p_targetType, p_propertyInfo, p_fieldInfo);
        }

        // ===================================================================================
        // PARAMETERS ------------------------------------------------------------------------

        /// <summary>
        /// Parameter > Smoothly closes the path, so that it can be used for cycling loops.
        /// </summary>
        /// <returns>
        /// A <see cref="PlugVector3Path"/>
        /// </returns>
        public PlugVector3Path ClosePath()
        {
            return ClosePath(true);
        }

        /// <summary>
        /// Parameter > Choose whether to smoothly close the path, so that it can be used for cycling loops.
        /// </summary>
        /// <param name="p_close">
        /// Set to <c>true</c> to close the path.
        /// </param>
        public PlugVector3Path ClosePath(bool p_close)
        {
            isClosedPath = p_close;
            return this;
        }

        /// <summary>
        /// Parameter > If the tween target is a <see cref="Transform"/>, orients the tween target to the path.
        /// </summary>
        /// <returns>
        /// A <see cref="PlugVector3Path"/>
        /// </returns>
        public PlugVector3Path OrientToPath()
        {
            return OrientToPath(true);
        }

        /// <summary>
        /// Parameter > Choose whether to orient the tween target to the path (only if it's a <see cref="Transform"/>).
        /// </summary>
        /// <param name="p_orient">
        /// Set to <c>true</c> to orient the tween target to the path.
        /// </param>
        public PlugVector3Path OrientToPath(bool p_orient)
        {
            return OrientToPath(p_orient, 0.0001f, Axis.None);
        }

        /// <summary>
        /// Parameter > If the tween target is a <see cref="Transform"/>, orients the tween target to the path,
        /// using the given lookAhead percentage.
        /// </summary>
        /// <param name="p_lookAhead">
        /// The look ahead percentage (0 to 1).
        /// </param>
        public PlugVector3Path OrientToPath(float p_lookAhead)
        {
            return OrientToPath(true, p_lookAhead, Axis.None);
        }

        /// <summary>
        /// Parameter > If the tween target is a <see cref="Transform"/>, orients the tween target to the path,
        /// locking it's rotation on the given axis.
        /// </summary>
        /// <param name="p_lockAxis_alphaDontUseIt">
        /// WARNING: do NOT use, still in alpha.
        /// Sets one or more axis to lock while rotating.
        /// To lock more than one axis, use the bitwise OR operator (ex: <c>Axis.X | Axis.Y</c>).
        /// </param>
        public PlugVector3Path OrientToPath(Axis p_lockAxis_alphaDontUseIt)
        {
            return OrientToPath(true, 0.0001f, p_lockAxis_alphaDontUseIt);
        }

        /// <summary>
        /// Parameter > Choose whether to orient the tween target to the path (only if it's a <see cref="Transform"/>),
        /// and which lookAhead percentage to use.
        /// </summary>
        /// <param name="p_orient">
        /// Set to <c>true</c> to orient the tween target to the path.
        /// </param>
        /// <param name="p_lookAhead">
        /// The look ahead percentage (0 to 1).
        /// </param>
        /// <param name="p_lockAxis_alphaDontUseIt">
        /// WARNING: do NOT use, still in alpha.
        /// Sets one or more axis to lock while rotating.
        /// To lock more than one axis, use the bitwise OR operator (ex: <c>Axis.X | Axis.Y</c>).
        /// </param>
        public PlugVector3Path OrientToPath(bool p_orient, float p_lookAhead, Axis p_lockAxis_alphaDontUseIt)
        {
            if (p_orient)
            {
                orientType = OrientType.ToPath;
            }
            lookAheadVal = p_lookAhead;
            if (lookAheadVal < 0.0001f)
            {
                lookAheadVal = 0.0001f;
            }
            if (lookAheadVal > 0.9999f)
            {
                lookAheadVal = 0.9999f;
            }
            lockAxis = p_lockAxis_alphaDontUseIt;
            return this;
        }

        /// <summary>
        /// Parameter > If the tween target is a <see cref="Transform"/>, sets the tween so that the target will always look at the given transform.
        /// </summary>
        /// <param name="p_transform">
        /// The <see cref="Transform"/> to look at.
        /// </param>
        public PlugVector3Path LookAt(Transform p_transform)
        {
            if (p_transform != null)
            {
                orientType = OrientType.LookAtTransform;
                lookTrans = p_transform;
            }
            return this;
        }

        /// <summary>
        /// Parameter > If the tween target is a <see cref="Transform"/>, sets the tween so that the target will always look at the given position.
        /// </summary>
        /// <param name="p_position">
        /// The <see cref="Vector3"/> to look at.
        /// </param>
        public PlugVector3Path LookAt(Vector3 p_position)
        {
            orientType = OrientType.LookAtPosition;
            lookPos = p_position;
            lookTrans = null;
            return this;
        }

        // ===================================================================================
        // PRIVATE METHODS -------------------------------------------------------------------

        /// <summary>
        /// Returns the speed-based duration based on the given speed x second.
        /// </summary>
        protected override float GetSpeedBasedDuration(float p_speed)
        {
            return path.pathLength/p_speed;
        }

        /// <summary>
        /// Recreate the path if it was relative,
        /// otherwise simply add the correct starting and ending point so the path can be reached from the property's actual position.
        /// </summary>
        protected override void SetChangeVal()
        {
            if (orientType != OrientType.None)
            {
                // Store orient transform and lockRot.
                if (orientTrans == null)
                {
                    orientTrans = tweenObj.target as Transform;
                }
//                if ( lockAxis != Axis.None && orientTrans != null && !orientTrans.Equals( null ) ) {
//                    lockRot = orientTrans.rotation.eulerAngles;
//                }
            }

            // Create path.
            Vector3[] pts;
            if (isPartialPath) {
                // Use path as it is, since it was already being created correctly by Tweener.
                pts = points;
            } else {
                int indMod = 1;
                int pAdd = (isClosedPath ? 1 : 0);
                if (isRelative) {
                    // Path length is the same (plus control points).
                    hasAdditionalStartingP = false;
                    pts = new Vector3[points.Length + 2 + pAdd];
                    Vector3 diff = points[0] - typedStartVal;
                    for (int i = 0; i < points.Length; ++i) {
                        pts[i + indMod] = points[i] - diff;
                    }
                } else {
                    Vector3 currVal = (Vector3)GetValue();
                    // Calculate if currVal and start point are equal,
                    // managing floating point imprecision.
                    Vector3 diff = currVal - points[0];
                    if (diff.x < 0) diff.x = -diff.x;
                    if (diff.y < 0) diff.y = -diff.y;
                    if (diff.z < 0) diff.z = -diff.z;
                    if (diff.x < EPSILON && diff.y < EPSILON && diff.z < EPSILON) {
                        // Path length is the same (plus control points).
                        hasAdditionalStartingP = false;
                        pts = new Vector3[points.Length + 2 + pAdd];
                    } else {
                        // Path needs additional point for current value as starting point (plus control points).
                        hasAdditionalStartingP = true;
                        pts = new Vector3[points.Length + 3 + pAdd];
                        if (tweenObj.isFrom) {
                            pts[pts.Length - 2] = currVal;
                        } else {
                            pts[1] = currVal;
                            indMod = 2;
                        }
                    }
                    for (int i = 0; i < points.Length; ++i) {
                        pts[i + indMod] = points[i];
                    }
                }

                if (isClosedPath) {
                    // Close path.
                    pts[pts.Length - 2] = pts[1];
                }

                // Add control points.
                if (isClosedPath) {
                    pts[0] = pts[pts.Length - 3];
                    pts[pts.Length - 1] = pts[2];
                } else {
                    pts[0] = pts[1];
                    Vector3 lastP = pts[pts.Length - 2];
                    Vector3 diffV = lastP - pts[pts.Length - 3];
                    pts[pts.Length - 1] = lastP + diffV;
                }
            }

            // Create the path.
            path = new Path(pts);

            // Store arc lengths tables for constant speed.
            path.StoreTimeToArcLenTables(path.path.Length * 4);

            if (!isClosedPath)
            {
                // Store the changeVal used for Incremental loops.
                diffChangeVal = pts[pts.Length - 2] - pts[1];
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
            if (isClosedPath)
            {
                return;
            }

            Vector3[] pathPs = path.path;
            for (int i = 0; i < pathPs.Length; ++i)
            {
                pathPs[i] += (diffChangeVal*p_diffIncr);
            }
            path.changed = true;
        }

        /// <summary>
        /// Updates the tween.
        /// </summary>
        /// <param name="p_totElapsed">
        /// The total elapsed time since startup.
        /// </param>
        protected override void DoUpdate(float p_totElapsed)
        {
            pathPerc = ease(p_totElapsed, 0, 1, _duration);
            SetValue(GetConstPointOnPath(pathPerc, true));

            if (orientType != OrientType.None && orientTrans != null && !orientTrans.Equals(null))
            {
                switch (orientType)
                {
                case OrientType.LookAtPosition:
                    orientTrans.LookAt(lookPos, Vector3.up);
                    break;
                case OrientType.LookAtTransform:
                    if (orientTrans != null && !orientTrans.Equals(null))
                    {
                        orientTrans.LookAt(lookTrans.position, Vector3.up);
                    }
                    break;
                case OrientType.ToPath:
                    float nextT = pathPerc + lookAheadVal;
                    if (nextT > 1)
                    {
                        nextT = (isClosedPath ? nextT - 1 : 1.000001f);
                    }
                    Vector3 nextP = path.GetPoint(nextT);
                    orientTrans.LookAt(nextP, orientTrans.up);
//                    if ( lockAxis != Axis.None ) {
//                        // FIXME LockAxis
//                        Vector3 rot = orientTrans.eulerAngles;
//                        if ( ( lockAxis & Axis.X ) == Axis.X )    rot.x = lockRot.x;
//                        if ( ( lockAxis & Axis.Y ) == Axis.Y )    rot.y = lockRot.y;
//                        if ( ( lockAxis & Axis.Z ) == Axis.Z )    rot.z = lockRot.z;
//                        orientTrans.rotation = Quaternion.Euler( rot );
//                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Returns the point at the given percentage (0 to 1),
        /// considering the path at constant speed.
        /// Used by DoUpdate and by Tweener.GetPointOnPath.
        /// </summary>
        /// <param name="t">
        /// The percentage (0 to 1) at which to get the point.
        /// </param>
        internal Vector3 GetConstPointOnPath(float t) { return GetConstPointOnPath(t, false); }
        /// <summary>
        /// Returns the point at the given percentage (0 to 1),
        /// considering the path at constant speed.
        /// Used by DoUpdate and by Tweener.GetPointOnPath.
        /// </summary>
        /// <param name="t">
        /// The percentage (0 to 1) at which to get the point.
        /// </param>
        /// <param name="p_updatePathPerc">
        /// IF <c>true</c> updates also <see cref="pathPerc"/> value
        /// (necessary if this method is called for an update).
        /// </param>
        internal Vector3 GetConstPointOnPath(float t, bool p_updatePathPerc)
        {
            if (p_updatePathPerc) {
                return path.GetConstPoint(t, out pathPerc);
            } else {
                return path.GetConstPoint(t);
            }
        }

        /// <summary>
        /// Returns the percentage of the path length occupied by the given path waypoints interval.
        /// </summary>
        internal float GetWaypointsLengthPercentage(int p_pathWaypointId0, int p_pathWaypointId1)
        {
            if (path.waypointsLength == null) path.StoreWaypointsLengths(4);
            float partialLen = 0;
            for (int i = p_pathWaypointId0; i < p_pathWaypointId1; ++i) {
                partialLen += path.waypointsLength[i];
            }
            float perc = partialLen / path.pathLength;
            if (perc > 1) perc = 1; // Limit in case of near errors (because full path length is calculated differently then sum of waypoints)
            return perc;
        }

        // ===================================================================================
        // HELPERS ---------------------------------------------------------------------------

        internal PlugVector3Path CloneForPartialPath(Vector3[] p_precisePath, EaseType p_easeType)
        {
            PlugVector3Path plugClone = new PlugVector3Path(p_precisePath, p_easeType, isRelative, true);
            switch (orientType) {
            case OrientType.ToPath:
                plugClone.OrientToPath(true, lookAheadVal, lockAxis);
                break;
            case OrientType.LookAtTransform:
                plugClone.LookAt(lookTrans);
                break;
            case OrientType.LookAtPosition:
                plugClone.LookAt(lookPos);
                break;
            }

            return plugClone;
        }
    }
}
