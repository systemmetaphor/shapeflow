﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace DotNetFileUtils.Globbing.Nodes.Segments
{
    internal abstract class PathSegment
    {
        public abstract string Regex { get; }
        public abstract string Value { get; }
    }
}
