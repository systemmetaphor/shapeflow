using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotNetFileUtils
{
    [Serializable]
    public class DotNetFileUtilsException : Exception
    {
        public DotNetFileUtilsException()
        {
        }

        public DotNetFileUtilsException(string message) : base(message)
        {
        }

        public DotNetFileUtilsException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DotNetFileUtilsException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
