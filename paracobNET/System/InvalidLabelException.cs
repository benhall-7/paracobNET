namespace paracobNET
{
    public class InvalidLabelException : Exception
    {
        public string Label { get; set; }

        public override string Message => $"{base.Message} (Label = '{Label}')";

        public InvalidLabelException(string label) : base() { Label = label; }

        public InvalidLabelException(string message, string label): base(message)
        {
            Label = label;
        }

        public InvalidLabelException(string message, Exception innerException, string label)
            : base(message, innerException)
        {
            Label = label;
        }
    }
}
