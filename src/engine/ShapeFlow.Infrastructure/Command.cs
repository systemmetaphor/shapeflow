using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

        public async Task<int> Execute(IEnumerable<string> arguments)
        {               
            return await OnExecute(GetOptions(arguments));                        
        }
                        
        protected abstract Task<int> OnExecute(CommandOptions options);
                
        protected virtual CommandOptions GetOptions(IEnumerable<string> arguments)
        {
            return new CommandOptions(arguments);
        }                
    }
}
