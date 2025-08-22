using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeFlow.Infrastructure
{
    public class CommandOptions
    {
        public CommandOptions(IEnumerable<string> arguments)
        {
            Arguments = arguments;
            Parse();
        }

        public IEnumerable<string> Arguments { get; }
         
        public virtual bool Valid
        {
            get
            {
                return true;
            }
        }
                
        public virtual void FormatInvalidMessage(StringBuilder target)
        {           
        }

        protected virtual void Parse()
        {

        }
    }
}
