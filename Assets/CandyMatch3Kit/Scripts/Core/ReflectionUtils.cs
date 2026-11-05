// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Linq;

namespace GameVanilla.Core
{
    /// <summary>
    /// Miscellaneous reflection utilities.
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// Returns all the types derived from the specified type.
        /// </summary>
        /// <param name="aAppDomain">The app domain.</param>
        /// <param name="aType">The base type.</param>
        /// <returns>All the types derived from the specified type.</returns>
        public static System.Type[] GetAllDerivedTypes(this System.AppDomain aAppDomain, System.Type aType)
        {
            var assemblies = aAppDomain.GetAssemblies();
            return (from assembly in assemblies
                from type in assembly.GetTypes() where type.IsSubclassOf(aType)
                select type).ToArray();
        }
    }
}
