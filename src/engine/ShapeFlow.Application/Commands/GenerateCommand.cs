using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ShapeFlow.Infrastructure;
using ShapeFlow.Pipelines;

namespace ShapeFlow.Commands
{
    public class GenerateCommand : Command
    {
        private readonly IContainer _container;

        public GenerateCommand(IContainer container)
        {
            _container = container;
        }

        public override string Name => "generate";

        protected override async Task<int> OnExecute(CommandOptions options)
        {
            var generateOptions = (GenerateOptions)options;
            
            var projectFile = Path.Combine(Environment.CurrentDirectory, "shapeflow.config.json");
            if (File.Exists(projectFile) && string.IsNullOrWhiteSpace(generateOptions.ProjectFile))
            {
                generateOptions.ProjectFile = projectFile;
            }

            try
            {
                if(string.IsNullOrWhiteSpace(generateOptions.ProjectFile) || !File.Exists(generateOptions.ProjectFile))
                {
                    AppTrace.Error("It was not possible to find the shapeflow project file.");
                    return 0;
                }

                var parameters = new Dictionary<string, string>(generateOptions.Parameters)
                {
                    { "project-root", Environment.CurrentDirectory },
                    { "project", generateOptions.ProjectFile }
                };
                
                var engine = _container.Resolve<ShapeFlowEngine>();
                await engine.Run(parameters);
            }
            catch (ApplicationOptionException ex)
            {
                AppTrace.Error("Invalid command line arguments. See exception for details.", ex);                
            }

            return 0;
        }

        protected override CommandOptions GetOptions(IEnumerable<string> arguments)
        {
            return new GenerateOptions(arguments);
        }

        class GenerateOptions : CommandOptions
        {
            private readonly Dictionary<string, string> _parameters;

            public GenerateOptions(IEnumerable<string> args) : base(args)
            {
                _parameters = new Dictionary<string, string>();
            }

            public string ProjectFile { get; set; }

            public IDictionary<string, string> Parameters => _parameters;

            protected override void Parse()
            {
                var optionsParser = InitializeOptionsParser();
                optionsParser.Parse(Arguments);
            }

            Mono.Options.OptionSet InitializeOptionsParser()
            {
                var result = new Mono.Options.OptionSet
                {
                    { "parameter:", "Add parameter", value => {
                        value = value.Trim('\"');
                        var parts = value.Split('=');
                        if(parts.Length == 2)
                        {
                            _parameters.Add(parts[0], parts[1]);
                        }
                    }},
                    { "project:", "Set the project", value => {
                        ProjectFile = value;
                    }}
                };

                return result;
            }
        }
    }
}
