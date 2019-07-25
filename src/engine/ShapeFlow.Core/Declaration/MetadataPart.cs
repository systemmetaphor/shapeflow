using System.Collections.Generic;

namespace ShapeFlow.Declaration
{
    public class MetadataPart
    {
        public MetadataPart(string name, 
            IEnumerable<ProjectionDeclaration> generators, 
            IEnumerable<ShapeDeclaration> shapes, 
            IEnumerable<PipelineStageDeclaration> pipelines)
        {
            Name = name;
            Projections = generators;
            Shapes = shapes;
            Pipelines = pipelines;
        }

        public string Name { get; }

        public IEnumerable<ProjectionDeclaration> Projections { get; }

        public IEnumerable<ShapeDeclaration> Shapes { get; }

        public IEnumerable<PipelineStageDeclaration> Pipelines { get; }
    }
}