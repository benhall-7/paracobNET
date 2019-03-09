using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using paracobNET;

namespace prcEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ParamFile PFile { get; set; }

        static bool LabelsLoaded { get; set; }
        public static Dictionary<ulong, string> HashToStringLabels { get; set; }
        public static Dictionary<string, ulong> StringToHashLabels { get; set; }

        static MainWindow()
        {
            LabelsLoaded = false;
            HashToStringLabels = new Dictionary<ulong, string>();
            StringToHashLabels = new Dictionary<string, ulong>();
        }

        public MainWindow()
        {
            InitializeComponent();

            string autoLoadName = "ParamLabels.csv";
            if (!LabelsLoaded && File.Exists(autoLoadName))
            {
                HashToStringLabels = LabelIO.GetHashStringDict(autoLoadName);
                StringToHashLabels = LabelIO.GetStringHashDict(autoLoadName);
            }
        }

        private void SetupTreeView()
        {
            Param_TreeView.Items.Clear();
            ParamTreeItem root = new ParamTreeItem(PFile.Root, null);
            Param_TreeView.Items.Add(root);
            root.IsExpanded = true;
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "param files|*.prc;*.stdat;*.stprm";

            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                PFile = new ParamFile(ofd.FileName);
                SetupTreeView();
            }
        }
    }
}
