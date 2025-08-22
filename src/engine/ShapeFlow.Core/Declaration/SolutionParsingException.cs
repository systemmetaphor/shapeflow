using System;
using System.Collections.Generic;
using System.Text;

namespace ShapeFlow.Declaration
{

    [Serializable]
    public class SolutionParsingException : Exception
    {
        public SolutionParsingException() { }
        public SolutionParsingException(string message) : base(message) { }
        public SolutionParsingException(string message, Exception inner) : base(message, inner) { }
        protected SolutionParsingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
