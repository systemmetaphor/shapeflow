﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetFileUtils.Globbing;

namespace DotNetFileUtils
{
    /// <summary>
    /// The file system globber.
    /// </summary>
    public sealed class Globber : IGlobber
    {
        private readonly GlobParser _parser;
        private readonly GlobVisitor _visitor;
        private readonly PathComparer _comparer;
        private readonly IEnvironment _environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="Globber"/> class.
        /// </summary>
        public Globber() : this(new FileSystem(), new DotNetCoreEnvironment())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Globber"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="environment">The environment.</param>
        public Globber(IFileSystem fileSystem, IEnvironment environment)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            _environment = environment;
            _parser = new GlobParser(environment);
            _visitor = new GlobVisitor(fileSystem, environment);
            _comparer = new PathComparer(EnvironmentHelper.IsUnix());
        }

        /// <summary>
        /// Returns <see cref="Path" /> instances matching the specified pattern.
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="predicate">The predicate used to filter directories based on file system information.</param>
        /// <returns>
        ///   <see cref="Path" /> instances matching the specified pattern.
        /// </returns>
        [Obsolete("Please use the Match overload that accept globber settings instead.", false)]
        public IEnumerable<Path> Match(string pattern, Func<IDirectory, bool> predicate)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }
            if (string.IsNullOrWhiteSpace(pattern))
            {
                return Enumerable.Empty<Path>();
            }

            return Match(pattern, new GlobberSettings
            {
                Predicate = predicate
            });
        }

        /// <summary>
        /// Returns <see cref="Path" /> instances matching the specified pattern.
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="settings">The globber settings.</param>
        /// <returns>
        ///   <see cref="Path" /> instances matching the specified pattern.
        /// </returns>
        public IEnumerable<Path> Match(string pattern, GlobberSettings settings)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }
            if (string.IsNullOrWhiteSpace(pattern))
            {
                return Enumerable.Empty<Path>();
            }

            // Parse the pattern into an AST.
            var root = _parser.Parse(pattern, settings);

            // Visit all nodes in the parsed patterns and filter the result.
            return _visitor.Walk(root, settings)
                .Select(x => x.Path)
                .Distinct(_comparer);
        }
    }
}