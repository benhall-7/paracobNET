﻿using paracobNET;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace prcEditor
{
    //Presenting...
    //The world's most complicated inheritance structure!!!

    public abstract class VM_Param : INotifyPropertyChanged
    {
        public IParam Param { get; set; }
        
        public abstract string Name { get; }

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
                        Children.Add(new VM_StructStruct(node.Value as ParamStruct, this, node.Key));
                        break;
                    case ParamType.list:
                        Children.Add(new VM_StructList(node.Value as ParamList, this, node.Key));
                        break;
                    default:
                        Children.Add(new VM_StructValue(node.Value as ParamValue, this, node.Key));
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
            for (int i = 0; i < param.Nodes.Count; i++)
            {
                IParam node = param.Nodes[i];
                switch (node.TypeKey)
                {
                    case ParamType.@struct:
                        Children.Add(new VM_ListStruct(node as ParamStruct, this, i));
                        break;
                    case ParamType.list:
                        Children.Add(new VM_ListList(node as ParamList, this, i));
                        break;
                    default:
                        Children.Add(new VM_ListValue(node as ParamValue, this, i));
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
    /// Represents a generic child of a struct
    /// </summary>
    public interface IStructChild
    {
        VM_ParamStruct Parent { get; set; }
        ulong Hash40 { get; set; }
    }

    /// <summary>
    /// Represents a generic child of a list
    /// </summary>
    public interface IListChild
    {
        VM_ParamList Parent { get; set; }
        int Index { get; set; }
    }

    public class VM_ParamRoot : VM_ParamStruct
    {
        public override string Name => Param.TypeKey.ToString();

        public VM_ParamRoot(ParamStruct struc) : base(struc) { }
    }

    public class VM_StructStruct : VM_ParamStruct, IStructChild
    {
        public VM_ParamStruct Parent { get; set; }
        private ulong _hash40;
        public ulong Hash40
        {
            get { return _hash40; }
            set
            {
                if (_hash40 == value)
                    return;
                Util.ChangeStructChildHash40(this, ref _hash40, value);
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public override string Name => Util.GetStructChildName(Param, Hash40);

        public VM_StructStruct(ParamStruct struc, VM_ParamStruct parent, ulong hash40) : base(struc)
        {
            Parent = parent;
            _hash40 = hash40;
        }
    }

    public class VM_StructList : VM_ParamList, IStructChild
    {
        public VM_ParamStruct Parent { get; set; }
        private ulong _hash40;
        public ulong Hash40
        {
            get { return _hash40; }
            set
            {
                if (_hash40 == value)
                    return;
                Util.ChangeStructChildHash40(this, ref _hash40, value);
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public override string Name => Util.GetStructChildName(Param, Hash40);

        public VM_StructList(ParamList list, VM_ParamStruct parent, ulong hash40) : base(list)
        {
            Parent = parent;
            _hash40 = hash40;
        }
    }

    public class VM_StructValue : VM_ParamValue, IStructChild
    {
        public VM_ParamStruct Parent { get; set; }
        private ulong _hash40;
        public ulong Hash40
        {
            get { return _hash40; }
            set
            {
                if (_hash40 == value)
                    return;
                Util.ChangeStructChildHash40(this, ref _hash40, value);
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public override string Name => Util.GetStructChildName(Param, Hash40);

        public VM_StructValue(ParamValue value, VM_ParamStruct parent, ulong hash40) : base(value)
        {
            Parent = parent;
            _hash40 = hash40;
        }
    }

    public class VM_ListStruct : VM_ParamStruct, IListChild
    {
        public VM_ParamList Parent { get; set; }
        public int Index { get; set; }

        public override string Name => Util.GetListChildName(Param, Index);

        public VM_ListStruct(ParamStruct struc, VM_ParamList parent, int index) : base(struc)
        {
            Parent = parent;
            Index = index;
        }
    }

    public class VM_ListList : VM_ParamList, IListChild
    {
        public VM_ParamList Parent { get; set; }
        public int Index { get; set; }

        public override string Name => Util.GetListChildName(Param, Index);

        public VM_ListList(ParamList list, VM_ParamList parent, int index) : base(list)
        {
            Parent = parent;
            Index = index;
        }
    }

    public class VM_ListValue : VM_ParamValue, IListChild
    {
        public VM_ParamList Parent { get; set; }
        public int Index { get; set; }

        public override string Name => Util.GetListChildName(Param, Index);

        public VM_ListValue(ParamValue value, VM_ParamList parent, int index) : base(value)
        {
            Parent = parent;
            Index = index;
        }
    }
}