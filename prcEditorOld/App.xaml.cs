using System;
using System.Windows;
using System.IO;

namespace prcEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionHandler);
        }

        static void ExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            using (StreamWriter writer = File.AppendText("log.txt"))
            {
                writer.WriteLine($"TIMESTAMP: {DateTime.Now.ToString()}");
                writer.WriteLine(e.ToString());
                writer.WriteLine("================================");
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var args = e.Args;
            if (args.Length > 0 && File.Exists(args[0]))
                Current.Properties["OnStartupFile"] = args[0];
            base.OnStartup(e);
        }
    }
}
