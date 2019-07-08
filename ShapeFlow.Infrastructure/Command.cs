using System;
using System.Collections.Generic;
using System.IO;
using Mono.Options;

namespace ShapeFlow.Infrastructure
{    
    public abstract class Command : ICommand
    {   
                
        public abstract string Name
        {
            get;
        }

        public virtual string HelpSummary
        {
            get
            {
                return string.Empty;    
            }
        }

        public virtual string Help
        {
            get
            {
                return string.Empty;
            }
        }

        public int Execute(IEnumerable<string> arguments)
        {               
            return OnExecute(GetOptions(arguments));                        
        }
                        
        protected abstract int OnExecute(CommandOptions options);
                
        protected virtual CommandOptions GetOptions(IEnumerable<string> arguments)
        {
            return new CommandOptions(arguments);
        }                
    }
}
