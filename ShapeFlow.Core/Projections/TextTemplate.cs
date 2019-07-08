using System;
using System.IO;
using ShapeFlow.Declaration;

namespace ShapeFlow.Projections
{
    public class TextTemplate : IDisposable
    {        
        private string _text;

        private TextTemplate(TransformationRuleDeclaration step, string path)
        {
            Rule = step;
            Path = path;                 
        }

        private TextTemplate(TransformationRuleDeclaration step, Stream stream)
        {
            Rule = step;
            Stream = stream;
        }

        public TransformationRuleDeclaration Rule { get; }

        public string Path { get; }

        public Stream Stream { get; private set; }

        public string Text => _text ?? (_text = ReadText());

        public void CopyTo(Stream other)
        {
            EnsureStream();

            if (Stream != null)
            {
                Stream.Seek(0, SeekOrigin.Begin);
                Stream.CopyTo(other);
                Stream.Seek(0, SeekOrigin.Begin);
            }
        }

        public static TextTemplate Create(TransformationRuleDeclaration rule, string path)
        {
            return new TextTemplate(rule, path);       
        }

        public static TextTemplate Create(TransformationRuleDeclaration rule, Stream stream)
        {
            return new TextTemplate(rule, stream);
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
