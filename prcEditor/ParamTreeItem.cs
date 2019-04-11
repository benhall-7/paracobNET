using paracobNET;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace prcEditor
{
    public class ParamTreeItem : INotifyPropertyChanged
    {
        public IParam Param { get; set; }
        public ParamTreeItem Parent { get; set; }
        public object ParentAccessor { get; set; }
        public ObservableCollection<ParamTreeItem> Items { get; set; }

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
                        name += $" ({Hash40Util.FormatToString((ulong)ParentAccessor, MainWindow.HashToStringLabels)})";
                        break;
                    case ParamType.list:
                        name += $" ({((int)ParentAccessor).ToString()})";
                        break;
                }

                return name;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ParamTreeItem(IParam param, ParamTreeItem parent, object parentAccessor)
        {
            Param = param;
            Parent = parent;
            ParentAccessor = parentAccessor;

            switch (Param.TypeKey)
            {
                case ParamType.@struct:
                    {
                        ObservableCollection<ParamTreeItem> Items = new ObservableCollection<ParamTreeItem>();
                        foreach (var node in (Param as ParamStruct).Nodes)
                            Items.Add(new ParamTreeItem(node.Value, this, node.Key));
                        this.Items = Items;
                        break;
                    }
                case ParamType.list:
                    {
                        ObservableCollection<ParamTreeItem> Items = new ObservableCollection<ParamTreeItem>();
                        var list = (Param as ParamList).Nodes;
                        for (int i = 0; i < list.Count; i++)
                            Items.Add(new ParamTreeItem(list[i], this, i));
                        this.Items = Items;
                        break;
                    }
            }
        }

        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        /// <summary>
        /// Adds a new ParamTreeItem to the end of the observable collection of a parent.
        /// Only supported on list-types.
        /// </summary>
        public void Add(IParam child)
        {
            if (Param is ParamList list)
            {
                list.Nodes.Add(child);
                var node = new ParamTreeItem(child, this, list.Nodes.Count - 1);
                Items.Add(node);
            }
        }
        /// <summary>
        /// Removes a ParamTreeItem from the observable collection of its parent.
        /// If the item is a member of a list, notifies the children of the list to change their indeces.
        /// </summary>
        public void Remove()
        {
            if (Parent == null)
                return;
            //remove from class
            IParam parentParam = Parent.Param;
            if (parentParam is ParamStruct parentStruct)
                parentStruct.Nodes.Remove((ulong)ParentAccessor);
            else if (parentParam is ParamList parentList)
                parentList.Nodes.RemoveAt((int)ParentAccessor);
            //remove from this ViewModel
            Parent.Items.Remove(this);
            FixListAccessorOfSiblings();
            Param = null;
        }

        public void FixListAccessorOfSiblings()
        {
            if (Parent.Param is ParamList list)
            {
                int startIndex = (int)ParentAccessor;
                for (int i = startIndex; i < Parent.Items.Count; i++)
                {
                    var item = Parent.Items[i];
                    item.ParentAccessor = i;
                    item.NotifyPropertyChanged(nameof(Name));
                }
            }
        }
    }
}
