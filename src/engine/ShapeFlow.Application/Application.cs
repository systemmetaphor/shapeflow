using System.Linq;
using System.Threading.Tasks;
using ShapeFlow.Commands;
using ShapeFlow.Infrastructure;
using ShapeFlow.Loaders;
using ShapeFlow.Loaders.DbModel;
using ShapeFlow.PackageManagement;
using ShapeFlow.PackageManagement.NuGet;
using ShapeFlow.Projections;
using ShapeFlow.Shapes;
using ShapeFlow.TemplateEngines.DotLiquid;
using ShapeFlow.TemplateEngines.T4;

namespace ShapeFlow
{
    public static class Application
    {
        static Application()
        {
        }

        public static async Task Run(string[] args)
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
                var result = await commandSystem.Execute(commandName, commandArguments);
                if (result < 0)
                {
                    AppTrace.Error("Command failed.");
                }
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
            container.RegisterService<ShapeManager>();
            container.RegisterService<ApplicationContext>();
            container.RegisterService<IOutputLanguageInferenceService, OutputLanguageInferenceService>();
            container.RegisterService<ProjectionEngine>();
            container.RegisterService<TemplateEngineProvider>();
            container.RegisterService<ProjectionRegistry>();
            container.RegisterService<ShapeFlowEngine>();
            container.RegisterService<CommandManagementService>();
            container.RegisterService<PackageManagerFactory, NugetPackageManagerFactory>();

            container.RegisterMany<ILoader, JsonLoader>();
            container.RegisterMany<ILoader, DbModelLoader>();
            container.RegisterMany<ITextTemplateEngine, DotLiquidTemplateEngine>();
            container.RegisterMany<ITextTemplateEngine, T4TemplateEngine>();
        }

        public static void RegisterCommands(IContainer container)
        {
            container.RegisterMany<ICommand, GenerateCommand>();
            container.RegisterMany<ICommand, InitCommand>();
        }
    }
}
