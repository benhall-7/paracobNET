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
        static readonly string struct_child_type = "prc_struct_child";
        static readonly string list_child_type = "prc_list_child";

        private ParamFile pFile;
        ParamFile PFile
        {
            get { return pFile; }
            set
            {
                pFile = value;
                if (value == null)
                    ParamViewModel = null;
                else
                    ParamViewModel = new VM_ParamRoot(value.Root);
            }
        }

        Thread WorkerThread { get; set; }
        Queue<EnqueuableStatus> WorkerQueue { get; set; }
        readonly object WorkerThreadLock = new object();

        private bool KeyCtrl { get; set; }
        private bool KeyShift { get; set; }

        private static bool LabelsLoaded { get; set; }
        public static Dictionary<ulong, string> HashToStringLabels { get; set; }
        public static Dictionary<string, ulong> StringToHashLabels { get; set; }

        #region PROPERTY_BINDING

        private VM_ParamRoot paramVM;
        public VM_ParamRoot ParamViewModel
        {
            get { return paramVM; }
            set
            {
                paramVM = value;
                NotifyPropertyChanged(nameof(ParamViewModelList));
            }
        }
        public List<VM_ParamRoot> ParamViewModelList
        {
            get
            {
                if (ParamViewModel == null)
                    return new List<VM_ParamRoot>();
                return new List<VM_ParamRoot>() { ParamViewModel };
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
            Param_TreeView.DataContext = this;

            KeyCtrl = false;
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #region EVENT_HANDLERS

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            string autoLoadName = "ParamLabels.csv";
            if (!LabelsLoaded && File.Exists(autoLoadName))
            {
                IsOpenEnabled = false;

                WorkerQueue.Enqueue(new EnqueuableStatus(() =>
                {
                    HashToStringLabels = LabelIO.GetHashStringDict(autoLoadName);
                    StringToHashLabels = LabelIO.GetStringHashDict(autoLoadName);
                    LabelsLoaded = true;
                    IsOpenEnabled = true;
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
                //ParamData.ItemsSource = null;
                
                IsOpenEnabled = false;
                IsSaveEnabled = false;

                WorkerQueue.Enqueue(new EnqueuableStatus(() =>
                {
                    PFile = new ParamFile(ofd.FileName);
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
                IsOpenEnabled = false;
                IsSaveEnabled = false;

                WorkerQueue.Enqueue(new EnqueuableStatus(() =>
                {
                    PFile.Save(sfd.FileName);
                    IsOpenEnabled = true;
                    IsSaveEnabled = true;
                }, "Saving param file"));
                StartWorkerThread();
            }
        }

        private void ParamData_AutoGeneratedColumns(object sender, EventArgs e)
        {
            //Thanks to Sylwester Santorowski: https://stackoverflow.com/a/49285981
            int i = ((DataGrid)sender).Columns.Count;
            DataGridColumn column = ((DataGrid)sender).Columns[i - 1];
            column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }

        private void Param_TreeView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                KeyCtrl = true;

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                KeyShift = true;
        }

        private void Param_TreeView_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyUp(Key.LeftCtrl) && e.KeyboardDevice.IsKeyUp(Key.RightCtrl))
                KeyCtrl = false;

            if (e.KeyboardDevice.IsKeyUp(Key.LeftShift) && e.KeyboardDevice.IsKeyUp(Key.RightShift))
                KeyCtrl = false;
        }

        private void TreeViewItem_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.OriginalSource is TreeViewItem tvi)) return;
            
            switch (e.Key)
            {
                case Key.Enter:
                    //TO DO: re-add the data grid and such
                    break;
                case Key.Delete:
                    {
                        //if (e.OriginalSource is TreeViewItem tvi && tvi.Header is IStructChild child)
                        //    child.Parent.Children.Remove(child);
                    }
                    break;
                case Key.C:
                    if (!KeyCtrl) break;
                    if (tvi.Header is IStructChildBase structChild)
                        Clipboard.SetDataObject(new DataObject(struct_child_type, structChild), true);
                    else if (tvi.Header is IListChildBase listChild)
                        Clipboard.SetDataObject(new DataObject(list_child_type, listChild), true);
                    break;
                case Key.X:
                    if (!KeyCtrl) break;
                    Clipboard.SetDataObject(tvi.Header, true);
                    
                    break;
                case Key.V:
                    if (!KeyCtrl) break;
                    break;
            }
        }

        #endregion
    }
}
