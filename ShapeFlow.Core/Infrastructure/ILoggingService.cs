using System;

namespace ShapeFlow.Infrastructure
{
    
    public interface ILoggingService
    {
        string LogFileName { get; set; }

        string LogDirectory { get; set; }

        void EnableConsoleOutput();

        void Info(string message, params object[] formatArguments);

        void Info(string message, Exception ex);

        void Debug(string message, params object[] formatArguments);

        void Debug(string message, Exception ex);

        void Error(string message, params object[] formatArguments);

        void Error(string message, Exception ex);
               
        void Fatal(string message, params object[] formatArguments);

        void Fatal(string message, Exception ex);

        void Trace(string message, params object[] formatArguments);

        void Trace(string message, Exception ex);

        void Warning(string message, params object[] formatArguments);

        void Warning(string message, Exception ex);
    }
}
