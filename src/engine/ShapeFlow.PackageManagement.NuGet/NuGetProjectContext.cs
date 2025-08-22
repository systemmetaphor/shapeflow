using System;
using System.Xml.Linq;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.PackageExtraction;
using NuGet.ProjectManagement;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.PackageManagement.NuGet
{
    internal class NuGetProjectContext : INuGetProjectContext
    {
        private Guid _operationId;

        public NuGetProjectContext()
        {
            PackageExtractionContext = new PackageExtractionContext(
                PackageSaveMode.Defaultv2,
                PackageExtractionBehavior.XmlDocFileSaveMode,
                clientPolicyContext: null,
                logger: NullLogger.Instance);
        }

        public void Log(MessageLevel level, string message, params object[] args)
        {
            switch (level)
            {
                case MessageLevel.Warning:
                    AppTrace.Warning(message, args);
                    break;
                case MessageLevel.Error:
                    AppTrace.Error(message, args);
                    break;
                default:
                    AppTrace.Verbose(message, args);
                    break;
            }
        }

        public FileConflictAction ResolveFileConflict(string message) => FileConflictAction.Ignore;

        public PackageExtractionContext PackageExtractionContext { get; set; }

        public XDocument OriginalPackagesConfig { get; set; }

        public ISourceControlManagerProvider SourceControlManagerProvider { get; set; }

        public ExecutionContext ExecutionContext { get; set; }

        public void ReportError(string message)
        {
            AppTrace.Error(message);
        }

        public NuGetActionType ActionType { get; set; }

        public Guid OperationId
        {
            get
            {
                if (_operationId == Guid.Empty)
                {
                    _operationId = Guid.NewGuid();
                }

                return _operationId;
            }

            set => _operationId = value;
        }
    }
}