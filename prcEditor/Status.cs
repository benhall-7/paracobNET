using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prcEditor
{
    public class Status : INotifyPropertyChanged
    {

        private void NotifyChange(params object[] properties)
        {
            if (PropertyChanged != null)
            {
                foreach (string p in properties)
                    PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
