using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace paracobNet
{
    class ParamArray : IParam
    {
        public ParamType TypeKey { get; } = ParamType.array;
        
        public ParamArray(BinaryReader reader)
        {

        }

        public void Read(BinaryReader reader)
        {

        }
    }
}
