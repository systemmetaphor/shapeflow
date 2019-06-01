using System;
using System.IO;
using ShapeFlow.Infrastructure;
using Mono.Options;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using ShapeFlow.Models;
using ShapeFlow.Loaders;
using ShapeFlow.Loaders.DbModel;
using ShapeFlow.TemplateEngines;
using System.Linq;
using ShapeFlow.Commands;

namespace ShapeFlow
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
            using (var currentContainer = ApplicationContainerFactory.Create(Register))
            {
                // naif implementation of command detection

                if (args.Length == 0)
                {
                    args = new string[] { "generate" };
                }

                var commandName = args[0];
                var commandArguments = args.Skip(1).ToArray();

                var commandSystem = currentContainer.Resolve<CommandManagementService>();
                commandSystem.Execute(commandName, commandArguments);
            }
        }

        public static void Register(IContainer container)
        {
            RegisterComponents(container);
            RegisterCommands(container);
        }
        

        public static void RegisterComponents(IContainer container)
        {            
            container.RegisterService<IExtensibilityService, ExtensibilityService>();
            container.RegisterService<IFileService, FileService>();
            container.RegisterService<ModelManager>();
            container.RegisterService<ApplicationContext>();
            container.RegisterService<IOutputLanguageInferenceService, OutputLanguageInferenceService>();
            container.RegisterService<ModelToTextProjectionEngine>();
            container.RegisterService<TemplateEngineProvider>();
            container.RegisterService<TextGeneratorRegistry>();
            container.RegisterService<SolutionEngine>();
            container.RegisterService<CommandManagementService>();

            container.RegisterMany<IModelLoader, JsonModelLoader>();
            container.RegisterMany<IModelLoader, DbModelLoader>();
            container.RegisterMany<ITextTemplateEngine, DotLiquidTemplateEngine>();
        }

        public static void RegisterCommands(IContainer container)
        {
            container.RegisterMany<ICommand, GenerateCommand>();
            container.RegisterMany<ICommand, InitCommand>();
        }
    }
}
