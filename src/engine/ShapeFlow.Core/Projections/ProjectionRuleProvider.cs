using System;
using System.IO;
using System.Linq;
using DotNetFileUtils;
using ShapeFlow.Declaration;

namespace ShapeFlow.Projections
{
    public class ProjectionRuleProvider
    {
        private readonly ProjectionRegistry _projectionRegistry;

        public ProjectionRuleProvider(ProjectionRegistry projectionRegistry)
        {
            _projectionRegistry = projectionRegistry;
        }

        public ProjectionRule GetFile(ProjectionContext context, ProjectionRuleDeclaration file)
        {
            return ProjectionRule.Create(file, ResolveRulePath(context, file));
        }

        protected string ResolveRulePath(ProjectionContext context, ProjectionRuleDeclaration rule)
        {
            var projectionRef = context.PipelineStageDeclaration.ProjectionRef;

            if (!_projectionRegistry.TryGet(projectionRef, out var projectionDecl))
            {
                throw new InvalidOperationException($"It was not possible to find the declaration for the projection {projectionRef}.");
            }

            var projectionDeploymentDirectory = new DirectoryPath(projectionDecl.Location);
            projectionDeploymentDirectory = projectionDeploymentDirectory.Combine(projectionDecl.RulesBasePath);
            var filePath = projectionDeploymentDirectory.CombineWithFilePath(rule.FileName);
            return filePath.FullPath;
        }
    }
}
