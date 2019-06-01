using System;
using ShapeFlow.Infrastructure;
using ShapeFlow;

namespace shapeflow
{
    class Program
    {
        static void Main(string[] args)
        {
            AppTrace.AddListener(new ColorConsoleTraceListener { TraceOutputOptions = System.Diagnostics.TraceOptions.None });
            Application.Run(args);
        }
    }
}
