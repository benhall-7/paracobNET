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
using System.Globalization;

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
                if (MainWindow.StringToHashLabels.TryGetValue(Label, out ulong hash))
                    HashText = Hash40Util.FormatToString(hash);
                else if (autoCalcHash)
                    HashText = Hash40Util.FormatToString(AutoCalculatedHash);
                NotifyPropertyChanged(nameof(CanAddLabel));
            }
        }

        private string hashText { get; set; }
        public string HashText
        {
            get { return hashText == null ? "" : hashText; }
            set
            {
                hashText = value;
                NotifyPropertyChanged(nameof(HashText));
                NotifyPropertyChanged(nameof(CanAddLabel));
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

        public ulong CurrentHash { get; set; }

        public bool IsCurrentHashTextValid
        {
            get
            {
                if (TryParseHash(HashText, out ulong hash))
                {
                    CurrentHash = hash;
                    return true;
                }
                return false;
            }
        }

        public bool CanAddLabel
        {
            get
            {
                if (!IsCurrentHashTextValid
                    || MainWindow.StringToHashLabels.ContainsKey(Label)
                    || MainWindow.HashToStringLabels.ContainsKey(CurrentHash))
                {
                    return false;
                }
                return true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public LabelEditor()
        {
            InitializeComponent();

            Label_ComboBox.DataContext = this;
            Hash_TextBox.DataContext = this;
            AutoCalcHash_CheckBox.DataContext = this;

            Add_Button.DataContext = this;
            Delete_Button.DataContext = this;
            
        }

        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private bool TryParseHash(string hashText, out ulong hash)
        {
            hash = 0;
            if (!hashText.StartsWith("0x"))
                return false;
            try
            {
                hash = ulong.Parse(hashText.Substring(2), NumberStyles.HexNumber);
                return true;
            }
            catch { return false; }
        }

        private void Hash_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            hashText = tb.Text;
            NotifyPropertyChanged(nameof(CanAddLabel));
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
