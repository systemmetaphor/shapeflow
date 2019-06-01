using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ShapeFlow.Infrastructure;

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

        protected override int OnExecute(CommandOptions options)
        {
            var directory = Environment.CurrentDirectory;
            var name = Path.GetFileNameWithoutExtension(directory);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(@"{");
            sb.AppendLine($@"  ""name"": ""Generate { name } files"",");
            sb.AppendLine(@"  ""generators"": {");
            sb.AppendLine(@"    ""TablesToRecords"": {");
            sb.AppendLine(@"      ""version"": ""1.0.0"",");
            sb.AppendLine(@"      ""loaderName"": ""DbModelLoader"",");
            sb.AppendLine(@"      ""packageRoot"": ""$(project-root)"",");
            sb.AppendLine(@"      ""rules"": [");
            sb.AppendLine(@"        {");
            sb.AppendLine(@"          ""templateName"": ""Templates\\TablesToRecords.liquid"",");
            sb.AppendLine(@"          ""outputPathTemplate"": ""Generated\\Records.cs""");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"      ]");
            sb.AppendLine(@"    }");
            sb.AppendLine(@"  },");
            sb.AppendLine(@"  ""models"": [");
            sb.AppendLine(@"    {");
            sb.AppendLine($@"      ""name"": ""{ name } Model"",");
            sb.AppendLine(@"      ""tags"": ""dbEntityModel"",");
            sb.AppendLine(@"      ""loaderName"": ""DbModelLoader"",");
            sb.AppendLine(@"      ""parameters"": {");
            sb.AppendLine(@"        ""server"": """",");
            sb.AppendLine(@"        ""db"": """",");
            sb.AppendLine(@"        ""user"": """",");
            sb.AppendLine(@"        ""password"": """",");
            sb.AppendLine(@"        ""namespace"" :  """"");
            sb.AppendLine(@"      }");
            sb.AppendLine(@"    }");
            sb.AppendLine(@"  ],");
            sb.AppendLine(@"  ""pipelines"": [");
            sb.AppendLine(@"    {");
            sb.AppendLine($@"      ""name"": ""{ name } Model Data Tier"",");
            sb.AppendLine(@"");
            sb.AppendLine(@"      ""input"": {");
            sb.AppendLine($@"        ""selector"": ""{ name } Model""");
            sb.AppendLine(@"      },");
            sb.AppendLine(@"");
            sb.AppendLine(@"      ""transformation"": {");
            sb.AppendLine($@"        ""name"": ""{ name } Model Data Tier"",");
            sb.AppendLine(@"        ""generator"": ""TablesToRecords"",");
            sb.AppendLine(@"        ""input"": {");
            sb.AppendLine($@"          ""selector"": ""{ name } Model""");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"      },");
            sb.AppendLine(@"");
            sb.AppendLine(@"      ""output"": {");
            sb.AppendLine(@"        ""type"": ""file"",");
            sb.AppendLine(@"        ""parameters"": {");
            sb.AppendLine(@"          ""output-mode"": ""overwrite""");
            sb.AppendLine(@"        }");
            sb.AppendLine(@"      }");
            sb.AppendLine(@"    }");
            sb.AppendLine(@"  ]");
            sb.AppendLine(@"}");


            var file = Path.Combine(directory, "shapeflow.config.json");
            _fileService.PerformWrite(file, sb.ToString());

            sb = new System.Text.StringBuilder();

            sb.AppendLine(@"using System;");
            sb.AppendLine(@"using System.Collections.Generic;");
            sb.AppendLine(@"using System.Linq;");
            sb.AppendLine(@"using System.Text;");
            sb.AppendLine(@"using System.Threading.Tasks;");
            sb.AppendLine(@"");
            sb.AppendLine(@"namespace {{namespace}}");
            sb.AppendLine(@"{");
            sb.AppendLine(@"{% for item in Root.Entities -%}");
            sb.AppendLine(@"	public class {{item.ObjectName}}Record");
            sb.AppendLine(@"	{");
            sb.AppendLine(@"{% for property in item.Properties -%}");
            sb.AppendLine(@"		public {{property.PropertyDotNetType}} {{property.PropertyName}} {get; set;}");
            sb.AppendLine(@"{% endfor -%}");
            sb.AppendLine(@"	}");
            sb.AppendLine(@"{% endfor -%}");
            sb.AppendLine(@"}");

            file = Path.Combine(directory, "templates\\TablesToRecords.liquid");
            _fileService.PerformWrite(file, sb.ToString());

            return 0;
        }
    }
}
