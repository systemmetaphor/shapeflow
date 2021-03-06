﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace DotNetFileUtils
{
    /// <summary>
    /// Compares <see cref="PathBase"/> instances.
    /// </summary>
    public sealed class PathComparer : IEqualityComparer<PathBase>
    {
        /// <summary>
        /// The default path comparer.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly PathComparer Default;

        static PathComparer()
        {
            Default = new PathComparer(EnvironmentHelper.IsUnix());
        }

        /// <summary>
        /// Gets a value indicating whether comparison is case sensitive.
        /// </summary>
        /// <value>
        /// <c>true</c> if comparison is case sensitive; otherwise, <c>false</c>.
        /// </value>
        public bool IsCaseSensitive { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathComparer"/> class.
        /// </summary>
        /// <param name="isCaseSensitive">if set to <c>true</c>, comparison is case sensitive.</param>
        public PathComparer(bool isCaseSensitive)
        {
            IsCaseSensitive = isCaseSensitive;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathComparer"/> class.
        /// </summary>
        /// <param name="environment">The environment.</param>
        public PathComparer(IEnvironment environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }
            IsCaseSensitive = EnvironmentHelper.IsUnix();
        }

        /// <summary>
        /// Determines whether the specified <see cref="PathBase"/> instances are equal.
        /// </summary>
        /// <param name="x">The first <see cref="PathBase"/> to compare.</param>
        /// <param name="y">The second <see cref="PathBase"/> to compare.</param>
        /// <returns>
        /// True if the specified <see cref="PathBase"/> instances are equal; otherwise, false.
        /// </returns>
        public bool Equals(PathBase x, PathBase y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }

            if (IsCaseSensitive)
            {
                return x.FullPath.Equals(y.FullPath);
            }
            return x.FullPath.Equals(y.FullPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a hash code for the specified <see cref="PathBase"/>.
        /// </summary>
        /// <param name="obj">The path.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public int GetHashCode(PathBase obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            if (IsCaseSensitive)
            {
                return obj.FullPath.GetHashCode();
            }
            return obj.FullPath.ToUpperInvariant().GetHashCode();
        }
    }
}