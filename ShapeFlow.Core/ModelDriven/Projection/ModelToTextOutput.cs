using System.Collections.Generic;

namespace ShapeFlow
{
    public class ModelToTextOutput
    {
        private List<ModelToTextOutputFile> _outputFiles;

        public ModelToTextOutput()
        {
            _outputFiles = new List<ModelToTextOutputFile>();
        }

        public IEnumerable<ModelToTextOutputFile> OutputFiles => _outputFiles.AsReadOnly();

        public void AddOutputFile(ModelToTextOutputFile result)
        {
            _outputFiles.Add(result);
        }
    }
}
