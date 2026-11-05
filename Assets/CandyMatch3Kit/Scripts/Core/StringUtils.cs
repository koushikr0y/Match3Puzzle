// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using System.Linq;

namespace GameVanilla.Core
{
    /// <summary>
    /// A collection of miscellaneous string-related utilities.
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Returns the specified string as a readable string.
        /// </summary>
        /// <param name="camelCase">The original, camel case string.</param>
        /// <returns>The readable version of the original string.</returns>
        public static string DisplayCamelCaseString(string camelCase)
        {
            var chars = new List<char> { camelCase[0] };
            foreach (var c in camelCase.Skip(1))
            {
                if (char.IsUpper(c))
                {
                    chars.Add(' ');
                    chars.Add(char.ToLower(c));
                }
                else
                {
                    chars.Add(c);
                }
            }

            return new string(chars.ToArray());
        }
    }
}
