﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace DotNetFileUtils.Globbing.Nodes
{
    [DebuggerDisplay("./")]
    internal sealed class RelativeRootNode : GlobNode
    {
        [DebuggerStepThrough]
        public override void Accept(GlobVisitor globber, GlobVisitorContext context)
        {
            globber.VisitRelativeRoot(this, context);
        }
    }
}