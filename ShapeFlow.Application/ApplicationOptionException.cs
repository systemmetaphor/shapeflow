using System;
using System.Runtime.Serialization;

namespace ShapeFlow.Infrastructure
{
    [Serializable]
    internal class ApplicationOptionException : Exception
    {
        public ApplicationOptionException()
        {
        }

        public ApplicationOptionException(string message) : base(message)
        {
        }

        public ApplicationOptionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ApplicationOptionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}