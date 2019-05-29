using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeFlow.ModelDriven.Models
{
    public abstract class Model
    {
        protected Model(ModelFormat format, string name, IEnumerable<string> tags)
        {
            Format = format;
            Name = name;
            Tags = tags;
        }

        public ModelFormat Format { get; }

        public string Name { get; }

        public IEnumerable<string> Tags { get; }

        public abstract object GetModelInstance();
    }
}
