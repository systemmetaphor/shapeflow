using CodeClassification;

namespace ShapeFlow.Output
{
    internal class OutputLanguageInferenceService : IOutputLanguageInferenceService
    {
        public string InferFileExtension(string text)
        {
            // cancel out the classification output messages
            CodeClassifier.Output = null;

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
