using System;
using System.IO;
using ShapeFlow.Infrastructure;
using Mono.Options;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using ShapeFlow.ModelDriven.Models;
using ShapeFlow.ModelDriven.Loaders;
using ShapeFlow.Loaders.DbModel;
using ShapeFlow.ModelDriven.TemplateEngines;

namespace ShapeFlow.ModelDriven
{
    public static class Application
    {
        private static ApplicationOptions _options;

        static Application()
        {
            _options = new ApplicationOptions();
        }

        public static void Run(string[] args)
        {
            using (var currentContainer = ApplicationContainerFactory.Create(RegisterComponents))
            {
                // enable console output in logging
                currentContainer.Resolve<ILoggingService>().EnableConsoleOutput();

                var optionsParser = InitializeOptionsParser();
                optionsParser.Parse(args);

                var projectFile = Path.Combine(Environment.CurrentDirectory, "shapeflow.config.json");
                if (File.Exists(projectFile) && string.IsNullOrWhiteSpace(_options.ProjectFile))
                {
                    _options.ProjectFile = projectFile;
                }

                try
                {
                    var parameters = new Dictionary<string, string>
                    {
                        { "project-root", Environment.CurrentDirectory }
                    };

                    var solution = Solution.ParseFile(_options.ProjectFile);
                    solution.AddParameters(parameters);

                    var ev = new SolutionEventContext(solution);
                    var engine = currentContainer.Resolve<SolutionEngine>();

                    engine.Run(ev);
                }
                catch (ApplicationOptionException ex)
                {
                    var logging = currentContainer.Resolve<ILoggingService>();
                    logging?.Error("Invalid command line arguments. See exception for details.", ex);
                    Console.WriteLine(ex.Message);
                }
            }
        }

        static OptionSet InitializeOptionsParser()
        {
            var result = new OptionSet
            {
                { "parameter:", "Add parameter", value => {
                    value = value.Trim('\"');
                    var parts = value.Split('=');
                    if(parts.Length == 2)
                    {
                        _options.Parameters.Add(parts[0], parts[1]);
                    }
                }},
                { "project:", "Set the project", value => {
                    _options.ProjectFile = value;
                }}
            };

            return result;
        }

        public static void RegisterComponents(IContainer container)
        {
            container.RegisterService<ILoggingService, LoggingService>();
            container.RegisterService<IExtensibilityService, ExtensibilityService>();
            container.RegisterService<IFileService, FileService>();
            container.RegisterService<IModelManager, ModelManager>();
            container.RegisterService<ApplicationContext, ApplicationContext>();
            container.RegisterService<IOutputLanguageInferenceService, OutputLanguageInferenceService>();
            container.RegisterService<ModelToTextProjectionEngine, ModelToTextProjectionEngine>();
            container.RegisterService<ITemplateEngineProvider, TemplateEngineProvider>();
            container.RegisterService<TextGeneratorRegistry, TextGeneratorRegistry>();
            container.RegisterService<SolutionEngine, SolutionEngine>();

            container.RegisterMany<IModelLoader, JsonModelLoader>();
            container.RegisterMany<IModelLoader, DbModelLoader>();
            container.RegisterMany<ITextTemplateEngine, DotLiquidTemplateEngine>();
        }
    }
}
