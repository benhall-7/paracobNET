using System;
using System.Collections.Generic;
using System.Text;

namespace paracobNET
{
    public class InvalidHeaderException : Exception
    {
        public InvalidHeaderException() : base() { }
        public InvalidHeaderException(string message) : base(message) { }
        public InvalidHeaderException(string message, Exception innerException) : base(message, innerException) { }
    }
}
