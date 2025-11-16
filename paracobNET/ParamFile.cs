namespace paracobNET
{
    public class ParamFile
    {
        public const string Magic = "paracobn";
        public ParamMapNode Root { get; set; }

        public ParamFile()
        {
            Root = new ParamMapNode([]);
        }
        public ParamFile(ParamMapNode root)
        {
            Root = root;
        }

        public void Open(string filepath)
        {
            Disassembler disassembler = new Disassembler(filepath);
            Root = disassembler.Start();
        }

        public void Save(string filepath)
        {
            Assembler assembler = new Assembler(filepath, Root);
            assembler.Start();
        }
    }
}
