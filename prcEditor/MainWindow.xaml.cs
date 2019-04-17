using Microsoft.Win32;
using paracobNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
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

        Thread WorkerThread { get; set; }
        Queue<EnqueuableStatus> WorkerQueue { get; set; }
        readonly object WorkerThreadLock = new object();

        IParam CopiedParam { get; set; }

        private static bool KeyCtrl { get; set; }

        private static bool LabelsLoaded { get; set; }
        public static Dictionary<ulong, string> HashToStringLabels { get; set; }
        public static Dictionary<string, ulong> StringToHashLabels { get; set; }

        #region Binded Properties

        private ParamTreeItem paramTI;
        public ParamTreeItem ParamTI
        {
            get { return paramTI; }
            set
            {
                paramTI = value;
                NotifyPropertyChanged(nameof(ParamTreeRootContainer));
            }
        }
        public List<ParamTreeItem> ParamTreeRootContainer
        {
            get
            {
                if (ParamTI == null)
                    return new List<ParamTreeItem>();
                return new List<ParamTreeItem>() { ParamTI };
            }
        }

        //Thanks to Greg Sansom: https://stackoverflow.com/a/5507826
        private string statusMessage = "Idle";
        public string StatusMessage
        {
            get { return statusMessage; }
            set
            {
                statusMessage = value;
                NotifyPropertyChanged(nameof(StatusMessage));
            }
        }

        private bool isOpenEnabled = true;
        public bool IsOpenEnabled
        {
            get { return isOpenEnabled; }
            set
            {
                isOpenEnabled = value;
                NotifyPropertyChanged(nameof(IsOpenEnabled));
            }
        }

        private bool isSaveEnabled = false;
        public bool IsSaveEnabled
        {
            get { return isSaveEnabled; }
            set
            {
                isSaveEnabled = value;
                NotifyPropertyChanged(nameof(IsSaveEnabled));
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        static MainWindow()
        {
            KeyCtrl = false;
            LabelsLoaded = false;
            HashToStringLabels = new Dictionary<ulong, string>();
            StringToHashLabels = new Dictionary<string, ulong>();
        }

        public MainWindow()
        {
            InitializeComponent();

            Thread.CurrentThread.Name = "Main";
            WorkerQueue = new Queue<EnqueuableStatus>();
            StatusTB.DataContext = this;
            OpenFileButton.DataContext = this;
            SaveFileButton.DataContext = this;
            ParamTV.DataContext = this;
        }

        private void StartWorkerThread()
        {
            WorkerThread = new Thread(() =>
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
            WorkerThread.Name = "Worker";
            WorkerThread.IsBackground = true;
            WorkerThread.SetApartmentState(ApartmentState.STA);
            WorkerThread.Start();
        }

        private void NotifyPropertyChanged(string propName)
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
                ParamData.Tag = ptItem;
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
                ParamData.Tag = ptItem;
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
                StartWorkerThread();
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
                
                IsOpenEnabled = false;
                IsSaveEnabled = false;

                WorkerQueue.Enqueue(new EnqueuableStatus(() =>
                {
                    PFile = new ParamFile(ofd.FileName);
                    ParamTI = new ParamTreeItem(PFile.Root, null, null);
                    IsOpenEnabled = true;
                    IsSaveEnabled = true;
                }, "Loading param file"));
                StartWorkerThread();
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
                StartWorkerThread();
            }
        }

        private void ParamTV_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                KeyCtrl = true;
                return;
            }

            if (!(e.OriginalSource is TreeViewItem tvi && tvi.Header is ParamTreeItem ptItem))
                return;

            if (!KeyCtrl)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        SetupDataGrid(ptItem);
                        break;
                    case Key.Delete:
                        if (ParamData.Tag as ParamTreeItem == ptItem)
                            ParamData.ItemsSource = null;
                        ptItem.Remove();
                        break;
                }
            }
            else //commands that require holding ctrl first
            {
                switch (e.Key)
                {
                    case Key.C:
                        CopiedParam = ptItem.Param.Clone();
                        break;
                    case Key.V:
                        if (CopiedParam != null)
                            ptItem.Add(CopiedParam);
                        break;
                }
            }
        }

        private void ParamTV_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyUp(Key.LeftCtrl) && e.KeyboardDevice.IsKeyUp(Key.RightCtrl))
                KeyCtrl = false;
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
