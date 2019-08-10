using Microsoft.Win32;
using paracobNET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
        private ParamFile PFile { get; set; }

        private WorkQueue WorkerQueue { get; set; }

        private TimedMessage Timer { get; set; }

        private bool KeyCtrl { get; set; }
        private bool KeyShift { get; set; }

        public static OrderedDictionary<ulong, string> HashToStringLabels { get; set; }
        public static OrderedDictionary<string, ulong> StringToHashLabels { get; set; }

        #region PROPERTY_BINDING

        public static IEnumerable<string> StringLabels
        {
            get { return StringToHashLabels.Keys; }
        }

        private VM_ParamRoot paramVM;
        public VM_ParamRoot ParamViewModel
        {
            get { return paramVM; }
            set
            {
                paramVM = value;
                Struct_DataGrid_Source = null;
                List_DataGrid_Source = null;
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
        
        public string StatusMessage
        {
            get
            {
                if (!string.IsNullOrEmpty(WorkerThreadStatus))
                    return WorkerThreadStatus;
                if (!string.IsNullOrEmpty(TimedMessage))
                    return TimedMessage;
                return "Idle";
            }
        }

        private string workerThreadStatus;
        public string WorkerThreadStatus
        {
            get => workerThreadStatus;
            set
            {
                workerThreadStatus = value;
                NotifyPropertyChanged(nameof(StatusMessage));
            }
        }

        private string timedMessage;
        public string TimedMessage
        {
            get => timedMessage;
            set
            {
                timedMessage = value;
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

        private bool isLabelSaveEnabled = true;
        public bool IsLabelSaveEnabled
        {
            get { return isLabelSaveEnabled; }
            set
            {
                isLabelSaveEnabled = value;
                NotifyPropertyChanged(nameof(IsLabelSaveEnabled));
            }
        }

        private bool isLabelEditEnabled = true;
        public bool IsLabelEditEnabled
        {
            get { return isLabelEditEnabled; }
            set
            {
                isLabelEditEnabled = value;
                NotifyPropertyChanged(nameof(IsLabelEditEnabled));
            }
        }

        private ObservableCollection<IStructChild> _struct_source;
        public ObservableCollection<IStructChild> Struct_DataGrid_Source
        {
            get { return _struct_source; }
            set
            {
                _struct_source = value;
                NotifyPropertyChanged(nameof(Struct_DataGrid_Source));
                NotifyPropertyChanged(nameof(Struct_DataGrid_Visible));
            }
        }
        public Visibility Struct_DataGrid_Visible => Struct_DataGrid_Source == null ? Visibility.Hidden : Visibility.Visible;

        private ObservableCollection<IListChild> _list_source;
        public ObservableCollection<IListChild> List_DataGrid_Source
        {
            get { return _list_source; }
            set
            {
                _list_source = value;
                NotifyPropertyChanged(nameof(List_DataGrid_Source));
                NotifyPropertyChanged(nameof(List_DataGrid_Visible));
            }
        }
        public Visibility List_DataGrid_Visible => List_DataGrid_Source == null ? Visibility.Hidden : Visibility.Visible;

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        static MainWindow()
        {
            HashToStringLabels = new OrderedDictionary<ulong, string>();
            StringToHashLabels = new OrderedDictionary<string, ulong>();
        }

        public MainWindow()
        {
            InitializeComponent();

            WorkerQueue = new WorkQueue();
            WorkerQueue.RaiseMessageChangeEvent += WorkerStatusChangeEvent;

            Timer = new TimedMessage();
            Timer.RaiseMessageChangeEvent += TimerMessageChangeEvent;

            StatusTB.DataContext = this;
            OpenFileButton.DataContext = this;
            SaveFileButton.DataContext = this;

            SaveLabelButton.DataContext = this;
            EditLabelButton.DataContext = this;

            Param_TreeView.DataContext = this;
            ParamStruct_DataGrid.DataContext = this;
            ParamList_DataGrid.DataContext = this;

            KeyCtrl = false;
        }

        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void WorkerStatusChangeEvent(object sender, StatusChangeEventArgs e)
        {
            WorkerThreadStatus = e.Message;
        }

        private void TimerMessageChangeEvent(object sender, TimedMsgChangedEventArgs e)
        {
            TimedMessage = e.Message;
        }

        #region EVENT_HANDLERS

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            //load label dictionaries (and make it visible to user)
            string labelFileName = "ParamLabels.csv";
            WorkerQueue.Enqueue(new EnqueuableStatus(() =>
            {
                if (File.Exists(labelFileName))
                {
                    IsOpenEnabled = false;
                    IsLabelSaveEnabled = false;
                    IsLabelEditEnabled = false;

                    HashToStringLabels = LabelIO.GetHashStringDict(labelFileName);
                    StringToHashLabels = LabelIO.GetStringHashDict(labelFileName);

                    IsOpenEnabled = true;
                    IsLabelSaveEnabled = true;
                    IsLabelEditEnabled = true;
                }
            }, "Loading label dictionaries"));
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
                    PFile = new ParamFile();
                    try
                    {
                        PFile.Open(ofd.FileName);
                        ParamViewModel = new VM_ParamRoot(PFile.Root);
                    }
                    catch (InvalidHeaderException ex)
                    {
                        Timer.SetMessage(ex.Message, 5000);
                    }
                    IsOpenEnabled = true;
                    IsSaveEnabled = true;
                }, "Loading param file"));
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
                    Directory.CreateDirectory(Path.GetDirectoryName(sfd.FileName));
                    PFile.Save(sfd.FileName);
                    IsOpenEnabled = true;
                    IsSaveEnabled = true;
                }, "Saving param file"));
            }
        }

        private void EditLabelButton_Click(object sender, RoutedEventArgs e)
        {
            LabelEditor editor = new LabelEditor(false);
            editor.ShowDialog();
            paramVM?.UpdateHashes();
        }

        private void SaveLabelButton_Click(object sender, RoutedEventArgs e)
        {
            LabelIO.WriteLabels("ParamLabels.csv", HashToStringLabels);
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

        private void TreeViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is TreeViewItem tvi)) return;

            e.Handled = true;//bubbling event, don't send the event upward

            switch (e.Key)
            {
                case Key.Enter:
                    {
                        if (tvi.Header is VM_ParamStruct str)
                        {
                            Struct_DataGrid_Source = str.Children;
                            List_DataGrid_Source = null;
                        }
                        else if (tvi.Header is VM_ParamList list)
                        {
                            List_DataGrid_Source = list.Children;
                            Struct_DataGrid_Source = null;
                        }
                        else if (tvi.Header is IStructChild strc)
                        {
                            Struct_DataGrid_Source = strc.Parent.Children;
                            List_DataGrid_Source = null;

                            ParamStruct_DataGrid.SelectedItem = strc;
                            ParamStruct_DataGrid.ScrollIntoView(strc);
                        }
                        else if (tvi.Header is IListChild listc)
                        {
                            List_DataGrid_Source = listc.Parent.Children;
                            Struct_DataGrid_Source = null;

                            ParamList_DataGrid.SelectedItem = listc;
                            ParamList_DataGrid.ScrollIntoView(listc);
                        }
                    }
                    break;
                case Key.Delete:
                    {
                        if (tvi.Header is IStructChild structChild)
                            structChild.Parent.RemoveAt(structChild.Index);
                        else if (tvi.Header is IListChild listChild)
                            listChild.Parent.RemoveAt(listChild.Index);
                    }
                    break;
                case Key.C:
                    {
                        if (!KeyCtrl) break;

                        Clipboard.Clear();
                        if (tvi.Header is IStructChild structChild)
                        {
                            var data = new SerializableStructChild(structChild.Param.Clone(), structChild.Hash40);
                            Clipboard.SetDataObject(new DataObject(data), true);
                        }
                        else if (tvi.Header is IListChild listChild)
                        {
                            var data = new SerializableParam(listChild.Param.Clone());
                            Clipboard.SetDataObject(new DataObject(data), true);
                        }
                    }
                    break;
                case Key.X:
                    {
                        if (!KeyCtrl) break;

                        Clipboard.Clear();
                        if (tvi.Header is IStructChild structChild)
                        {
                            var data = new SerializableStructChild(structChild.Param.Clone(), structChild.Hash40);
                            Clipboard.SetDataObject(new DataObject(data), true);
                            structChild.Parent.RemoveAt(structChild.Index);
                        }
                        else if (tvi.Header is IListChild listChild)
                        {
                            var data = new SerializableParam(listChild.Param.Clone());
                            Clipboard.SetDataObject(new DataObject(data), true);
                            listChild.Parent.RemoveAt(listChild.Index);
                        }
                    }
                    break;
                case Key.V://paste into selected param
                    {
                        if (!KeyCtrl) break;

                        IDataObject dataObject = Clipboard.GetDataObject();
                        if (dataObject.GetDataPresent(typeof(SerializableStructChild)))
                        {
                            var data = (SerializableStructChild)dataObject.GetData(typeof(SerializableStructChild));

                            if (tvi.Header is VM_ParamStruct str)
                            {
                                try
                                {
                                    str.Add(data.Param, data.Hash40);
                                    str.UpdateChildrenIndeces();
                                }
                                catch (ArgumentException)
                                {
                                    LabelEditor le = new LabelEditor(true);
                                    bool? result = le.ShowDialog();
                                    if (result == true)
                                    {
                                        try
                                        {
                                            str.Add(data.Param, Hash40Util.LabelToHash40(le.Label, StringToHashLabels));
                                            str.UpdateChildrenIndeces();
                                        }
                                        catch
                                        {
                                            Timer.SetMessage("Paste operation failed (key " +
                                                $"'{le.Label}' " +
                                                $"already exists)", 3000);
                                        }
                                    }
                                    else
                                    {
                                        Timer.SetMessage("Paste operation failed (key " +
                                            $"'{Hash40Util.FormatToString(data.Hash40, HashToStringLabels)}' " +
                                            $"already exists)", 3000);
                                    }
                                }
                            }
                            else if (tvi.Header is VM_ParamList list)
                            {
                                list.Add(data.Param);
                                list.UpdateChildrenIndeces();
                            }
                        }
                        else if (dataObject.GetDataPresent(typeof(SerializableParam)))
                        {
                            var data = (SerializableParam)dataObject.GetData(typeof(SerializableParam));

                            if (tvi.Header is VM_ParamStruct str)
                            {
                                LabelEditor le = new LabelEditor(true);
                                if (le.ShowDialog() == true)
                                {
                                    try
                                    {
                                        str.Add(data.Param, StringToHashLabels[le.Label]);
                                        str.UpdateChildrenIndeces();
                                        str.UpdateHashes();
                                    }
                                    catch (ArgumentException)
                                    {
                                        Timer.SetMessage($"Paste operation failed (key '{le.Label}' already exists)", 3000);
                                    }
                                }
                            }
                            else if (tvi.Header is VM_ParamList list)
                            {
                                list.Add(data.Param);
                                list.UpdateChildrenIndeces();
                            }
                        }
                    }
                    break;
                case Key.P://paste into parent of selected param
                    {
                        if (!KeyCtrl) break;

                        IDataObject dataObject = Clipboard.GetDataObject();
                        if (dataObject.GetDataPresent(typeof(SerializableStructChild)))
                        {
                            var data = (SerializableStructChild)dataObject.GetData(typeof(SerializableStructChild));

                            if (tvi.Header is IStructChild structChild)
                            {
                                try
                                {
                                    structChild.Parent.Add(data.Param, data.Hash40);
                                    structChild.Parent.UpdateChildrenIndeces();
                                }
                                catch (ArgumentException)
                                {
                                    LabelEditor le = new LabelEditor(true);
                                    bool? result = le.ShowDialog();
                                    if (result == true)
                                    {
                                        try
                                        {
                                            structChild.Parent.Add(data.Param, Hash40Util.LabelToHash40(le.Label, StringToHashLabels));
                                            structChild.Parent.UpdateChildrenIndeces();
                                        }
                                        catch
                                        {
                                            Timer.SetMessage("Paste operation failed (key " +
                                                $"'{le.Label}' " +
                                                $"already exists)", 3000);
                                        }
                                    }
                                    else
                                    {
                                        Timer.SetMessage("Paste operation failed (key " +
                                            $"'{Hash40Util.FormatToString(data.Hash40, HashToStringLabels)}' " +
                                            $"already exists)", 3000);
                                    }
                                }
                            }
                            else if (tvi.Header is IListChild listChild)
                            {
                                listChild.Parent.Add(data.Param);
                                listChild.Parent.UpdateChildrenIndeces();
                            }
                        }
                        else if (dataObject.GetDataPresent(typeof(SerializableParam)))
                        {
                            var data = (SerializableParam)dataObject.GetData(typeof(SerializableParam));

                            if (tvi.Header is IStructChild structChild)
                            {
                                LabelEditor le = new LabelEditor(true);
                                if (le.ShowDialog() == true)
                                {
                                    try
                                    {
                                        structChild.Parent.Add(data.Param, StringToHashLabels[le.Label]);
                                        structChild.Parent.UpdateChildrenIndeces();
                                        structChild.Parent.UpdateHashes();
                                    }
                                    catch (ArgumentException)
                                    {
                                        Timer.SetMessage($"Paste operation failed (key '{le.Label}' already exists)", 3000);
                                    }
                                }
                            }
                            else if (tvi.Header is IListChild listChild)
                            {
                                listChild.Parent.Add(data.Param);
                                listChild.Parent.UpdateChildrenIndeces();
                            }
                        }
                    }
                    break;
            }
        }

        #endregion
    }
}
