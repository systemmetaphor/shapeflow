using System.Collections.Generic;

namespace ShapeFlow.Shapes
{
    public class FileSet
    {
        private List<FileShape> _outputFiles;

        public FileSet()
        {
            _outputFiles = new List<FileShape>();
        }

        public IEnumerable<FileShape> OutputFiles => _outputFiles.AsReadOnly();

        public void Add(FileShape result)
        {
            _outputFiles.Add(result);
        }
    }
}
