using paracobNET;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace prcEditor
{
    #region ABSTRACT_PARAM_MODEL

    public abstract class VM_Param : INotifyPropertyChanged
    {
        public IParam Param { get; set; }

        public ParamType Type
        {
            get { return Param.TypeKey; }
            set { }
        }
        
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
                AddToChildren(node.Value, node.Key);
            UpdateChildrenIndeces();
        }

        public void Add(IParam param, ulong hash40)
        {
            Param.Nodes.Add(hash40, param);
            AddToChildren(param, hash40);
        }

        private void AddToChildren(IParam param, ulong hash40)
        {
            switch (param.TypeKey)
            {
                case ParamType.@struct:
                    Children.Add(new VM_StructStruct(param as ParamStruct, this, hash40));
                    break;
                case ParamType.list:
                    Children.Add(new VM_StructList(param as ParamList, this, hash40));
                    break;
                default:
                    Children.Add(new VM_StructValue(param as ParamValue, this, hash40));
                    break;
            }
        }

        public void RemoveAt(int index)
        {
            Param.Nodes.RemoveAt(index);
            Children.RemoveAt(index);
            UpdateChildrenIndeces();
        }

        public void UpdateChildrenIndeces()
        {
            for (int i = 0; i < Children.Count; i++)
                Children[i].Index = i;
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
                AddToChildren(node);
            UpdateChildrenIndeces();
        }

        public void Add(IParam param)
        {
            Param.Nodes.Add(param);
            AddToChildren(param);
        }

        private void AddToChildren(IParam param)
        {
            switch (param.TypeKey)
            {
                case ParamType.@struct:
                    Children.Add(new VM_ListStruct(param as ParamStruct, this));
                    break;
                case ParamType.list:
                    Children.Add(new VM_ListList(param as ParamList, this));
                    break;
                default:
                    Children.Add(new VM_ListValue(param as ParamValue, this));
                    break;
            }
        }

        public void RemoveAt(int index)
        {
            Param.Nodes.RemoveAt(index);
            Children.RemoveAt(index);
            UpdateChildrenIndeces();
        }

        public void UpdateChildrenIndeces()
        {
            for (int i = 0; i < Children.Count; i++)
                Children[i].Index = i;
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

        public string Value
        {
            get { return Param.ToString(MainWindow.HashToStringLabels); }
            set { Param.SetValue(value, MainWindow.StringToHashLabels); }
        }

        public VM_ParamValue(ParamValue param)
        {
            Param = param;
        }
    }

    #endregion

    #region PARAM_CHILD_INTERFACES

    /// <summary>
    /// Represents a generic child of a struct
    /// </summary>
    public interface IStructChild
    {
        IParam Param { get; set; }
        ParamType Type { get; set; }
        ulong Hash40 { get; set; }
        VM_ParamStruct Parent { get; set; }
        int Index { get; set; }
        int Index_EventCaller { get; set; }
    }

    /// <summary>
    /// Represents a generic child of a list
    /// </summary>
    public interface IListChild
    {
        IParam Param { get; set; }
        VM_ParamList Parent { get; set; }
        int Index { get; set; }
        int Index_EventCaller { get; set; }
    }

    #endregion

    #region CLIPBOARD_COMPATIBLE

    [Serializable]
    public class SerializableParam
    {
        public IParam Param { get; set; }

        public SerializableParam(IParam param)
        {
            Param = param;
        }
    }

    [Serializable]
    public class SerializableStructChild : SerializableParam
    {
        public ulong Hash40 { get; set; }

        public SerializableStructChild(IParam param, ulong hash40) : base(param)
        {
            Hash40 = hash40;
        }
    }

    #endregion

    #region IMPLEMENTATIONS

    //param with no parents (must be a struct)
    public class VM_ParamRoot : VM_ParamStruct
    {
        public override string Name => Param.TypeKey.ToString();

        public VM_ParamRoot(ParamStruct struc) : base(struc) { }
    }

    //params with a struct parent
    public class VM_StructStruct : VM_ParamStruct, IStructChild
    {
        public VM_ParamStruct Parent { get; set; }

        public int Index { get; set; }
        public int Index_EventCaller
        {
            get { return Index; }
            set
            {
                if (value == Index)
                    return;
                Util.ChangeStructChildIndex(this, value);
            }
        }

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

        public int Index { get; set; }
        public int Index_EventCaller
        {
            get { return Index; }
            set
            {
                if (value == Index)
                    return;
                Util.ChangeStructChildIndex(this, value);
            }
        }

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

        public int Index { get; set; }
        public int Index_EventCaller
        {
            get { return Index; }
            set
            {
                if (value == Index)
                    return;
                Util.ChangeStructChildIndex(this, value);
            }
        }

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

    //params with a list parent
    public class VM_ListStruct : VM_ParamStruct, IListChild
    {
        public VM_ParamList Parent { get; set; }
        public int Index { get; set; }
        public int Index_EventCaller
        {
            get { return Index; }
            set
            {
                if (Index == value)
                    return;
                Util.ChangeListChildIndex(this, value);
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public override string Name => Util.GetListChildName(Param, Index_EventCaller);

        public VM_ListStruct(ParamStruct struc, VM_ParamList parent) : base(struc)
        {
            Parent = parent;
        }
    }

    public class VM_ListList : VM_ParamList, IListChild
    {
        public VM_ParamList Parent { get; set; }
        public int Index { get; set; }
        public int Index_EventCaller
        {
            get { return Index; }
            set
            {
                if (Index == value)
                    return;
                Util.ChangeListChildIndex(this, value);
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public override string Name => Util.GetListChildName(Param, Index_EventCaller);

        public VM_ListList(ParamList list, VM_ParamList parent) : base(list)
        {
            Parent = parent;
        }
    }

    public class VM_ListValue : VM_ParamValue, IListChild
    {
        public VM_ParamList Parent { get; set; }
        public int Index { get; set; }
        public int Index_EventCaller
        {
            get { return Index; }
            set
            {
                if (Index == value)
                    return;
                Util.ChangeListChildIndex(this, value);
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public override string Name => Util.GetListChildName(Param, Index_EventCaller);

        public VM_ListValue(ParamValue value, VM_ParamList parent) : base(value)
        {
            Parent = parent;
        }
    }

    #endregion
}
