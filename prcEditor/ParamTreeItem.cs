using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using paracobNET;

namespace prcEditor
{
    public class ParamTreeItem
    {
        public IParam Param { get; set; }
        public ParamTreeItem Parent { get; set; }
        public object ParentAccessor { get; set; }
        public List<ParamTreeItem> Items { get; set; }

        public string Name
        {
            get
            {
                string name = Param.TypeKey.ToString();

                if (Parent == null)
                    return name;

                switch (Parent.Param.TypeKey)
                {
                    case ParamType.@struct:
                        name += " " + Hash40Util.FormatToString((ulong)ParentAccessor, MainWindow.HashToStringLabels);
                        break;
                    case ParamType.list:
                        name += " " + ((int)ParentAccessor).ToString();
                        break;
                }

                return name;
            }
        }

        public ParamTreeItem(IParam param, ParamTreeItem parent, object parentAccessor)
        {
            Param = param;
            Parent = parent;
            ParentAccessor = parentAccessor;

            switch (Param.TypeKey)
            {
                case ParamType.@struct:
                    {
                        List<ParamTreeItem> Items = new List<ParamTreeItem>();
                        foreach (var node in (Param as ParamStruct).Nodes)
                            Items.Add(new ParamTreeItem(node.Value, this, node.Key));
                        this.Items = Items;
                        break;
                    }
                case ParamType.list:
                    {
                        List<ParamTreeItem> Items = new List<ParamTreeItem>();
                        var list = (Param as ParamList).Nodes;
                        for (int i = 0; i < list.Count; i++)
                            Items.Add(new ParamTreeItem(list[i], this, i));
                        this.Items = Items;
                        break;
                    }
            }
        }
    }
}
