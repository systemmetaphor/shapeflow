using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace ShapeFlow.Infrastructure
{
    /// <summary>
    /// Default implementation of <see cref="ILoggingService"/>.
    /// </summary>
    internal class LoggingService : ILoggingService
    {
        private readonly ApplicationContext _applicationContext;
        private LoggingConfiguration _loggingConfiguration;
        private string _logFileName;
        private string _logDirectory;
        private Logger _logger;
        private bool _enableLogToConsole;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingService"/> class.
        /// </summary>
        public LoggingService(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext ?? throw new ArgumentNullException(nameof(applicationContext));
        }

        /// <summary>
        /// Gets or sets the name of the log file.
        /// </summary>
        /// <value>
        /// The name of the log file.
        /// </value>
        public string LogFileName
        {
            get
            {
                return _logFileName ?? (_logFileName = _applicationContext.GetArgumentValue(ArgumentNames.LogFile));
            }

            set
            {
                _logFileName = value;
            }
        }

        /// <summary>
        /// Gets or sets the log directory.
        /// </summary>
        /// <value>
        /// The log directory.
        /// </value>
        public string LogDirectory 
        { 
            get
            {
                return _logDirectory ?? (_logDirectory = _applicationContext.GetArgumentValue(ArgumentNames.LogDirectory));
            }

            set
            {
                _logDirectory = value;
            }
        }
                
        private Logger Logger
        {
            get
            {
                return _logger ?? (_logger = InitializeLogger());
            }
        }

        /// <summary>
        /// Writes the specified message to the log with the Info category.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="formatArguments">The format arguments.</param>
        public void Info(string message, params object[] formatArguments)
        {
            Logger.Info(message, formatArguments);
        }

        /// <summary>
        /// Writes the specified message and exception to the log with the Info category.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        public void Info(string message, Exception ex)
        {
            Logger.Info(ex, message);
        }

        /// <summary>
        /// Writes the specified message to the log with the Trace category.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="formatArguments">The format arguments.</param>
        public void Trace(string message, params object[] formatArguments)
        {
            Logger.Trace(message, formatArguments);
        }

        /// <summary>
        /// Writes the specified message and exception to the log with the Trace category.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        public void Trace(string message, Exception ex)
        {
            Logger.Trace(ex, message);
        }

        /// <summary>
        /// Writes the specified message to the log with the Debug category.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="formatArguments">The format arguments.</param>
        public void Debug(string message, params object[] formatArguments)
        {
            Logger.Debug(message, formatArguments);
        }

        /// <summary>
        /// Writes the specified message and exception to the log with the Debug category.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        public void Debug(string message, Exception ex)
        {
            Logger.Debug(ex, message);
        }

        /// <summary>
        /// Writes the specified message to the log with the Warning category.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="formatArguments">The format arguments.</param>
        public void Warning(string message, params object[] formatArguments)
        {
            Logger.Warn(message, formatArguments);
        }

        /// <summary>
        /// Writes the specified message and exception to the log with the Warning category.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        public void Warning(string message, Exception ex)
        {
            Logger.Warn(ex, message);
        }

        /// <summary>
        /// Writes the specified message to the log with the Info category.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="formatArguments">The format arguments.</param>
        public void Error(string message, params object[] formatArguments)
        {
            Logger.Error(message, formatArguments);
        }

        /// <summary>
        /// Errors the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The ex.</param>
        public void Error(string message, Exception ex)
        {
            Logger.Error(ex, message);
        }

        /// <summary>
        /// Writes the specified message to the log with the Fatal category.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="formatArguments">The format arguments.</param>
        public void Fatal(string message, params object[] formatArguments)
        {
            Logger.Fatal(message, formatArguments);
        }

        /// <summary>
        /// Writes the specified message and exception to the log with the Fatal category.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        public void Fatal(string message, Exception ex)
        {
            Logger.Fatal(ex, message);
        }

        public void EnableConsoleOutput()
        {
            _enableLogToConsole = true;
            EnsureLogger();           
        }
        
        private void EnsureLogger()
        {
            var tmpLogger = Logger;
            if(tmpLogger == null)
            {
                throw new InvalidOperationException(InfrastructureErrorMessages.LogInitializationFailed);
            }
        }

        private Logger InitializeLogger()
        {
            _loggingConfiguration = new LoggingConfiguration();

            var fileTarget = new FileTarget();

            var logFilePath = Path.Combine(LogDirectory, LogFileName);

            if(Path.IsPathRooted(logFilePath))
            {
                fileTarget.FileName = logFilePath;
            }
            else
            {
                if(logFilePath.StartsWith("/"))
                {
                    logFilePath = logFilePath.Substring(1);
                }

                fileTarget.FileName = string.Concat("${basedir}/", logFilePath);
            }
                        
            fileTarget.Layout = "${longdate} | ${level} | ${message} | ${onexception:inner=${newline}${exception:format=tostring}}";
            _loggingConfiguration.AddTarget("file", fileTarget);

            var includeAllInFileTarget = new LoggingRule("*", LogLevel.Trace, fileTarget);
            _loggingConfiguration.LoggingRules.Add(includeAllInFileTarget);

            if (_enableLogToConsole)
            {
                var consoleTarget = new ColoredConsoleTarget() { Name = "Console" };
                consoleTarget.Layout = "${level} | ${message} ${onexception:inner=${newline}${exception:format=tostring}}";
                _loggingConfiguration.AddTarget(consoleTarget);
                _loggingConfiguration.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget.Name);
            }

            LogManager.Configuration = _loggingConfiguration;
            var logger = LogManager.GetLogger("Default");

            return logger;
        }
    }
}
