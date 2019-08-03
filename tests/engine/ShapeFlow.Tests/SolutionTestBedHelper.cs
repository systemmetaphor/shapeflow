using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;

namespace ShapeFlow.Tests
{
    public class SolutionTestBedHelper
    {
        public const string RecordFolderName = "Records";
        public const string RepositoryFolderName = "Repositories";
        public const string ShapeflowPackages = ".shapeflow";

        private readonly string _projectName;

        public SolutionTestBedHelper(string rootFolder = "", string projectName = "")
        {
            RootFolder = rootFolder;
            if (string.IsNullOrWhiteSpace(RootFolder))
            {
                RootFolder = Environment.CurrentDirectory;
            }

            _projectName = projectName;
            if (string.IsNullOrWhiteSpace(_projectName))
            {
                _projectName = "Project1";
            }

            // TODO: consider the case where the folder already exists and you need to find the next project number

            SolutionDir = Path.Combine(RootFolder, _projectName);
        }

        public string SolutionDir { get; }

        public string RootFolder { get; }

        public string RecordsFolder { get; private set; }

        public string RepositoriesFolder { get; private set; }

        public string ShapeflowFolder { get; private set; }

        public void Create()
        {
            CreateFolders();
            CreateFiles();
        }

        private void CreateFolders()
        {
            if (!Directory.Exists(RootFolder))
            {
                Directory.CreateDirectory((RootFolder));
            }

            if (!Directory.Exists(SolutionDir))
            {
                Directory.CreateDirectory((SolutionDir));
            }
            
            var recordsFolder = Path.Combine(SolutionDir, RecordFolderName);
            var repositoriesFolder = Path.Combine(SolutionDir, RepositoryFolderName);
            var shapeflowFolder = Path.Combine(SolutionDir, ShapeflowPackages);

            if (!Directory.Exists(recordsFolder))
            {
                Directory.CreateDirectory((recordsFolder));
            }

            if (!Directory.Exists(repositoriesFolder))
            {
                Directory.CreateDirectory((repositoriesFolder));
            }

            if (!Directory.Exists(shapeflowFolder))
            {
                Directory.CreateDirectory((shapeflowFolder));
            }

            RecordsFolder = recordsFolder;
            RepositoriesFolder = repositoriesFolder;
            ShapeflowFolder = shapeflowFolder;
        }

        private void CreateFiles()
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"<Project Sdk=""Microsoft.NET.Sdk"">");
            sb.AppendLine(@"");
            sb.AppendLine(@"  <PropertyGroup>");
            sb.AppendLine(@"    <TargetFramework>netstandard2.0</TargetFramework>");
            sb.AppendLine(@"  </PropertyGroup>");
            sb.AppendLine(@"");
            sb.AppendLine(@"  <ItemGroup>");
            sb.AppendLine(@"    <Folder Include=""Repositories\"" />");
            sb.AppendLine(@"    <Folder Include=""Records\"" />");
            sb.AppendLine(@"  </ItemGroup>");
            sb.AppendLine(@"");
            sb.AppendLine(@"</Project>");
            
            var projectFileName = $"{_projectName}.csproj";
            var projectFilePath = Path.Combine(SolutionDir, projectFileName);

            if (File.Exists(projectFilePath))
            {
                File.Delete(projectFilePath);
            }

            File.WriteAllText(projectFilePath, sb.ToString());

            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();

            sb = new StringBuilder(1118);
            sb.AppendLine(@"");
            sb.AppendLine(@"Microsoft Visual Studio Solution File, Format Version 12.00");
            sb.AppendLine(@"# Visual Studio Version 16");
            sb.AppendLine(@"VisualStudioVersion = 16.0.29025.244");
            sb.AppendLine(@"MinimumVisualStudioVersion = 10.0.40219.1");
            sb.AppendLine($@"Project(""{{{ guid1 }}}"") = ""{ _projectName }"", ""{ projectFileName }"", ""{{{ guid2 }}}""");
            sb.AppendLine(@"EndProject");
            sb.AppendLine(@"Global");
            sb.AppendLine(@"	GlobalSection(SolutionConfigurationPlatforms) = preSolution");
            sb.AppendLine(@"		Debug|Any CPU = Debug|Any CPU");
            sb.AppendLine(@"		Release|Any CPU = Release|Any CPU");
            sb.AppendLine(@"	EndGlobalSection");
            sb.AppendLine(@"	GlobalSection(ProjectConfigurationPlatforms) = postSolution");
            sb.AppendLine($@"		{{{guid2}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
            sb.AppendLine($@"		{{{guid2}}}.Debug|Any CPU.Build.0 = Debug|Any CPU");
            sb.AppendLine($@"		{{{guid2}}}.Release|Any CPU.ActiveCfg = Release|Any CPU");
            sb.AppendLine($@"		{{{guid2}}}.Release|Any CPU.Build.0 = Release|Any CPU");
            sb.AppendLine(@"	EndGlobalSection");
            sb.AppendLine(@"	GlobalSection(SolutionProperties) = preSolution");
            sb.AppendLine(@"		HideSolutionNode = FALSE");
            sb.AppendLine(@"	EndGlobalSection");
            sb.AppendLine(@"	GlobalSection(ExtensibilityGlobals) = postSolution");
            sb.AppendLine($@"		SolutionGuid = {{{guid3}}}");
            sb.AppendLine(@"	EndGlobalSection");
            sb.AppendLine(@"EndGlobal");

            var solutionFileName = $"{_projectName}.sln";
            var solutionFilePath = Path.Combine(SolutionDir, solutionFileName);

            if (File.Exists(solutionFilePath))
            {
                File.Delete(solutionFilePath);
            }

            File.WriteAllText(solutionFilePath, sb.ToString());
        }

        public void AddFile(string sourceFilePath)
        {
            if (File.Exists(sourceFilePath))
            {
                var destinationFileName = Path.GetFileName(sourceFilePath);
                var destinationPath = Path.Combine(SolutionDir, destinationFileName);
                File.Copy(sourceFilePath, destinationPath, true);
            }
            else
            {
                throw new FileNotFoundException("It was not possible to add a file to the test bed", sourceFilePath);
            }
        }

        public bool FileExists(string path)
        {
            return File.Exists(Path.Combine(SolutionDir, path));
        }
    }
}
