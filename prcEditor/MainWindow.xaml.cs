using Microsoft.Win32;
using paracobNET;
using prcEditor.Controls;
using prcEditor.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
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
        internal ParamFile PFile { get; set; }

        internal WorkQueue WorkerQueue { get; set; }

        internal static TimedMessage Timer { get; set; }

        internal static bool KeyCtrl { get; set; }

        public static OrderedDictionary<ulong, string> HashToStringLabels { get; set; }
        public static OrderedDictionary<string, ulong> StringToHashLabels { get; set; }

        private string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;
        private IDictionary AppProperties => Application.Current.Properties;
        private string LabelPath => Path.Combine(BaseDirectory, "ParamLabels.csv");
        private const string ParamFilter = "Param files|*.prc;*.stdat;*.stprm|All files|*.*";

        #region PROPERTY_BINDING

        /// <summary>
        /// The body of the app, which can either be a blank background with an image, or the param data
        /// </summary>
        private UserControl body;
        public UserControl Body
        {
            get => body;
            set
            {
                body = value;
                NotifyPropertyChanged(nameof(Body));
            }
        }

        public static IEnumerable<string> StringLabels
        {
            get { return StringToHashLabels.Keys; }
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

        private bool isLabelDownloadEnabled = true;
        public bool IsLabelDownloadEnabled
        {
            get { return isLabelDownloadEnabled; }
            set
            {
                isLabelDownloadEnabled = value;
                NotifyPropertyChanged(nameof(IsLabelDownloadEnabled));
            }
        }

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

            Body = new BlankBody();

            BodyContent.DataContext = this;
            StatusTB.DataContext = this;
            OpenFileButton.DataContext = this;
            SaveFileButton.DataContext = this;

            EditLabelButton.DataContext = this;
            SaveLabelButton.DataContext = this;
            DownloadLabelButton.DataContext = this;

            KeyCtrl = false;
        }

        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void OpenFileDialog()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = ParamFilter;

            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                OpenFile(ofd.FileName);
            }
        }

        private void SaveFileDialog()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = ParamFilter;

            bool? result = sfd.ShowDialog();
            if (result == true)
            {
                SaveFile(sfd.FileName);
            }
        }

        private void OpenFile(string file)
        {
            IsOpenEnabled = false;
            IsSaveEnabled = false;

            WorkerQueue.Enqueue(new EnqueuableStatus(() =>
            {
                PFile = new ParamFile();
                try
                {
                    PFile.Open(file);
                    // I don't know why this is supposed to work tbh
                    Dispatcher.BeginInvoke(new Action(() => Body = new ParamControl(PFile.Root)));
                }
                catch (InvalidHeaderException ex)
                {
                    Timer.SetMessage(ex.Message, 5000);
                }
                IsOpenEnabled = true;
                IsSaveEnabled = true;
            }, "Loading param file"));
        }

        private void SaveFile(string file)
        {
            IsOpenEnabled = false;
            IsSaveEnabled = false;

            WorkerQueue.Enqueue(new EnqueuableStatus(() =>
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                PFile.Save(file);
                IsOpenEnabled = true;
                IsSaveEnabled = true;
            }, "Saving param file"));
        }

        private void OpenLabels()
        {
            WorkerQueue.Enqueue(new EnqueuableStatus(() =>
            {
                string name = LabelPath;
                if (File.Exists(name))
                {
                    IsOpenEnabled = false;
                    IsLabelSaveEnabled = false;
                    IsLabelEditEnabled = false;
                    IsLabelDownloadEnabled = false;

                    HashToStringLabels = LabelIO.GetHashStringDict(name);
                    StringToHashLabels = LabelIO.GetStringHashDict(name);
                }
                IsOpenEnabled = true;
                IsLabelSaveEnabled = true;
                IsLabelEditEnabled = true;
                IsLabelDownloadEnabled = true;
                if (body is ParamControl pc)
                {
                    pc.ParamViewModel.UpdateHashes();
                }
            }, "Loading label dictionaries"));
        }

        private void SaveLabels()
        {
            LabelIO.WriteLabels(LabelPath, HashToStringLabels);
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
            OpenLabels();
            if (AppProperties.Contains("OnStartupFile"))
            {
                OpenFile((string)AppProperties["OnStartupFile"]);
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog();
        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog();
        }

        private void EditLabelButton_Click(object sender, RoutedEventArgs e)
        {
            LabelEditor editor = new LabelEditor(false);
            editor.ShowDialog();
            if (body is ParamControl pc)
            {
                pc.ParamViewModel.UpdateHashes();
            }
        }

        private void SaveLabelButton_Click(object sender, RoutedEventArgs e)
        {
            SaveLabels();
        }

        #endregion

        private void Window_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyUp(Key.LeftCtrl) && e.KeyboardDevice.IsKeyUp(Key.RightCtrl))
                KeyCtrl = false;
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                KeyCtrl = true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.O:
                    if (!KeyCtrl || !IsOpenEnabled)
                        return;
                    OpenFileDialog();
                    break;
                case Key.S:
                    if (!KeyCtrl || !IsSaveEnabled)
                        return;
                    SaveFileDialog();
                    break;
                default:
                    return;
            }

            e.Handled = true;
        }

        private void DownloadLabelButton_Click(object sender, RoutedEventArgs e)
        {
            IsOpenEnabled = false;
            IsLabelEditEnabled = false;
            IsLabelSaveEnabled = false;
            IsLabelDownloadEnabled = false;
            string ParamLabels = "https://github.com/ultimate-research/param-labels/raw/master/ParamLabels.csv";
            using (WebClient wc = new WebClient())
            {
                WorkerQueue.Enqueue(new EnqueuableStatus(() =>
                {
                    try
                    {
                        wc.DownloadFile(ParamLabels, LabelPath);
                        OpenLabels();
                    }
                    catch (Exception e)
                    {
                        Timer.SetMessage(e.Message, 5000);
                        IsOpenEnabled = true;
                        IsLabelSaveEnabled = true;
                        IsLabelEditEnabled = true;
                        IsLabelDownloadEnabled = true;
                    }
                }, "Downloading labels from source"));
            }
        }
    }
}
