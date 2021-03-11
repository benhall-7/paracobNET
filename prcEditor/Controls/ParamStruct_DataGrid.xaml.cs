using prcEditor.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace prcEditor.Windows
{
    /// <summary>
    /// Interaction logic for ParamStruct_DataGrid.xaml
    /// </summary>
    public partial class ParamStruct_DataGrid : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public ParamStruct_DataGrid(ObservableCollection<IStructChild> children)
        {
            InitializeComponent();
            Struct_DataGrid.DataContext = this;
            _struct_source = children;
        }

        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #region PROPERTIES

        private ObservableCollection<IStructChild> _struct_source;
        public ObservableCollection<IStructChild> Struct_DataGrid_Source
        {
            get { return _struct_source; }
            set
            {
                _struct_source = value;
                NotifyPropertyChanged(nameof(Struct_DataGrid_Source));
            }
        }

        #endregion
    }
}
