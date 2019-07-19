using ShapeFlow.Projections;

namespace ShapeFlow.Output
{
    public interface IFileService
    {
        string GetWritePath(string outputPath, string outputRoot = null);

        void PerformWrite(string outputPath, string outputText);


        void Process(ProjectionContext projection);
    }
}
