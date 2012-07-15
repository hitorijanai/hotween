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
    [CustomEditor(typeof(HOTween))]
    public class HOTweenInspector : UnityEditor.Editor
    {
        enum TweenGroup
        {
            Running,
            Paused,
            Disabled
        }

        int _labelsWidth = 150;
        int _fieldsWidth = 60;

        // ===================================================================================
        // UNITY METHODS ---------------------------------------------------------------------

        override public void OnInspectorGUI()
        {
            HOGUIStyle.InitGUI();

            EditorGUIUtility.LookLikeControls(_labelsWidth, _fieldsWidth);

            GUILayout.Space(4);

            TweenInfo[] twInfos = HOTween.GetTweenInfos();
            if (twInfos == null) {
                GUILayout.Label("No tweens");
                return;
            }

            // Store and display tot running/paused/disabled tweens.
            int totTweens = twInfos.Length;
            List<TweenInfo> runningTweens = new List<TweenInfo>();
            List<TweenInfo> pausedTweens = new List<TweenInfo>();
            List<TweenInfo> disabledTweens = new List<TweenInfo>();
            foreach (TweenInfo twInfo in twInfos) {
                if (!twInfo.isEnabled) {
                    disabledTweens.Add(twInfo);
                } else if (twInfo.isPaused) {
                    pausedTweens.Add(twInfo);
                } else {
                    runningTweens.Add(twInfo);
                }
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Tweens (tot - running/paused/disabled): " + totTweens + " - " + runningTweens.Count + "/" + pausedTweens.Count + "/" + disabledTweens.Count, HOGUIStyle.LabelCentered);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Draw play/pause/kill all buttons
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Play All", GUILayout.Width(76))) {
                HOTween.Play();
            }
            if (GUILayout.Button("Pause All", GUILayout.Width(76))) {
                HOTween.Pause();
            }
            if (GUILayout.Button("Kill All", GUILayout.Width(76))) {
                HOTween.Kill();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Display data for each tween (divided by running/paused)
            for (int i = 0; i < 3; ++i) {
                TweenGroup twGroup = i == 0 ? TweenGroup.Running : i == 1 ? TweenGroup.Paused : TweenGroup.Disabled;
                List<TweenInfo> targetInfos = twGroup == TweenGroup.Running ? runningTweens : twGroup == TweenGroup.Paused ? pausedTweens : disabledTweens;
                if (targetInfos.Count == 0) continue;
                GUILayout.Space(8);
                GUILayout.BeginVertical(HOGUIStyle.BoxStyleRegular);
                GUILayout.BeginHorizontal();
                GUILayout.Label((twGroup == TweenGroup.Running ? "Running" : twGroup == TweenGroup.Paused ? "Paused" : "Disabled") + " Tweens (" + targetInfos.Count + ")", HOGUIStyle.TitleStyle, GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                GUILayout.Label("Click a target to select it");
                GUILayout.EndHorizontal();
                GUILayout.Space(6);
                foreach (TweenInfo twInfo in targetInfos) {
                    GUILayout.BeginVertical(GUI.skin.box);
                    if (twInfo.isSequence) {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("[Sequence]", HOGUIStyle.LabelSmallStyle);
                        if (twGroup != TweenGroup.Disabled) DrawTargetButtons(twInfo, twGroup);
                        GUILayout.EndHorizontal();
                        DrawInfo(twInfo);
                        foreach (object twTarget in twInfo.targets) {
                            DrawTarget(twInfo, twTarget, twGroup, true);
                        }
                    } else {
                        DrawTarget(twInfo, twInfo.targets[0], twGroup, false);
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }

        // ===================================================================================
        // METHODS ---------------------------------------------------------------------------

        void DrawTarget(TweenInfo twInfo, object twTarget, TweenGroup twGroup, bool isSequenced)
        {
            GUILayout.BeginHorizontal();
            if (isSequenced) GUILayout.Space(9);
            if (GUILayout.Button(twTarget.ToString(), HOGUIStyle.BtLabelStyle)) {
                SelectTargetGameObject(twTarget);
            }
            if (!isSequenced && twGroup != TweenGroup.Disabled) {
                DrawTargetButtons(twInfo, twGroup);
            }
            GUILayout.EndHorizontal();
            if (!isSequenced && twGroup != TweenGroup.Disabled) {
                DrawInfo(twInfo);
            }
        }

        void DrawInfo(TweenInfo twInfo)
        {
            GUILayout.Label("Loops: " + twInfo.tween.completedLoops + "/" + twInfo.tween.loops, HOGUIStyle.LabelSmallStyle);
        }

        void DrawTargetButtons(TweenInfo twInfo, TweenGroup twGroup)
        {
            if (GUILayout.Button(twGroup == TweenGroup.Running ? "Pause" : "Play", HOGUIStyle.BtTinyStyle, GUILayout.Width(45))) {
                if (twGroup == TweenGroup.Running) {
                    twInfo.tween.Pause();
                } else {
                    twInfo.tween.Play();
                }
            }
            if (GUILayout.Button("Kill", HOGUIStyle.BtTinyStyle, GUILayout.Width(30))) {
                twInfo.tween.Kill();
            } 
        }

        /// <summary>
        /// If the given target is a Component or a MonoBehaviour returns the relative GameObject.
        /// </summary>
        void SelectTargetGameObject(object obj)
        {
            GameObject targetGO = null;
            MonoBehaviour mb = obj as MonoBehaviour;
            if (mb != null) {
                targetGO = mb.gameObject;
            } else {
                Component cmp = obj as Component;
                if (cmp != null) targetGO = cmp.gameObject;
            }
            if (targetGO != null) Selection.activeGameObject = targetGO;
        }
    }
}