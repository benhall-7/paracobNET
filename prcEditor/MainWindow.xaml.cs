using Microsoft.Win32;
using paracobNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace prcEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        ParamFile PFile { get; set; }
        ParamTreeItem PTreeRoot { get; set; }
        
        Queue<EnqueuableStatus> WorkerQueue { get; set; } 
        public readonly object WorkerThreadLock = new object();

        IParam CopiedParam { get; set; }

        //Thanks to Greg Sansom: https://stackoverflow.com/a/5507826
        private string statusMessage = "Idle";
        public string StatusMessage
        {
            get { return statusMessage; }
            set
            {
                statusMessage = value;
                RaisePropertyChanged(nameof(StatusMessage));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

            WorkerQueue = new Queue<EnqueuableStatus>();
            StatusTB.DataContext = this;
        }

        private void ExecuteWorkerQueue()
        {
            Thread thread = new Thread(() =>
            {
                EnqueuableStatus status;
                while (WorkerQueue.Count > 0)
                {
                    status = WorkerQueue.Dequeue();

                    StatusMessage = status.Message;
                    status.Action.Invoke();
                }

                StatusMessage = "Idle";
            });
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private void SetupDataGrid(ParamTreeItem ptItem)
        {
            IParam param = ptItem.Param;
            if (param is ParamStruct paramStruct)
            {
                var entries = new List<ParamStructEntry>();
                foreach (var node in paramStruct.Nodes)
                {
                    if (node.Value is ParamValue pValue)
                        entries.Add(new ParamStructEntry(node.Key, pValue));
                }
                ParamData.ItemsSource = entries;
            }
            else if (param is ParamList paramList)
            {
                var entries = new List<ParamListEntry>();
                for (int i = 0; i < paramList.Nodes.Count; i++)
                {
                    var node = paramList.Nodes[i];
                    if (node is ParamValue pValue)
                        entries.Add(new ParamListEntry(i, pValue));
                }
                ParamData.ItemsSource = entries;
            }
        }

        #region EVENT_HANDLERS

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            string autoLoadName = "ParamLabels.csv";
            if (!LabelsLoaded && File.Exists(autoLoadName))
            {
                WorkerQueue.Enqueue(new EnqueuableStatus(() =>
                {
                    HashToStringLabels = LabelIO.GetHashStringDict(autoLoadName);
                    StringToHashLabels = LabelIO.GetStringHashDict(autoLoadName);
                }, "Loading label dictionaries"));
                ExecuteWorkerQueue();
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Param files|*.prc;*.stdat;*.stprm|All files|*.*";

            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                ParamData.ItemsSource = null;
                ParamTV.Items.Clear();
                OpenFileButton.IsEnabled = false;
                SaveFileButton.IsEnabled = false;

                WorkerQueue.Enqueue(new EnqueuableStatus(() =>
                {
                    PFile = new ParamFile(ofd.FileName);
                }, "Loading param file"));
                WorkerQueue.Enqueue(new EnqueuableStatus(() =>
                {
                    PTreeRoot = new ParamTreeItem(PFile.Root, null);
                    PTreeRoot.IsExpanded = true;
                    Application.Current.Dispatcher.Invoke(() => ParamTV.Items.Add(PTreeRoot));
                }, "Setting up treeview"));
                ExecuteWorkerQueue();

                OpenFileButton.IsEnabled = true;
                SaveFileButton.IsEnabled = true;
            }
        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Param files|*.prc;*.stdat;*.stprm|All files|*.*";

            bool? result = sfd.ShowDialog();
            if (result == true)
            {
                WorkerQueue.Enqueue(new EnqueuableStatus(() =>
                {
                    PFile.Save(sfd.FileName);
                }, "Saving param file"));
                ExecuteWorkerQueue();
            }
        }

        private void ParamTV_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.OriginalSource is ParamTreeItem ptItem))
                return;

            switch (e.Key)
            {
                case Key.Enter:
                    SetupDataGrid(ptItem);
                    break;
                case Key.C:
                    //copies param into new entry
                    break;
                case Key.Delete:
                    if (ptItem.Parent == null)
                        return;
                    IParam parentParam = ptItem.Parent.Param;
                    if (parentParam is ParamStruct parentStruct)
                    {
                        ulong key = Hash40Util.LabelToHash40(ptItem.ParentAccessor, StringToHashLabels);
                        parentStruct.Nodes.Remove(key);
                    }
                    else
                    {
                        ParamList parentList = parentParam as ParamList;
                        parentList.Nodes.RemoveAt(int.Parse(ptItem.ParentAccessor));
                    }
                    ptItem.Parent.Items.Remove(ptItem);
                    ptItem.Param = null;
                    ptItem = null;
                    break;
            }
        }

        private void ParamData_AutoGeneratedColumns(object sender, EventArgs e)
        {
            //Thanks to Sylwester Santorowski: https://stackoverflow.com/a/49285981
            int i = ((DataGrid)sender).Columns.Count;
            DataGridColumn column = ((DataGrid)sender).Columns[i - 1];
            column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }

        #endregion
    }
}
