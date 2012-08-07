// Author: Daniele Giardini
// Copyright (c) 2012 Daniele Giardini - Holoville - http://www.holoville.com
// Created: 2012/04/27 11:49

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Holoville.HOTween.Editor.Core
{
    /// <summary>
    /// INIT REQUIRED (must be called inside an OnGUI call).
    /// Stores common textures and GUIStyles used by windows and inspectors.
    /// </summary>
    internal static class HOGUIStyle
    {
        // PUBLIC ///////////////////////////////////////////////////////

        public const int VSpace = 6;
        public const int TinyToggleWidth = 15;
        public const int NormalButtonWidth = 80;
        public const int TinyButtonWidth = 20;

        public static GUIStyle TitleStyle;
        public static GUIStyle BoxStyleRegular;
        public static GUIStyle LabelCentered;
        public static GUIStyle LabelSmallStyle;
        public static GUIStyle LabelSmallItalicStyle;
        public static GUIStyle LabelBoldStyle;
        public static GUIStyle LabelItalicStyle;
        public static GUIStyle LabelBoldItalicStyle;
        public static GUIStyle LabelWordWrapStyle;
        public static GUIStyle BtNegStyle;
        public static GUIStyle BtTinyStyle;
        public static GUIStyle BtTinyNegStyle;
        public static GUIStyle BtLabelStyle;
        public static GUIStyle BtLabelBoldStyle;
        public static GUIStyle BtLabelErrorStyle;

        // NON-PUBLIC ///////////////////////////////////////////////////

        static readonly Color _NegativeColor = Color.red;

        static bool _initialized;


        // ***********************************************************************************
        // INIT
        // ***********************************************************************************

        static public void InitGUI()
        {
            if (_initialized) return;

            _initialized = true;

            // Assign GUIStyles
            StoreGUIStyles();
        }

        // ===================================================================================
        // PUBLIC METHODS --------------------------------------------------------------------

        public static GUIStyle Label(int size) { return Label(size, FontStyle.Normal, Color.clear); }
        public static GUIStyle Label(FontStyle fontStyle) { return Label(-1, fontStyle, Color.clear); }
        public static GUIStyle Label(Color color) { return Label(-1, FontStyle.Normal, color); }
        public static GUIStyle Label(int size, FontStyle fontStyle) { return Label(size, fontStyle, Color.clear); }
        public static GUIStyle Label(int size, Color color) { return Label(size, FontStyle.Normal, color); }
        public static GUIStyle Label(FontStyle fontStyle, Color color) { return Label(-1, fontStyle, color); }
        public static GUIStyle Label(int size, FontStyle fontStyle, Color color)
        {
            GUIStyle label = new GUIStyle(GUI.skin.label);
            if (size != -1) label.fontSize = size;
            label.fontStyle = fontStyle;
            if (color != Color.clear) label.normal.textColor = color;
            return label;
        }

        public static void SetStyleNormalTextColors(GUIStyle style, Color color)
        {
            style.normal.textColor = style.hover.textColor = style.active.textColor = color;
        }

        public static void SetStyleOnTextColors(GUIStyle style, Color color)
        {
            style.onNormal.textColor = style.onHover.textColor = style.onActive.textColor = color;
        }

        // ===================================================================================
        // METHODS ---------------------------------------------------------------------------

        static void StoreGUIStyles()
        {
            BoxStyleRegular = new GUIStyle(GUI.skin.box);
            BoxStyleRegular.margin = new RectOffset(0, 0, 0, 0);
            BoxStyleRegular.overflow = new RectOffset(1, 2, 0, 1);
            BoxStyleRegular.padding = new RectOffset(6, 6, 6, 6);

            TitleStyle = Label(12, FontStyle.Bold);

            LabelCentered = new GUIStyle(GUI.skin.label);
            LabelCentered.alignment = TextAnchor.LowerCenter;
            LabelSmallStyle = Label(10);
            LabelSmallStyle.margin = new RectOffset(0, 0, -6, 0);
            LabelSmallItalicStyle = new GUIStyle(LabelSmallStyle);
            LabelSmallItalicStyle.fontStyle = FontStyle.Italic;
            LabelBoldStyle = Label(FontStyle.Bold);
            LabelItalicStyle = Label(FontStyle.Italic);
            LabelBoldItalicStyle = Label(FontStyle.BoldAndItalic);

            LabelWordWrapStyle = new GUIStyle(GUI.skin.label) {wordWrap = true};

            BtNegStyle = new GUIStyle(GUI.skin.button);
            SetStyleNormalTextColors(BtNegStyle, _NegativeColor);

            BtTinyStyle = new GUIStyle(GUI.skin.button) {padding = {left = -2, right = 0, top = 2}};
            BtTinyNegStyle = new GUIStyle(BtTinyStyle);
            SetStyleNormalTextColors(BtTinyNegStyle, _NegativeColor);

            BtLabelStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.UpperLeft, normal = { background = null }, padding = { left = 2 } };
            BtLabelStyle.margin = new RectOffset(0, 0, -2, -2);
            BtLabelStyle.fontSize = 10;
            BtLabelBoldStyle = new GUIStyle(BtLabelStyle) { fontStyle = FontStyle.Bold };
            BtLabelErrorStyle = new GUIStyle(BtLabelBoldStyle);
            SetStyleNormalTextColors(BtLabelErrorStyle, _NegativeColor);
        }
    }
}