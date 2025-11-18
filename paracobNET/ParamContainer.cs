namespace paracobNET
{
    public class ParamContainer
    {
        // "param container binary"
        public const string Magic = "paracobn";
        public ParamMapNode Root { get; set; }

        public ParamContainer()
        {
            Root = new ParamMapNode([]);
        }
        public ParamContainer(ParamMapNode root)
        {
            Root = root;
        }

        public ParamContainer(string filepath)
        {
            Disassembler disassembler = new Disassembler();
            var file = File.OpenRead(filepath);
            Root = disassembler.Start(file);
        }

        public ParamContainer(Stream source)
        {
            Disassembler disassembler = new Disassembler();
            Root = disassembler.Start(source);
        }

        public void SaveFile(string filepath)
        {
            if (filepath == null) throw new ArgumentNullException(nameof(filepath));
            using var file = File.Create(filepath);
            ParamWriter.Write(this, file);
        }

        public void SaveStream(Stream destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            ParamWriter.Write(this, destination);
        }
    }
}
