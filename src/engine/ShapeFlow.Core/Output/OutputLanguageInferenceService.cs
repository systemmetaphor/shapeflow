using CodeClassification;

namespace ShapeFlow
{
    internal class OutputLanguageInferenceService : IOutputLanguageInferenceService
    {
        public string InferFileExtension(string text)
        {
            var language = CodeClassifier.Classify(text);

            switch(language.ToLower())
            {
                case "csharp":
                    return ".cs";

                case "c++":
                    return ".cpp";

                case "c":
                    return ".c";

                case "html":
                    return ".html";

                case "javascript":
                    return ".js";

                case "python":
                    return ".py";

                case "ruby":
                    return ".rb";
            }

            return ".txt";
        }
    }
}
