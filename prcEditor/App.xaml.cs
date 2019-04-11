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
            using (StreamWriter writer = new StreamWriter(File.OpenWrite("log.txt")))
            {
                writer.WriteLine(e.Message);
                writer.WriteLine(e.StackTrace);
            }
        }
    }
}
