﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using DotNetFileUtils.Globbing.Nodes;

namespace DotNetFileUtils.Globbing
{
    internal static class GlobNodeValidator
    {
        public static void Validate(string pattern, GlobNode node)
        {
            var previous = (GlobNode)null;
            var current = node;
            while (current != null)
            {
                if (previous is RecursiveWildcardNode)
                {
                    if (current is ParentDirectoryNode)
                    {
                        throw new NotSupportedException("Visiting a parent that is a recursive wildcard is not supported.");
                    }
                }

                if (current is UncRootNode unc)
                {
                    if (string.IsNullOrWhiteSpace(unc.Server))
                    {
                        throw new DotNetFileUtilsException($"The pattern '{pattern}' has no server part specified.");
                    }
                }

                previous = current;
                current = current.Next;
            }
        }
    }
}