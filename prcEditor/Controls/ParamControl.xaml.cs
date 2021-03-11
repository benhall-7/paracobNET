using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using paracobNET;
using prcEditor.Controls;
using prcEditor.ViewModel;

namespace prcEditor.Windows
{
    /// <summary>
    /// Interaction logic for ParamControl.xaml
    /// </summary>
    public partial class ParamControl : UserControl, INotifyPropertyChanged
    {
        bool KeyCtrl
        {
            get => MainWindow.KeyCtrl;
            set => MainWindow.KeyCtrl = value;
        }
        TimedMessage Timer => MainWindow.Timer;
        OrderedDictionary<ulong, string> HashToStringLabels => MainWindow.HashToStringLabels;
        OrderedDictionary<string, ulong> StringToHashLabels => MainWindow.StringToHashLabels;

        public event PropertyChangedEventHandler PropertyChanged;

        public ParamControl(ParamStruct root)
        {
            InitializeComponent();
            DataGridControl = new KeyCommands();

            paramVM = new VM_ParamRoot(root);
            ParamDataBody.DataContext = this;
            Param_TreeView.DataContext = this;
            dataGridControl.DataContext = this;
        }

        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #region PROPERTIES

        private UserControl dataGridControl;
        public UserControl DataGridControl {
            get => dataGridControl;
            set {
                dataGridControl = value;
                NotifyPropertyChanged(nameof(DataGridControl));
            }
        }

        private VM_ParamRoot paramVM;
        public VM_ParamRoot ParamViewModel
        {
            get { return paramVM; }
            set
            {
                paramVM = value;
                DataGridControl = new KeyCommands();
                NotifyPropertyChanged(nameof(DataGridControl));
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

        #endregion

        //=========== TODO ===========
        // These would greatly benefit
        // from a functional approach
        // and some extra interfaces.
        private void OpenParamNode(TreeViewItem tvi)
        {
            if (tvi.Header is VM_ParamStruct str)
            {
                if (DataGridControl is ParamStruct_DataGrid dg)
                {
                    dg.Struct_DataGrid_Source = str.Children;
                }
                else
                {
                    DataGridControl = new ParamStruct_DataGrid(str.Children);
                }
            }
            else if (tvi.Header is VM_ParamList list)
            {
                if (DataGridControl is ParamList_DataGrid dg)
                {
                    dg.List_DataGrid_Source = list.Children;
                }
                else
                {
                    DataGridControl = new ParamList_DataGrid(list.Children);
                }
            }
            else if (tvi.Header is IStructChild strc)
            {
                if (DataGridControl is ParamStruct_DataGrid dg)
                {
                    dg.Struct_DataGrid_Source = strc.Parent.Children;
                }
                else
                {
                    DataGridControl = new ParamStruct_DataGrid(strc.Parent.Children);
                }
                var actual_dg = ((ParamStruct_DataGrid)DataGridControl).Struct_DataGrid;
                actual_dg.SelectedItem = strc;
                actual_dg.ScrollIntoView(strc);
            }
            else if (tvi.Header is IListChild listc)
            {
                if (DataGridControl is ParamList_DataGrid dg)
                {
                    dg.List_DataGrid_Source = listc.Parent.Children;
                }
                else
                {
                    DataGridControl = new ParamList_DataGrid(listc.Parent.Children);
                }
                var actual_dg = ((ParamList_DataGrid)DataGridControl).List_DataGrid;
                actual_dg.SelectedItem = listc;
                actual_dg.ScrollIntoView(listc);
            }
        }

        private void DeleteParamNode(TreeViewItem tvi)
        {
            if (tvi.Header is IStructChild structChild)
                structChild.Parent.RemoveAt(structChild.Index);
            else if (tvi.Header is IListChild listChild)
                listChild.Parent.RemoveAt(listChild.Index);
        }

        private void CopyParamNode(TreeViewItem tvi)
        {
            if (!KeyCtrl) return;

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

        private void CutParamNode(TreeViewItem tvi)
        {
            if (!KeyCtrl) return;

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

        private void PasteParamNode(TreeViewItem tvi)
        {
            if (!KeyCtrl) return;

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
                            paramVM.UpdateHashes();
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

        private void PasteParamNodeParent(TreeViewItem tvi)
        {
            if (!KeyCtrl) return;

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
                            paramVM.UpdateHashes();
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

        #region EVENT_HANDLERS

        private void Param_TreeView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                KeyCtrl = true;
        }

        private void Param_TreeView_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyUp(Key.LeftCtrl) && e.KeyboardDevice.IsKeyUp(Key.RightCtrl))
                KeyCtrl = false;
        }

        private void TreeViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is TreeViewItem tvi)) return;

            switch (e.Key)
            {
                case Key.Enter:
                    OpenParamNode(tvi);
                    break;
                case Key.Delete:
                    DeleteParamNode(tvi);
                    break;
                case Key.C:
                    CopyParamNode(tvi);
                    break;
                case Key.X:
                    CutParamNode(tvi);
                    break;
                case Key.V:
                    PasteParamNode(tvi);
                    break;
                case Key.P:
                    PasteParamNodeParent(tvi);
                    break;
                case Key.D:
                    CopyParamNode(tvi);
                    PasteParamNodeParent(tvi);
                    break;
            }

            //TODO: Can this interfere with "global" key shortcuts?
            e.Handled = true;//bubbling event, don't send the event upward
        }

        #endregion
    }
}
