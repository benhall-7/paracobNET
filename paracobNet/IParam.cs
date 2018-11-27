using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace paracobNet
{
    public interface IParam
    {
        ParamType TypeKey { get; }
        
        void Read(BinaryReader reader);
    }
}
