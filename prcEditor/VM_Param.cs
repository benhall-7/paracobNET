using paracobNET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace prcEditor
{
    //Presenting...
    //The world's most complicated inheritance structure!!!

    public abstract class VM_Param
    {
        public IParam Param { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Represents a generic struct
    /// </summary>
    public abstract class VM_ParamStruct : VM_Param
    {
        public new ParamStruct Param
        {
            get => (ParamStruct)base.Param;
            set => base.Param = value;
        }

        public ObservableCollection<IStructChild> Children { get; set; }

        public VM_ParamStruct(ParamStruct param)
        {
            Param = param;
            Children = new ObservableCollection<IStructChild>();
            foreach (var node in param.Nodes)
            {
                switch (node.Value.TypeKey)
                {
                    case ParamType.@struct:
                        Children.Add(new VM_StructStruct(node.Value as ParamStruct, this));
                        break;
                    case ParamType.list:
                        Children.Add(new VM_StructList(node.Value as ParamList, this));
                        break;
                    default:
                        Children.Add(new VM_StructValue(node.Value as ParamValue, this));
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Represents a generic list
    /// </summary>
    public abstract class VM_ParamList : VM_Param
    {
        public new ParamList Param
        {
            get => (ParamList)base.Param;
            set => base.Param = value;
        }

        public ObservableCollection<IListChild> Children { get; set; }

        public VM_ParamList(ParamList param)
        {
            Param = param;
            Children = new ObservableCollection<IListChild>();
            foreach (var node in param.Nodes)
            {
                switch (node.TypeKey)
                {
                    case ParamType.@struct:
                        Children.Add(new VM_ListStruct(node as ParamStruct, this));
                        break;
                    case ParamType.list:
                        Children.Add(new VM_ListList(node as ParamList, this));
                        break;
                    default:
                        Children.Add(new VM_ListValue(node as ParamValue, this));
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Represents a generic value
    /// </summary>
    public abstract class VM_ParamValue : VM_Param
    {
        public new ParamValue Param
        {
            get => (ParamValue)base.Param;
            set => base.Param = value;
        }

        public VM_ParamValue(ParamValue param)
        {
            Param = param;
        }
    }

    /// <summary>
    /// Represents a generic child of a list
    /// </summary>
    public interface IListChild
    {
        VM_ParamList Parent { get; set; }
    }

    /// <summary>
    /// Represents a generic child of a struct
    /// </summary>
    public interface IStructChild
    {
        VM_ParamStruct Parent { get; set; }
    }

    public class VM_ListList : VM_ParamList, IListChild
    {
        public VM_ParamList Parent { get; set; }

        public VM_ListList(ParamList list, VM_ParamList parent) : base(list)
        {
            Parent = parent;
        }
    }

    public class VM_ListStruct : VM_ParamStruct, IListChild
    {
        public VM_ParamList Parent { get; set; }

        public VM_ListStruct(ParamStruct struc, VM_ParamList parent) : base(struc)
        {
            Parent = parent;
        }
    }

    public class VM_ListValue : VM_ParamValue, IListChild
    {
        public VM_ParamList Parent { get; set; }

        public VM_ListValue(ParamValue value, VM_ParamList parent) : base(value)
        {
            Parent = parent;
        }
    }

    public class VM_StructList : VM_ParamList, IStructChild
    {
        public VM_ParamStruct Parent { get; set; }

        public VM_StructList(ParamList list, VM_ParamStruct parent) : base(list)
        {
            Parent = parent;
        }
    }

    public class VM_StructStruct : VM_ParamStruct, IStructChild
    {
        public VM_ParamStruct Parent { get; set; }

        public VM_StructStruct(ParamStruct struc, VM_ParamStruct parent) : base(struc)
        {
            Parent = parent;
        }
    }

    public class VM_StructValue : VM_ParamValue, IStructChild
    {
        public VM_ParamStruct Parent { get; set; }

        public VM_StructValue(ParamValue value, VM_ParamStruct parent) : base(value)
        {
            Parent = parent;
        }
    }
}
