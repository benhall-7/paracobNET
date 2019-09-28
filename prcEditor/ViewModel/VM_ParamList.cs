using paracobNET;
using System.Collections.ObjectModel;

namespace prcEditor.ViewModel
{
    /// <summary>
    /// Represents a generic list
    /// </summary>
    public abstract class VM_ParamList : VM_Param
    {
        // PROPERTIES
        public new ParamList Param
        {
            get => (ParamList)base.Param;
            set => base.Param = value;
        }

        public ObservableCollection<IListChild> Children { get; set; }

        // CONSTRUCTOR
        public VM_ParamList(ParamList param)
        {
            Param = param;
            Children = new ObservableCollection<IListChild>();
            foreach (var node in param.Nodes)
                AddToChildren(node);
            UpdateChildrenIndeces();
        }

        // PRIVATE PROPERTIES
        private void AddToChildren(IParam param)
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

        // PUBLIC PROPERTIES
        public void Add(IParam param)
        {
            Param.Nodes.Add(param);
            AddToChildren(param);
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
            //foreach (var child in Children)
            //    child.UpdateHashes();
        }

        // INTERFACES
        private interface IChild : IListChild
        {
            int _index { get; set; }
        }

        // SUB-CLASSES
        public class Struct : VM_ParamStruct, IChild
        {
            public VM_ParamList Parent { get; set; }

            public int _index { get; set; }
            public int Index
            {
                get { return _index; }
                set
                {
                    if (value == _index)
                        return;
                    Parent.Children.Move(_index, value);
                    var item = Parent.Param.Nodes[_index];
                    Parent.Param.Nodes.RemoveAt(_index);
                    Parent.Param.Nodes.Insert(value, item);
                }
            }

            public override string Name => Util.GetListChildName(this);

            public Struct(ParamStruct struc, VM_ParamList parent) : base(struc)
            {
                Parent = parent;
            }

            public override void UpdateHashes()
            {
                NotifyPropertyChanged(nameof(Name));
                base.UpdateHashes();
            }
        }

        public class List : VM_ParamList, IChild
        {
            public VM_ParamList Parent { get; set; }

            public int _index { get; set; }
            public int Index
            {
                get { return _index; }
                set
                {
                    if (value == _index)
                        return;
                    Parent.Children.Move(_index, value);
                    var item = Parent.Param.Nodes[_index];
                    Parent.Param.Nodes.RemoveAt(_index);
                    Parent.Param.Nodes.Insert(value, item);
                }
            }

            public override string Name => Util.GetListChildName(this);

            public List(ParamList list, VM_ParamList parent) : base(list)
            {
                Parent = parent;
            }

            public override void UpdateHashes()
            {
                NotifyPropertyChanged(nameof(Name));
                base.UpdateHashes();
            }
        }

        public class Value : VM_ParamValue, IChild
        {
            public VM_ParamList Parent { get; set; }

            public int _index { get; set; }
            public int Index
            {
                get { return _index; }
                set
                {
                    if (value == _index)
                        return;
                    Parent.Children.Move(_index, value);
                    var item = Parent.Param.Nodes[_index];
                    Parent.Param.Nodes.RemoveAt(_index);
                    Parent.Param.Nodes.Insert(value, item);
                }
            }

            public override string Name => Util.GetListChildName(this);

            public Value(ParamValue value, VM_ParamList parent) : base(value)
            {
                Parent = parent;
            }

            public override void UpdateHashes()
            {
                NotifyPropertyChanged(nameof(Name));
                base.UpdateHashes();
            }
        }
    }
}
