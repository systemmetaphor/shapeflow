using System.IO;
using System.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow.Projections;
using ShapeFlow.Shapes;

namespace ShapeFlow.Output
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
        
        public void Process(ProjectionContext projection)
        {
            if (projection.Output.Shape.Format != ShapeFormat.FileSet)
            {
                return;
            }

            var output = projection.Output.Shape.GetInstance() as FileSet;
            if (output?.OutputFiles == null || !output.OutputFiles.Any())
            {
                return;
            }

            foreach (var outputFile in output.OutputFiles)
            {
                var fullPath = outputFile.Path;

                if (!Path.IsPathRooted(fullPath))
                {
                    var root = projection.PipelineDeclaration.GetParameter("project-root");
                    if (!string.IsNullOrEmpty(root))
                    {
                        fullPath = Path.Combine(root, outputFile.Path);
                    }
                }

                PerformWrite(fullPath, outputFile.Text);
            }
        }
    }
}
