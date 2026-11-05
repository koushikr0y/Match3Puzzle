// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using FullSerializer;

namespace GameVanilla.Editor
{
    /// <summary>
    /// The base class used for the editor tabs.
    /// </summary>
    public class EditorTab
    {
        protected PuzzleMatchKitEditor parentEditor;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="editor">The parent editor.</param>
        public EditorTab(PuzzleMatchKitEditor editor)
        {
            parentEditor = editor;
        }

        /// <summary>
        /// Called when this tab is selected by the user.
        /// </summary>
        public virtual void OnTabSelected()
        {
        }

        /// <summary>
        /// Called when this tab is drawn.
        /// </summary>
        public virtual void Draw()
        {
        }

        /// <summary>
        /// Utility method to create and initialize a reorderable list.
        /// </summary>
        /// <param name="headerText">The header text.</param>
        /// <param name="elements">The list of elements contained in the list.</param>
        /// <param name="currentElement">A reference to the currently selected element of the list.</param>
        /// <param name="drawElement">Callback to invoke when an element of the list is drawn.</param>
        /// <param name="selectElement">Callback to invoke when an element of the list is selected.</param>
        /// <param name="createElement">Callback to invoke when an element of the list is created.</param>
        /// <param name="removeElement">Callback to invoke when an element of the list is removed.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ReorderableList SetupReorderableList<T>(
            string headerText,
            List<T> elements,
            ref T currentElement,
            Action<Rect, T> drawElement,
            Action<T> selectElement,
            Action createElement,
            Action<T> removeElement)
        {
            var list = new ReorderableList(elements, typeof(T), true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, headerText); },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = elements[index];
                    drawElement(rect, element);
                }
            };

            list.onSelectCallback = l =>
            {
                var selectedElement = elements[list.index];
                selectElement(selectedElement);
            };

            if (createElement != null)
            {
                list.onAddDropdownCallback = (buttonRect, l) =>
                {
                    createElement();
                };
            }

            list.onRemoveCallback = l =>
            {
                if (!EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this item?", "Yes", "No")
                )
                {
                    return;
                }
                var element = elements[l.index];
                removeElement(element);
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
            };

            return list;
        }

        /// <summary>
        /// Loads the json file at the specified path.
        /// </summary>
        /// <param name="path">The path to the json file.</param>
        /// <typeparam name="T">The type of the data to which to deserialize the file to.</typeparam>
        /// <returns></returns>
        protected T LoadJsonFile<T>(string path) where T : class
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var file = new StreamReader(path);
            var fileContents = file.ReadToEnd();
            var data = fsJsonParser.Parse(fileContents);
            object deserialized = null;
            var serializer = new fsSerializer();
            serializer.TryDeserialize(data, typeof(T), ref deserialized).AssertSuccessWithoutWarnings();
            file.Close();
            return deserialized as T;
        }

        /// <summary>
        /// Saves the specified data to a json file at the specified path.
        /// </summary>
        /// <param name="path">The path to the json file.</param>
        /// <param name="data">The data to save.</param>
        /// <typeparam name="T">The type of the data to serialize to the file.</typeparam>
        protected void SaveJsonFile<T>(string path, T data) where T : class
        {
            fsData serializedData;
            var serializer = new fsSerializer();
            serializer.TrySerialize(data, out serializedData).AssertSuccessWithoutWarnings();
            var file = new StreamWriter(path);
            var json = fsJsonPrinter.PrettyJson(serializedData);
            file.WriteLine(json);
            file.Close();
        }
    }
}
