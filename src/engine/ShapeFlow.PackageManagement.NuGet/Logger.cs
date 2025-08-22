using System.Threading.Tasks;
using NuGet.Common;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.PackageManagement.NuGet
{
    public class Logger : ILogger
    {
        // TODO: tune the mapping between log levels

        public void LogDebug(string data) => AppTrace.Verbose(data);
        public void LogVerbose(string data) => AppTrace.Verbose(data);
        public void LogInformation(string data) => AppTrace.Verbose(data);
        public void LogMinimal(string data) => AppTrace.Verbose(data);
        public void LogWarning(string data) => AppTrace.Verbose(data);
        public void LogError(string data) => AppTrace.Verbose(data);

        public void LogInformationSummary(string data)
        {
            AppTrace.Information(data);
        }

        public void Log(LogLevel level, string data)
        {
            AppTrace.Information(data);
        }

        public Task LogAsync(LogLevel level, string data)
        {
            AppTrace.Information(data);
            return Task.FromResult(0);
        }

        public void Log(ILogMessage message)
        {
            AppTrace.Information(message.Message);
        }

        public Task LogAsync(ILogMessage message)
        {
            AppTrace.Information(message.Message);
            return Task.FromResult(0);
        }

        public void LogSummary(string data) => AppTrace.Verbose(data);
    }
}