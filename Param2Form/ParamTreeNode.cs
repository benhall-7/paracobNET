using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using paracobNET;

namespace Param2Form
{
    class ParamTreeNode : TreeNode
    {
        public IParam Param { get; set; }
        public bool ContainsHash { get; set; }
        public ulong Hash { get; set; }

        public ParamTreeNode(IParam param, string text) : base(text)
        {
            ContainsHash = false;
            Hash = 0;
            Param = param;
        }
    }
}
