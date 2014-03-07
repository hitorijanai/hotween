// Author: Daniele Giardini
// Copyright (c) 2012 Daniele Giardini - Holoville - http://www.holoville.com
// Created: 2012/07/07 12:56
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

using System.Collections.Generic;
using Holoville.HOTween.Core;
using Holoville.HOTween.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Holoville.HOTween.Editor
{
    /// <summary>
    /// Activated when selecting HOTween's instance during editor runtime.
    /// Used to show a series of debug informations.
    /// </summary>
    [CustomEditor(typeof(Object))]
    public class HOTweenDLLInspector : UnityEditor.Editor
    {
        bool _stylesSet;
        GUIStyle _wordrapStyle;

        override public void OnInspectorGUI()
        {
            if (target.name == "HOTween" || target.name == "HOTween.dll") {
                GUI.enabled = true;
                if (!_stylesSet) {
                    _stylesSet = true;
                    _wordrapStyle = new GUIStyle(GUI.skin.label);
                    _wordrapStyle.wordWrap = true;
                }
#if MICRO
                GUILayout.Label("HOTweenMicro v" + HOTween.VERSION
                    + "\n\nThis is the micro version of HOTween, necessary if you're building for Windows 8 Store/Phone or iOS with max stripping level. "
                    + "Otherwise, use the regular version of HOTween, which is slightly faster.",
                _wordrapStyle);
#else
                GUILayout.Label("HOTween v" + HOTween.VERSION
                    + "\n\nThis is the default version of HOTween. If you need compatibility with Windows 8 Store/Phone or iOS with max stripping level, "
                    + "download HOTweenMicro from HOTween's website.",
                _wordrapStyle);
#endif
            } else {
                DrawDefaultInspector();
            }
        }
    }
}