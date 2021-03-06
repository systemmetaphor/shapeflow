﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ShapeFlow.Shapes
{
    public abstract class Shape
    {
        protected Shape(ShapeFormat format, string name) : this(format, name, Enumerable.Empty<string>())
        {
        }

        protected Shape(ShapeFormat format, string name, IEnumerable<string> tags)
        {
            Format = format;
            Name = name;
            Tags = tags;
        }

        public ShapeFormat Format { get; }

        public string Name { get; }

        public IEnumerable<string> Tags { get; }

        public abstract object GetInstance();

        public virtual void Add(Shape element)
        {
            throw new NotSupportedException();
        }
    }
}
