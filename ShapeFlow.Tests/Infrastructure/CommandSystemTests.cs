using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Tests.Infrastructure
{
    [TestClass]
    public class CommandSystemTests
    {
        [TestMethod]
        public void CanResolveCommand()
        {
            using (var currentContainer = ApplicationContainerFactory.Create(RegisterComponents))
            {
                currentContainer.RegisterMany<ICommand, FakeCommmand>();
                currentContainer.RegisterMany<ICommand, CopyCommmand>();

                var service = currentContainer.Resolve<CommandManagementService>();

                var command = service.GetCommand("fake") as FakeCommmand;
                Assert.IsNotNull(command);

                command.Execute(Enumerable.Empty<string>());
                Assert.IsTrue(command.Executed);
            }
        }


        class FakeCommmand : Command
        {
            public override string Name => "fake";

            public bool Executed { get; set; }

            protected override int OnExecute(CommandOptions options)
            {
                Executed = true;

                return 0;
            }
        }

        class CopyCommmand : Command
        {
            public override string Name => "copy";

            public bool Executed { get; set; }

            protected override int OnExecute(CommandOptions options)
            {
                Executed = true;

                return 0;
            }
        }


        static void RegisterComponents(IContainer container)
        {
            container.RegisterService<IExtensibilityService, ExtensibilityService>();
            container.RegisterService<CommandManagementService, CommandManagementService>();
        }
    }
}
