﻿using System;
using System.Linq;
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
            SettingsPath = e.Args.FirstOrDefault();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject);

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception);
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception);
                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception exception)
        {
            MessageBox.Show(exception.ToString(), exception.Message, MessageBoxButton.OK);
        }
    }
}
