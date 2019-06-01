using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeFlow.Infrastructure
{   
    [Serializable]
    public class CommandException : Exception
    { 
        public CommandException() { }

        public CommandException(string message) : base(message) { }

        public CommandException(string message, Exception inner) : base(message, inner) { }

        protected CommandException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
