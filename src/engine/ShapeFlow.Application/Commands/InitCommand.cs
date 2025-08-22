using System;
using System.IO;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.Output;

namespace ShapeFlow.Commands
{
    public class InitCommand : Command
    {
        private readonly IFileService _fileService;

        public InitCommand(IFileService fileService)
        {
            _fileService = fileService;
        }

        public override string Name => "init";

        protected override Task<int> OnExecute(CommandOptions options)
        {
            var directory = Environment.CurrentDirectory;
            var name = Path.GetFileNameWithoutExtension(directory);

            var initTemplate = InitTemplate.Generate(new InitTemplateOptions { ProjectName = name, RootDirectory =  directory });
            
            var file = Path.Combine(directory, "shapeflow.config.json");
            _fileService.PerformWrite(file, initTemplate);
            
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(@"using System;");
            sb.AppendLine(@"using System.Collections.Generic;");
            sb.AppendLine(@"using System.Linq;");
            sb.AppendLine(@"using System.Text;");
            sb.AppendLine(@"using System.Threading.Tasks;");
            
            sb.AppendLine(@"");
            sb.AppendLine(@"namespace {{namespace}}");
            sb.AppendLine(@"{");
            sb.AppendLine(@"{% for item in Entities -%}");
            sb.AppendLine(@"	public class {{item.ObjectName}}Record");
            sb.AppendLine(@"	{");
            sb.AppendLine(@"{% for property in item.Properties -%}");
            sb.AppendLine(@"		public {{ property.SqlDataType | sqltocsharp }} {{property.Name}} {get; set;}");
            sb.AppendLine(@"{% endfor -%}");
            sb.AppendLine(@"	}");
            sb.AppendLine(@"{% unless forloop.last -%}");
            sb.AppendLine(@"");
            sb.AppendLine(@"{% endunless -%}");
            sb.AppendLine(@"{% endfor -%}");
            sb.AppendLine(@"}");

            file = Path.Combine(directory, "Templates\\Data\\Records.liquid");
            _fileService.PerformWrite(file, sb.ToString());

            return Task.FromResult(0);
        }
    }
}
