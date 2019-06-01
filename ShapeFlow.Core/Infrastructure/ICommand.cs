using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeFlow.Infrastructure
{    
    public interface ICommand
    {
        string HelpSummary { get; }

        string Help { get; }

        string Name { get; }

        int Execute(IEnumerable<string> arguments);           
    }
}
