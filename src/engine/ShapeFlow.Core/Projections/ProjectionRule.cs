using System;
using System.IO;
using ShapeFlow.Declaration;

namespace ShapeFlow.Projections
{
    public class ProjectionRule : IDisposable
    {        
        private string _text;

        private ProjectionRule()
        {
        }

        public ProjectionRuleDeclaration RuleDecl { get; private set; }

        public string Path { get; private set; }

        public string Text
        {
            get => _text ?? (_text = ReadText());

            set
            {
                if (Stream != null)
                {
                    Stream.Dispose();
                    Stream = null;
                }

                _text = value;
            }
        }

        protected Stream Stream { get; set; }

        public static ProjectionRule Create(string rule, string language)
        {
            var result = new ProjectionRule()
            {
                RuleDecl = new ProjectionRuleDeclaration(string.Empty, language),
                _text =  rule
            };

            return result;
        }

        public static ProjectionRule Create(ProjectionRuleDeclaration rule)
        {
            return new ProjectionRule
            {
                RuleDecl = rule,
                Path = rule.FileName
            };
        }

        public static ProjectionRule Create(ProjectionRuleDeclaration rule, string path)
        {
            return new ProjectionRule
            {
                RuleDecl = rule,
                Path = path
            };
        }

        public static ProjectionRule Create(ProjectionRuleDeclaration rule, Stream stream)
        {
            return new ProjectionRule
            {
                RuleDecl = rule,
                Path = rule.FileName,
                Stream =  stream
            };
        }

        private string ReadText()
        {
            string result = null;

            EnsureStream();

            if (Stream != null)
            {
                using (var reader = new StreamReader(Stream))
                {
                    result = reader.ReadToEnd();
                }
            }

            return result;
        }

        private void EnsureStream()
        {
            if (Stream == null && !string.IsNullOrWhiteSpace(Path))
            {
                try
                {
                    Stream = File.OpenRead(Path);
                }
                catch (Exception)
                {
                    // explicitly silenced
                }
            }
        }

        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}
