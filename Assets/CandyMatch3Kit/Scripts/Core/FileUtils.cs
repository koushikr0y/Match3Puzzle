// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

using FullSerializer;

namespace GameVanilla.Core
{
    /// <summary>
    /// Miscellaneous file utilities.
    /// </summary>
    public static class FileUtils
    {
        /// <summary>
        /// Loads the specified json file.
        /// </summary>
        /// <param name="serializer">The FullSerializer serializer to use.</param>
        /// <param name="path">The json file path.</param>
        /// <typeparam name="T">The type of the data to load.</typeparam>
        /// <returns>The loaded json data.</returns>
        public static T LoadJsonFile<T>(fsSerializer serializer, string path) where T : class
        {
            var textAsset = Resources.Load<TextAsset>(path);
            Assert.IsNotNull((textAsset));
            var data = fsJsonParser.Parse(textAsset.text);
            object deserialized = null;
            serializer.TryDeserialize(data, typeof(T), ref deserialized).AssertSuccessWithoutWarnings();
            return deserialized as T;
        }

        /// <summary>
        /// Returns true if the specified path exists and false otherwise.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>True if the specified path exists; false otherwise.</returns>
        public static bool FileExists(string path)
        {
            var textAsset = Resources.Load<TextAsset>(path);
            return textAsset != null;
        }
    }
}
