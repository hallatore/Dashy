using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace Dashy
{
    public partial class App : Application
    {
        public string SettingsPath { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            HandleExceptions();

            SettingsPath = e.Args.FirstOrDefault();

            if (string.IsNullOrEmpty(SettingsPath))
            {
                StartupUri = new Uri("SelectConfigWindow.xaml", UriKind.Relative);
            }
            else
            {
                StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
            }

            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void HandleExceptions()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject);

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception);

                if (e.Exception is not COMException)
                {
                    e.Handled = true;
                }
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception);

                if (e.Exception.InnerException is not COMException)
                {
                    e.SetObserved();
                }
            };
        }

        private void LogUnhandledException(Exception exception)
        {
            MessageBox.Show(exception.ToString(), exception.Message, MessageBoxButton.OK);
        }
    }
}