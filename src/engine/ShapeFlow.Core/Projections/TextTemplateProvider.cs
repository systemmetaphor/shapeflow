using System;
using System.IO;
using System.Linq;
using DotNetFileUtils;
using ShapeFlow.Declaration;

namespace ShapeFlow.Projections
{
    public class TextTemplateProvider
    {
        public TextTemplate GetFile(ProjectionContext context, ProjectionRuleDeclaration file)
        {
            /* file.IsEmbedded ? TextTemplate.Create(file, GetResourceStream(file.FileName)) : */
            return TextTemplate.Create(file, ResolveRulePath(context, file));
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
