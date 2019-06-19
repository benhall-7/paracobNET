using System;
using System.Collections.Generic;
using System.Text;

namespace paracobNET
{
    public class InvalidLabelException : Exception
    {
        public string Label { get; set; }

        public override string Message
        {
            get
            {
                if (string.IsNullOrEmpty(Label))
                    return base.Message;
                else
                    return $"{base.Message} (Label = '{Label}')";
            }
        }

        public InvalidLabelException() : base() { }
        public InvalidLabelException(string message) : base(message) { }
        public InvalidLabelException(string message, Exception innerException) : base(message, innerException) { }
    }
}
