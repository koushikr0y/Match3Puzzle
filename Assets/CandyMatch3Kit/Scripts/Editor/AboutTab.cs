// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;
using UnityEngine;

namespace GameVanilla.Editor
{
    /// <summary>
    /// The "About" tab in the editor.
    /// </summary>
    public class AboutTab : EditorTab
    {
        private const string purchaseText = "Thank you for your purchase!";
        private const string copyrightText =
            "Candy Match 3 Kit is brought to you by gamevanilla. Copyright (C) gamevanilla 2018.";
        private const string wikiUrl = "https://wiki.gamevanilla.com";
        private const string supportUrl = "https://www.gamevanilla.com";
        private const string eulaUrl = "https://unity3d.com/es/legal/as_terms";
        private const string assetStoreUrl = "https://www.assetstore.unity3d.com/#!/content/111083";

        private readonly Texture2D logoTexture;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="editor">The parent editor.</param>
        public AboutTab(PuzzleMatchKitEditor editor) : base(editor)
        {
            logoTexture = Resources.Load<Texture2D>("Logo");
        }

        /// <summary>
        /// Called when this tab is drawn.
        /// </summary>
        public override void Draw()
        {
            GUILayout.Space(15);

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(logoTexture);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(15);

            var windowWidth = EditorWindow.focusedWindow.position.width;
            var centeredLabelStyle = new GUIStyle("label") {alignment = TextAnchor.MiddleCenter};
            GUI.Label(new Rect(0, 0, windowWidth, 650), purchaseText, centeredLabelStyle);
            GUI.Label(new Rect(0, 0, windowWidth, 700), copyrightText, centeredLabelStyle);
            var centeredButtonStyle = new GUIStyle("button") {alignment = TextAnchor.MiddleCenter};
            if (GUI.Button(new Rect(windowWidth / 2 - 100 / 2.0f, 400, 100, 50), "Documentation", centeredButtonStyle))
            {
                Application.OpenURL(wikiUrl);
            }
            if (GUI.Button(new Rect(windowWidth / 2 - 100 / 2.0f, 460, 100, 50), "Support", centeredButtonStyle))
            {
                Application.OpenURL(supportUrl);
            }
            if (GUI.Button(new Rect(windowWidth / 2 - 100 / 2.0f, 520, 100, 50), "License", centeredButtonStyle))
            {
                Application.OpenURL(eulaUrl);
            }
            if (GUI.Button(new Rect(windowWidth / 2 - 100 / 2.0f, 580, 100, 50), "Rate me!", centeredButtonStyle))
            {
                Application.OpenURL(assetStoreUrl);
            }
        }
    }
}
