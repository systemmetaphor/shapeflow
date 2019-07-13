using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ShapeFlow
{
    // adapted from: https://github.com/Wyamio/Wyam/blob/develop/src/clients/Wyam/Tracing/SimpleColorConsoleTraceListener.cs
    internal class ColorConsoleTraceListener : TextWriterTraceListener
    {
        public ColorConsoleTraceListener()
            : base(Console.Out)
        {
        }

        private readonly Dictionary<TraceEventType, Tuple<ConsoleColor, ConsoleColor?>> _eventColors
            = new Dictionary<TraceEventType, Tuple<ConsoleColor, ConsoleColor?>>
            {
                { TraceEventType.Verbose, Tuple.Create(ConsoleColor.Gray, (ConsoleColor?)null) },
                { TraceEventType.Information, Tuple.Create(ConsoleColor.White, (ConsoleColor?)null) },
                { TraceEventType.Warning, Tuple.Create(ConsoleColor.Yellow, (ConsoleColor?)null) },
                { TraceEventType.Error, Tuple.Create(ConsoleColor.Red, (ConsoleColor?)null) },
                { TraceEventType.Critical, Tuple.Create(ConsoleColor.White, (ConsoleColor?)ConsoleColor.Red) },
                { TraceEventType.Start, Tuple.Create(ConsoleColor.Green, (ConsoleColor?)null) },
                { TraceEventType.Stop, Tuple.Create(ConsoleColor.DarkGreen, (ConsoleColor?)null) }
            };

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            TraceEvent(eventCache, source, eventType, id, "{0}", message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            Tuple<ConsoleColor, ConsoleColor?> colors;
            if (!_eventColors.TryGetValue(eventType, out colors))
            {
                WriteLine(string.Format(format, args));
                return;
            }

            ConsoleColor originalForegroundColor = Console.ForegroundColor;
            ConsoleColor originalBackgroundColor = Console.BackgroundColor;
            Console.ForegroundColor = colors.Item1;
            if (colors.Item2.HasValue)
            {
                Console.BackgroundColor = colors.Item2.Value;
            }

            WriteLine(string.Format(format, args));

            Console.ForegroundColor = originalForegroundColor;
            if (colors.Item2.HasValue)
            {
                Console.BackgroundColor = originalBackgroundColor;
            }
        }
    }
}
