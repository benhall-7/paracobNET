using paracobNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace prcEditor
{
    /// <summary>
    /// Interaction logic for Hash40Editor.xaml
    /// </summary>
    public partial class LabelEditor : Window, INotifyPropertyChanged
    {
        private string label { get; set; }
        public string Label
        {
            get { return label == null ? "" : label; }
            set
            {
                label = value;
                NotifyPropertyChanged(nameof(Label));
                if (autoCalcHash)
                    HashText = Hash40Util.FormatToString(AutoCalculatedHash);
                else if (MainWindow.StringToHashLabels.TryGetValue(value, out ulong hash))
                    HashText = Hash40Util.FormatToString(hash);
            }
        }

        private string hashText { get; set; }
        public string HashText
        {
            get { return hashText; }
            set
            {
                hashText = value;
                NotifyPropertyChanged(nameof(HashText));
            }
        }

        private bool autoCalcHash { get; set; }
        public bool AutoCalculateHash
        {
            get { return autoCalcHash; }
            set
            {
                autoCalcHash = value;
                NotifyPropertyChanged(nameof(AutoCalculateHash));
                if (value)
                    HashText = Hash40Util.FormatToString(AutoCalculatedHash);
            }
        }
        public ulong AutoCalculatedHash => Hash40Util.StringToHash40(Label);

        public event PropertyChangedEventHandler PropertyChanged;

        public LabelEditor()
        {
            InitializeComponent();

            Label_ComboBox.DataContext = this;
            Hash_TextBox.DataContext = this;
            AutoCalcHash_CheckBox.DataContext = this;
        }

        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AutoCalculateHash = !AutoCalculateHash;
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
