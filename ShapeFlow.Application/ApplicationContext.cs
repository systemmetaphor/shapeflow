using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ShapeFlow.Infrastructure
{
    public class ApplicationContext
    {       
        private string _applicationDirectory;
        private IDictionary<string, string> _applicationArguments;
        private IDictionary<string, string> _defaults;

        public ApplicationContext()
        {            
            _applicationArguments = new Dictionary<string, string>();
            _defaults = new Dictionary<string, string>
            {
                { ArgumentNames.OutputDirectory, "out" },
                { ArgumentNames.LogDirectory, "Log" },
                { ArgumentNames.LogFile, "log.txt" }
            };
        }

        public string ApplicationDirectory
        {
            get
            {
                return _applicationDirectory ?? (_applicationDirectory = Path.GetDirectoryName(typeof(ApplicationContext).Assembly.Location));
            }
        }

        public string OutputDirectory
        {
            get
            {
                return GetArgumentValue(ArgumentNames.OutputDirectory);
            }
        }
        
        public string GetArgumentValue(string argumentName)
        {
            _applicationArguments.TryGetValue(argumentName, out string argumentValue);
            return string.IsNullOrWhiteSpace(argumentValue) ? GetDefaultValue(argumentName) : argumentValue;
        }

        private string GetDefaultValue(string argumentName)
        {
            _defaults.TryGetValue(argumentName, out string argumentValue);
            return argumentValue ?? string.Empty;
        }
    }
}
