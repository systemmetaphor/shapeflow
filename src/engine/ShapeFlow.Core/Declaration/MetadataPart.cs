using System.Collections.Generic;

namespace ShapeFlow.Declaration
{
    public class MetadataPart
    {
        public MetadataPart(string name, 
            IEnumerable<ProjectionDeclaration> generators, 
            IEnumerable<ShapeDeclaration> shapes, 
            IEnumerable<PipelineDeclaration> pipelines)
        {
            Name = name;
            Projections = generators;
            Shapes = shapes;
            Pipelines = pipelines;
        }

        public string Name { get; }

        public IEnumerable<ProjectionDeclaration> Projections { get; }

        public IEnumerable<ShapeDeclaration> Shapes { get; }

        public IEnumerable<PipelineDeclaration> Pipelines { get; }
    }
}