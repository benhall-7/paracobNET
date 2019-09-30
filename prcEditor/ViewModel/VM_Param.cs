using paracobNET;
using System.ComponentModel;

namespace prcEditor.ViewModel
{
    public abstract class VM_Param : INotifyPropertyChanged
    {
        public IParam Param { get; set; }

        public virtual ParamType Type
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
}
