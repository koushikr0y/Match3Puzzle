// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;

namespace GameVanilla.Editor
{
    /// <summary>
    /// Utility class for deleting the EditorPrefs from within the editor.
    /// </summary>
    public class DeleteEditorPrefs
    {
        [MenuItem("Window/Candy Match 3 Kit/Delete EditorPrefs", false, 2)]
        public static void DeleteAllEditorPrefs()
        {
            EditorPrefs.DeleteAll();
        }
    }
}

