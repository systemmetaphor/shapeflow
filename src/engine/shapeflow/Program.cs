using System;
using System.Threading.Tasks;
using ShapeFlow.Infrastructure;
using ShapeFlow;

namespace shapeflow
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            AppTrace.AddListener(new ColorConsoleTraceListener { TraceOutputOptions = System.Diagnostics.TraceOptions.None });
            await Application.Run(args);
        }
    }
}
