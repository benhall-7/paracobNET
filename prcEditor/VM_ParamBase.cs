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
    //VM = "View Model"
    public abstract class VM_ParamBase
    {
        public IParam Param { get; set; }
        public VM_ParamBase Parent { get; set; }

        public VM_ParamBase(IParam param, VM_ParamBase parent)
        {
            Param = param;
            Parent = parent;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName]string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }
    }
}
