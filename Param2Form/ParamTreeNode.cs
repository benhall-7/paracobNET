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
        public Hash40 Hash { get; set; }

        public ParamTreeNode(IParam param, string text) : base(text)
        {
            ContainsHash = false;
            Hash = new Hash40(0);
            Param = param;
        }
    }
}
