using System;
using System.IO;
using System.Linq;
using DotNetFileUtils;
using ShapeFlow.Declaration;

namespace ShapeFlow.Projections
{
    public class ProjectionRuleProvider
    {
        public ProjectionRule GetFile(ProjectionContext context, ProjectionRuleDeclaration file)
        {
            /* file.IsEmbedded ? ProjectionRule.Create(file, GetResourceStream(file.FileName)) : */
            return ProjectionRule.Create(file, ResolveRulePath(context, file));
        }

        protected string ResolveRulePath(ProjectionContext context, ProjectionRuleDeclaration rule)
        {
            var projectionDeploymentDirectory = new DirectoryPath(context.PipelineDeclaration.Projection.Location);
            projectionDeploymentDirectory =
                projectionDeploymentDirectory.Combine(context.PipelineDeclaration.Projection.RulesBasePath);
            var filePath = projectionDeploymentDirectory.CombineWithFilePath(rule.FileName);
            return filePath.FullPath;
        }        
    }
}
