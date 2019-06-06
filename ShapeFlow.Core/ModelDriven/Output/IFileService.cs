﻿namespace ShapeFlow
{
    public interface IFileService
    {
        string GetWritePath(string outputPath, string outputRoot = null);

        void PerformWrite(string outputPath, string outputText);

        SolutionEventContext Process(SolutionEventContext context);

        void Process(ProjectionContext projection, ModelToTextOutput outputFiles);
    }
}
