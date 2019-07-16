using System.Collections.Generic;

namespace ShapeFlow.Declaration
{
    public class MetadataPart
    {
        public MetadataPart(string name, 
            IEnumerable<ProjectionDeclaration> generators, 
            IEnumerable<ShapeDeclaration> shapeDeclarations, 
            IEnumerable<PipelineDeclaration> pipelines)
        {
            Name = name;
            Projections = generators;
            ShapeDeclarations = shapeDeclarations;
            Pipelines = pipelines;
        }

        public string Name { get; }

        public IEnumerable<ProjectionDeclaration> Projections { get; }

        public IEnumerable<ShapeDeclaration> ShapeDeclarations { get; }

        public IEnumerable<PipelineDeclaration> Pipelines { get; }
    }
}