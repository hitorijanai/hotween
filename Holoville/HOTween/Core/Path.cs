// 
// Path.cs
//  
// Author: Daniele Giardini
// 
// Copyright (c) 2012 Daniele Giardini - Holoville - http://www.holoville.com
// Contains code from Andeeee's CRSpline (http://forum.unity3d.com/threads/32954-Waypoints-and-constant-variable-speed-problems?p=213942&viewfull=1#post213942)
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
using System.Collections.Generic;

namespace Holoville.HOTween.Core
{
	/// <summary>
	/// Used to manage movement on a Cardinal spline (of Catmull-Rom type).
	/// Contains code from Andeeee's CRSpline (http://forum.unity3d.com/threads/32954-Waypoints-and-constant-variable-speed-problems).
	/// </summary>
	internal class Path
	{
		// VARS ///////////////////////////////////////////////////
		
		internal		Vector3[]						path;
		internal		bool							changed; // Used by incremental loops to tell that drawPs should be recalculated.
		
		private			Vector3[]						drawPs; // Used by GizmoDraw to store point only once.
		
		
		// ***********************************************************************************
		// CONSTRUCTOR
		// ***********************************************************************************
		
		/// <summary>
		/// Creates a new <see cref="Path"/> based on the given array of <see cref="Vector3"/> points.
		/// </summary>
		/// <param name="p_path">
		/// The <see cref="Vector3"/> array used to create the path.
		/// </param>
		public Path( params Vector3[] p_path )
		{
			path = new Vector3[p_path.Length];
			Array.Copy( p_path, path, path.Length );
		}
		
		// ===================================================================================
		// METHODS ---------------------------------------------------------------------------
		
		/// <summary>
		/// Gets the point on the curve at the given time position.
		/// </summary>
		public Vector3 GetPoint( float t )
		{
			int numSections = path.Length - 3;
			int tSec = Mathf.FloorToInt( t * numSections );
			int currPt = numSections - 1;
			if ( currPt > tSec )		currPt = tSec;
			float u = t * numSections - currPt;
			
			Vector3 a = path[currPt];
			Vector3 b = path[currPt + 1];
			Vector3 c = path[currPt + 2];
			Vector3 d = path[currPt + 3];
			
			return .5f * (
				( -a + 3f * b - 3f * c + d ) * ( u * u * u )
				+ ( 2f * a - 5f * b + 4f * c - d ) * ( u * u )
				+ ( -a + c ) * u
				+ 2f * b
			);
		}
		
		/// <summary>
		/// Gets the velocity at the given time position.
		/// </summary>
		public Vector3 Velocity( float t )
		{
			int numSections = path.Length - 3;
			int tSec = (int)Mathf.Floor( t * numSections );
			int currPt = numSections - 1;
			if ( currPt > tSec )		currPt = tSec;
			float u = t * numSections - currPt;
			
			Vector3 a = path[currPt];
			Vector3 b = path[currPt + 1];
			Vector3 c = path[currPt + 2];
			Vector3 d = path[currPt + 3];
			
			return 1.5f * ( -a + 3f * b - 3f * c + d ) * ( u * u )
				+ ( 2f * a -5f * b + 4f * c - d ) * u
				+ .5f * c - .5f * a;
		}
		
		/// <summary>
		/// Draws the full path.
		/// </summary>
		public void GizmoDraw() { GizmoDraw( -1, false ); }
		/// <summary>
		/// Draws the full path, and if <c>t</c> is not -1 also draws the velocity at <c>t</c>.
		/// </summary>
		/// <param name="t">
		/// The point where to calculate velocity and eventual additional trigonometry.
		/// </param>
		/// <param name="p_drawTrig">
		/// If <c>true</c> also draws the normal, tangent, and binormal of t.
		/// </param>
		public void GizmoDraw( float t, bool p_drawTrig )
		{
			Gizmos.color = new Color( 0.6f,0.6f,0.6f,0.6f );
			
			Vector3 currPt;
			if ( changed || drawPs == null ) {
				changed = false;
				// Store draw points.
				float pm;
				int subdivisions = path.Length * 10;
				drawPs = new Vector3[subdivisions + 1];
				for ( int i = 0; i <= subdivisions; ++i ) {
					pm = i / (float)subdivisions;
					currPt = GetPoint( pm );
					drawPs[i] = currPt;
				}
			}
			// Draw path.
			Vector3 prevPt = drawPs[0];
			for ( int i = 1; i < drawPs.Length; ++i ) {
				currPt = drawPs[i];
				Gizmos.DrawLine( currPt, prevPt );
				prevPt = currPt;
			}
			// Draw path control points.
			Gizmos.color = Color.white;
			for ( int i = 1; i < path.Length - 1; ++i ) {
				Gizmos.DrawSphere( path[i], 0.1f );
			}
			
			if ( p_drawTrig && t != -1 ) {
				Vector3 pos = GetPoint( t );
				Vector3 prevP;
				Vector3 p = pos;
				Vector3 nextP;
				float nextT = t + 0.0001f;
				if ( nextT > 1 ) {
					nextP = pos;
					p = GetPoint( t - 0.0001f );
					prevP = GetPoint( t - 0.0002f );
				} else {
					float prevT = t - 0.0001f;
					if ( prevT < 0 ) {
						prevP = pos;
						p = GetPoint( t + 0.0001f );
						nextP = GetPoint( t + 0.0002f );
					} else {
						prevP = GetPoint( prevT );
						nextP = GetPoint( nextT );
					}
				}
				Vector3 tangent = nextP - p;
				tangent.Normalize();
				Vector3 tangent2 = p - prevP;
				tangent2.Normalize();
				Vector3 normal = Vector3.Cross( tangent, tangent2 );
				normal.Normalize();
				Vector3 binormal = Vector3.Cross( tangent, normal );
				binormal.Normalize();
				// Draw normal.
				Gizmos.color = Color.black;
				Gizmos.DrawLine( pos, pos + tangent );
				Gizmos.color = Color.blue;
				Gizmos.DrawLine( pos, pos + normal );
				Gizmos.color = Color.red;
				Gizmos.DrawLine( pos, pos + binormal );
			}
		}
		
		// ===================================================================================
		// INTERNAL METHODS ------------------------------------------------------------------
		
		internal Dictionary<float,float> GetTimeToArcLenTable( int p_subdivisions, out float out_fullLen )
		{
			float incr = 1f / p_subdivisions;
			float t;
			float len = 0;
			float prevLen = 0;
			Vector3 p;
			Vector3 prevP = Vector3.zero;
			Dictionary<float,float> dc = new Dictionary<float, float>();
			for ( int i = 0; i < p_subdivisions + 1; ++i ) {
				if ( i == 0 ) {
					prevP = GetPoint( 0 );
					prevLen = 0;
				} else {
					t = incr * i;
					p = GetPoint( t );
					len = prevLen + Vector3.Distance( p, prevP );
					dc.Add( t, len );
					prevP = p;
					prevLen = len;
				}
			}
			out_fullLen = len;
			return dc;
		}
	}
}

