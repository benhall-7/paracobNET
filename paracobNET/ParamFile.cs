using System;

namespace paracobNET
{
    public class ParamFile
    {
        public const string Magic = "paracobn";
        public ParamStruct Root { get; set; }

        public event EventHandler<DuplicateKeyEventArgs> RaiseDuplicateKeyEvent;

        public ParamFile()
        {
            Root = new ParamStruct();
        }
        public ParamFile(ParamStruct root)
        {
            Root = root;
        }

        public void Open(string filepath)
        {
            Disassembler disassembler = new Disassembler(filepath);
            disassembler.RaiseDuplicateKeyEvent = RaiseDuplicateKeyEvent;
            Root = disassembler.Start();
        }

        public void Save(string filepath)
        {
            Assembler assembler = new Assembler(filepath, Root);
            assembler.Start();
        }
    }
}
