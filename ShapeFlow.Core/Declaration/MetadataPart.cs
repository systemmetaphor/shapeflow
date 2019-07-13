using System.Collections.Generic;

namespace ShapeFlow.Declaration
{
    public class MetadataPart
    {
        public MetadataPart(string name, 
            IEnumerable<ProjectionDeclaration> generators, 
            IEnumerable<ShapeDeclaration> models, 
            IEnumerable<PipelineDeclaration> pipelines)
        {
            Name = name;
            Projections = generators;
            Models = models;
            Pipelines = pipelines;
        }

        public string Name { get; }

        public IEnumerable<ProjectionDeclaration> Projections { get; }

        public IEnumerable<ShapeDeclaration> Models { get; }

        public IEnumerable<PipelineDeclaration> Pipelines { get; }
    }
}