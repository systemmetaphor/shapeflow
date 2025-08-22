﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace DotNetFileUtils.Globbing.Nodes.Segments
{
    internal sealed class WildcardSegment : PathSegment
    {
        public override string Value => "*";

        public override string Regex => ".*";
    }
}
