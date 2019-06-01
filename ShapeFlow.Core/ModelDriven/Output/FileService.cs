using System;
using System.IO;
using ShapeFlow.Infrastructure;

namespace ShapeFlow
{
    internal class FileService : IFileService
    { 
        public FileService()
        {            
        }

        public string GetWritePath(string outputPath, string outputRoot=null)
        {
            if (Path.IsPathRooted(outputPath))
            {
                return outputPath;
            }

            if(string.IsNullOrEmpty(outputPath))
            {
                return outputPath;
            }

            return Path.Combine(outputRoot, outputPath);
        }

        public void PerformWrite(string outputFileName, string outputText)
        {
            outputFileName = GetWritePath(outputFileName);

            var outputDirectory = Path.GetDirectoryName(outputFileName);

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            AppTrace.Information("Writing file '{0}'", outputFileName);

            File.WriteAllText(outputFileName, outputText);
        }

        public SolutionEventContext Process(SolutionEventContext context)
        {
            foreach (var projection in context.Projections)
            {
                var outputFiles = projection.Output;

                foreach (var outputFile in projection.Output.OutputFiles)
                {
                    var fullPath = outputFile.OutputPath;

                    if (!Path.IsPathRooted(fullPath))
                    {
                        var root = projection.Solution.GetParameter("project-root");
                        if (!string.IsNullOrEmpty(root))
                        {
                            fullPath = Path.Combine(root, outputFile.OutputPath);
                        }
                    }

                    PerformWrite(fullPath, outputFile.Text);
                }
            }

            return context;
        }
    }
}
