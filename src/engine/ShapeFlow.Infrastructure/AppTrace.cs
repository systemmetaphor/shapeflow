using System.Diagnostics;

namespace ShapeFlow.Infrastructure
{    
    public class AppTrace
    {
        private static readonly TraceSource _source = new TraceSource("shapeflow", SourceLevels.Information);
        private static readonly object _synchRoot = new object();
        
        private AppTrace()
        {
        }

        public static SourceLevels Level
        {
            get { return _source.Switch.Level; }
            set { _source.Switch.Level = value; }
        }

        public static void AddListener(TraceListener listener)
        {
            lock (_synchRoot)
            {
                _source.Listeners.Add(listener);                
            }
        }

        public static void RemoveListener(TraceListener listener)
        {
            lock (_synchRoot)
            {
                listener.IndentLevel = 0;
                _source.Listeners.Remove(listener);
            }
        }
                
        public static void Critical(string messageOrFormat, params object[] args) =>
            TraceEvent(TraceEventType.Critical, messageOrFormat, args);
                
        public static void Error(string messageOrFormat, params object[] args) =>
            TraceEvent(TraceEventType.Error, messageOrFormat, args);
                
        public static void Warning(string messageOrFormat, params object[] args) =>
            TraceEvent(TraceEventType.Warning, messageOrFormat, args);

        public static void Information(string messageOrFormat, params object[] args) =>
            TraceEvent(TraceEventType.Information, messageOrFormat, args);

        public static void Verbose(string messageOrFormat, params object[] args) =>
            TraceEvent(TraceEventType.Verbose, messageOrFormat, args);

        public static void TraceEvent(TraceEventType eventType, string messageOrFormat, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                _source.TraceEvent(eventType, 0, messageOrFormat);
            }
            else
            {
                _source.TraceEvent(eventType, 0, messageOrFormat, args);
            }
        }                
    }
}
