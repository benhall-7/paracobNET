using paracobNET;
using System.Collections.ObjectModel;

namespace prcEditor.ViewModel
{
    /// <summary>
    /// Represents a generic struct
    /// </summary>
    public abstract class VM_ParamStruct : VM_Param
    {
        // PROPERTIES
        public new ParamStruct Param
        {
            get => (ParamStruct)base.Param;
            set => base.Param = value;
        }

        public ObservableCollection<IStructChild> Children { get; set; }

        // CONSTRUCTOR
        public VM_ParamStruct(ParamStruct param)
        {
            Param = param;
            Children = new ObservableCollection<IStructChild>();
            foreach (var node in param.Nodes)
                AddToChildren(node.Value, node.Key);
            UpdateChildrenIndeces();
        }

        // PRIVATE METHODS
        private void AddToChildren(IParam param, ulong hash40)
        {
            switch (param.TypeKey)
            {
                case ParamType.@struct:
                    Children.Add(new Struct(param as ParamStruct, this));
                    break;
                case ParamType.list:
                    Children.Add(new List(param as ParamList, this));
                    break;
                default:
                    Children.Add(new Value(param as ParamValue, this));
                    break;
            }
        }

        // PUBLIC METHODS
        public void Add(IParam param, ulong hash40)
        {
            Param.Nodes.Add(hash40, param);
            AddToChildren(param, hash40);
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
                ((IChild)Children[i])._index = i;
        }

        public virtual void UpdateHashes()
        {
            foreach (var child in Children)
            {

            }
        }

        // INTERFACES
        private interface IChild : IStructChild
        {
            int _index { get; set; }
        }

        // SUB-CLASSES
        public class Struct : VM_ParamStruct, IChild
        {
            public VM_ParamStruct Parent { get; set; }

            // EXPLANATION:
            // __index:
            //     is totally private, not part of any interface. It's just a backing field
            // _index:
            //     is exposed to the parent, but not outside because it's part of the private
            //     interface "IChild". This lets the parent change index without invoking Index.
            //     changes to this value notify the UI of a change to the Index value.
            // Index:
            //     is publically exposed and editable, invokes call to the parent to change
            //     index of children.
            int __index;
            int IChild._index
            {
                get => __index;
                set
                {
                    if (value == __index)
                        return;
                    __index = value;
                    NotifyPropertyChanged(nameof(Index));
                }
            }
            public int Index
            {
                get { return ((IChild)this)._index; }
                set
                {
                    var old = Index;
                    if (value == old)
                        return;
                    Parent.Children.Move(old, value);
                    Parent.Param.Nodes.Move(old, value);
                    Parent.UpdateChildrenIndeces();
                }
            }

            public ulong Hash40
            {
                get { return Parent.Param.Nodes[Index].Key; }
                set
                {
                    if (Parent.Param.Nodes[Index].Key == value)
                        return;
                    Parent.Param.Nodes[Index].Key = value;
                    NotifyPropertyChanged(nameof(Key));
                    NotifyPropertyChanged(nameof(Name));
                }
            }

            public string Key
            {
                get { return Hash40Util.FormatToString(Hash40, MainWindow.HashToStringLabels); }
                set
                {
                    try { Hash40 = Hash40Util.LabelToHash40(value, MainWindow.StringToHashLabels); }
                    catch (InvalidLabelException e)
                    {
                        LabelEditor editor = new LabelEditor(e.Label);
                        bool? fix = editor.ShowDialog();
                        if (fix == true)
                            Hash40 = editor.CurrentHash;
                    }
                }
            }

            public override string Name => Util.GetStructChildName(this);

            public Struct(ParamStruct struc, VM_ParamStruct parent) : base(struc)
            {
                Parent = parent;
            }

            public override void UpdateHashes()
            {
                NotifyPropertyChanged(nameof(Key));
                NotifyPropertyChanged(nameof(Name));
                base.UpdateHashes();
            }
        }

        public class List : VM_ParamList, IChild
        {
            public VM_ParamStruct Parent { get; set; }

            int __index;
            int IChild._index
            {
                get => __index;
                set
                {
                    if (value == __index)
                        return;
                    __index = value;
                    NotifyPropertyChanged(nameof(Index));
                }
            }
            public int Index
            {
                get { return ((IChild)this)._index; }
                set
                {
                    var old = Index;
                    if (value == old)
                        return;
                    Parent.Children.Move(old, value);
                    Parent.Param.Nodes.Move(old, value);
                    Parent.UpdateChildrenIndeces();
                }
            }

            public ulong Hash40
            {
                get { return Parent.Param.Nodes[Index].Key; }
                set
                {
                    if (Parent.Param.Nodes[Index].Key == value)
                        return;
                    Parent.Param.Nodes[Index].Key = value;
                    NotifyPropertyChanged(nameof(Key));
                    NotifyPropertyChanged(nameof(Name));
                }
            }

            public string Key
            {
                get { return Hash40Util.FormatToString(Hash40, MainWindow.HashToStringLabels); }
                set
                {
                    try { Hash40 = Hash40Util.LabelToHash40(value, MainWindow.StringToHashLabels); }
                    catch (InvalidLabelException e)
                    {
                        LabelEditor editor = new LabelEditor(e.Label);
                        bool? fix = editor.ShowDialog();
                        if (fix == true)
                            Hash40 = editor.CurrentHash;
                    }
                }
            }

            public override string Name => Util.GetStructChildName(this);

            public List(ParamList list, VM_ParamStruct parent) : base(list)
            {
                Parent = parent;
            }

            public override void UpdateHashes()
            {
                NotifyPropertyChanged(nameof(Key));
                NotifyPropertyChanged(nameof(Name));
                base.UpdateHashes();
            }
        }

        public class Value : VM_ParamValue, IChild
        {
            public VM_ParamStruct Parent { get; set; }

            int __index;
            int IChild._index
            {
                get => __index;
                set
                {
                    if (value == __index)
                        return;
                    __index = value;
                    NotifyPropertyChanged(nameof(Index));
                }
            }
            public int Index
            {
                get { return ((IChild)this)._index; }
                set
                {
                    var old = Index;
                    if (value == old)
                        return;
                    Parent.Children.Move(old, value);
                    Parent.Param.Nodes.Move(old, value);
                    Parent.UpdateChildrenIndeces();
                }
            }

            public ulong Hash40
            {
                get { return Parent.Param.Nodes[Index].Key; }
                set
                {
                    if (Parent.Param.Nodes[Index].Key == value)
                        return;
                    Parent.Param.Nodes[Index].Key = value;
                    NotifyPropertyChanged(nameof(Key));
                    NotifyPropertyChanged(nameof(Name));
                }
            }

            public string Key
            {
                get { return Hash40Util.FormatToString(Hash40, MainWindow.HashToStringLabels); }
                set
                {
                    try { Hash40 = Hash40Util.LabelToHash40(value, MainWindow.StringToHashLabels); }
                    catch (InvalidLabelException e)
                    {
                        LabelEditor editor = new LabelEditor(e.Label);
                        bool? fix = editor.ShowDialog();
                        if (fix == true)
                            Hash40 = editor.CurrentHash;
                    }
                }
            }

            public override string Name => Util.GetStructChildName(this);

            public Value(ParamValue value, VM_ParamStruct parent) : base(value)
            {
                Parent = parent;
            }

            public override void UpdateHashes()
            {
                NotifyPropertyChanged(nameof(Key));
                NotifyPropertyChanged(nameof(Name));
                base.UpdateHashes();
            }
        }
    }
}
