using System.Collections.Generic;
using ShapeFlow.ModelToCode;
using ShapeFlow.Projections;
using ShapeFlow.Shapes;

namespace ShapeFlow.Declaration
{
    public static class InitTemplate
    {
        public static string Generate(InitTemplateOptions options)
        { 
            var projectionRule = ProjectionRuleDeclaration.Create("Data\\Records.liquid", RuleLanguages.DotLiquid);
            
            var projection = ProjectionDeclaration.Create(
                "tablesToRecords",
                options.RootDirectory,
                "Templates",
                new []{ projectionRule },
                InputDeclaration.Create(nameof(ShapeFormat.Clr), string.Empty),
                OutputDeclaration.Create(nameof(ShapeFormat.FileSet), "FileSetLoader", null));

            var shape = ShapeDeclaration.Create(
                $"databaseModel",
                "DbModelLoader",
                new[] { "dbEntityModel" },
                new Dictionary<string, string>()
                {
                      { "server", "" }
                    , { "db", "" }
                    , { "user", "" }
                    , { "password", "" }
                });

            var stage = PipelineStageDeclaration.Create("entities", "tablesToRecords", "databaseModel");

            var pipeline = PipelineDeclaration.Create($"applicationTier", new[] { stage });

            var solution = SolutionDeclaration.Create(
                $"Generate { options.ProjectName } files",
                options.RootDirectory,
                new[] { projection },
                new[] { shape },
                new[] { pipeline });

            var test = SolutionDeclaration.Save(solution);
            return test;
        }
    }

    public class InitTemplateOptions
    {
        public string ProjectName { get; set; }

        public string RootDirectory { get; set; }
    }
}
