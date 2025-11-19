using prcEditor.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace prcEditor.Windows
{
    /// <summary>
    /// Interaction logic for ParamList_DataGrid.xaml
    /// </summary>
    public partial class ParamList_DataGrid : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ParamList_DataGrid(ObservableCollection<IListChild> children)
        {
            InitializeComponent();
            List_DataGrid.DataContext = this;
            _list_source = children;
        }

        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #region PROPERTIES

        private ObservableCollection<IListChild> _list_source;
        public ObservableCollection<IListChild> List_DataGrid_Source
        {
            get { return _list_source; }
            set
            {
                _list_source = value;
                NotifyPropertyChanged(nameof(List_DataGrid_Source));
            }
        }

        #endregion
    }
}
