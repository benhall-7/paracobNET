namespace paracobNET
{
    public class ParamFile
    {
        public const string Magic = "paracobn";
        public ParamStruct Root { get; set; }

        public ParamFile() { }
        public ParamFile(ParamStruct root)
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
