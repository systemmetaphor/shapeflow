using System.Collections.Generic;

namespace ShapeFlow.Declaration
{
    public abstract class MetadataPart
    {
        public string Name { get; protected set; }

        public IEnumerable<ProjectionDeclaration> Projections { get; protected set; }

        public IEnumerable<ShapeDeclaration> Shapes { get; protected set; }

        public IEnumerable<PipelineDeclaration> Pipelines { get; protected set; }
    }
}