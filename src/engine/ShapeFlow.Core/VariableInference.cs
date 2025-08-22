using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetFileUtils;
using ShapeFlow.ModelToCode;

namespace ShapeFlow
{
    public class VariableInference
    {
        public static IDictionary<string, string> Run(string projectFile)
        {
            // use system environment and the given file path to create
            // the core variables
            var result = new Dictionary<string, string>
            {
                {"project-root", Path.GetDirectoryName(projectFile)},
                {"project", projectFile},
                {"current-directory", Environment.CurrentDirectory},
                {"command-line", Environment.CommandLine},
                {"machine-name", Environment.MachineName}
            };


            var searchRoot = new DirectoryPath(result["project-root"]);
            var searchExpression = "**/*.sln";
            var fullSearch = searchRoot.Combine(searchExpression);

            var globber = new Globber();
            var solutionFiles = globber.Match(fullSearch.FullPath).OrderBy(p => p.FullPath).ToArray();

            if (solutionFiles.Any())
            {
                var numberOfSegments = int.MaxValue;
                FilePath solutionPath = null;

                // search for the less nested file

                foreach (var solutionFile in solutionFiles)
                {
                    // in case there is a folder ending in .sln

                    if (!(solutionFile is FilePath currentFilePath))
                    {
                        continue;
                    }

                    // we use the old school find the least value because we need to iterate to all the
                    // results in this loop anyway (so a Linq min would imply iterating twice)

                    if (currentFilePath.Segments.Length < numberOfSegments)
                    {
                        solutionPath = currentFilePath;
                        numberOfSegments = solutionFile.Segments.Length;
                    }

                    // foreach solution file we are generating an automatic variable
                    // that can be used inside templates and anywhere that is path

                    var baseName = currentFilePath.GetFilenameWithoutExtension().FullPath;
                    baseName = baseName.ToSafeName("-");
                    result.Add($"{baseName}-solution-file", currentFilePath.FullPath);
                    result.Add($"{baseName}-solution-dir", currentFilePath.GetDirectory().FullPath);
                }

                // if there are multiple the first in alphabetic order will win

                if (solutionPath != null)
                {
                    result.Add("solution-file", solutionPath.FullPath);
                    result.Add("solution-dir", solutionPath.GetDirectory().FullPath);
                }
            }

            return result;
        }
    }
}