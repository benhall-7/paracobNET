using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace paracobNet
{
    public class ParamValue : IParam
    {
        public ParamType TypeKey { get; set; }

        public ParamValue(BinaryReader reader, ParamType type)
        {

        }

        public void Read(BinaryReader reader)
        {

        }
    }
}
