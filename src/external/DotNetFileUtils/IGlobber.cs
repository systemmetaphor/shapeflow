﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace DotNetFileUtils
{
    /// <summary>
    /// Represents a file system globber.
    /// </summary>
    public interface IGlobber
    {
        /// <summary>
        /// Returns <see cref="PathBase" /> instances matching the specified pattern.
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="predicate">The predicate used to filter directories based on file system information.</param>
        /// <returns>
        ///   <see cref="PathBase" /> instances matching the specified pattern.
        /// </returns>
        [Obsolete("Please use the Match overload that accept globber settings instead.", false)]
        IEnumerable<PathBase> Match(string pattern, Func<IDirectory, bool> predicate);

        /// <summary>
        /// Returns <see cref="PathBase" /> instances matching the specified pattern.
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="settings">The globber settings.</param>
        /// <returns>
        ///   <see cref="PathBase" /> instances matching the specified pattern.
        /// </returns>
        IEnumerable<PathBase> Match(string pattern, GlobberSettings settings);
    }
}