using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using paracobNET;

namespace prcEditor
{
    public class VM_ParamList : VM_ParamBase
    {
        public ObservableCollection<VM_ParamBase> Children { get; set; }

        public VM_ParamList(ParamList list, VM_ParamBase parent) : base(list, parent)
        {
            Children = new ObservableCollection<VM_ParamBase>();
            foreach (var param in list.Nodes)
                AddChild(param);
        }

        protected void AddChild(IParam param)
        {
            switch (param.TypeKey)
            {
                case ParamType.@struct:
                    Children.Add(new VM_ParamStruct(param as ParamStruct, this));
                    break;
                case ParamType.list:
                    Children.Add(new VM_ParamList(param as ParamList, this));
                    break;
                default:
                    Children.Add(new VM_ParamValue(param as ParamValue, this));
                    break;
            }
        }
    }
}
