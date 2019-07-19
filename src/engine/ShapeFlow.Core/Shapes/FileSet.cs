using System.Collections.Generic;

namespace ShapeFlow.Shapes
{
    public class FileSet
    {
        private List<FileSetFile> _outputFiles;

        public FileSet()
        {
            _outputFiles = new List<FileSetFile>();
        }

        public IEnumerable<FileSetFile> OutputFiles => _outputFiles.AsReadOnly();

        public void AddFile(FileSetFile result)
        {
            _outputFiles.Add(result);
        }
    }
}
