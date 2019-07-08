using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Options;

namespace ShapeFlow.Infrastructure
{
    internal class CommandManagementService : IInitializable
    {
        private bool _showUsage = false;
        private readonly Regex _commandNameValidator;
        private readonly Dictionary<string, ICommand> _commands;
        private readonly IExtensibilityService _extensibility;
        private readonly IContainer _container;

        public CommandManagementService(IExtensibilityService extensibility, IContainer container)
        {
            _commands = new Dictionary<string, ICommand>();
            _commandNameValidator = new Regex("^[a-zA-Z]+$");
            _extensibility = extensibility;
            _container = container;

            Initialize();
        }

        public void Initialize()
        {
            var commands = _extensibility.LoadExtensions<ICommand>();
            foreach (var command in commands)
            {
                if (!_commands.ContainsKey(command.Name))
                {
                    _commands.Add(command.Name, command);
                }
            }
        }

        public int Execute(string commandName, IEnumerable<string> arguments)
        {   
            var command = GetCommand(commandName);
            if (command != null)
            {
                return command.Execute(arguments);
            }
            else
            {
                AppTrace.Error($"It was not possible to find a command named { commandName }.");
                return -1;
            }
        }

        public ICommand GetCommand(string commandName)
        { 
            if (_commands.ContainsKey(commandName))
            {
                var template = _commands[commandName];
                var type = template.GetType();
                var command = _container.Activate<ICommand>(type);                
                return command;
            }
            else
            {
                return null;
            }
        }
    }
}