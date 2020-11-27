using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using Dashy.Settings;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Dashy
{
    public class BrowserInstance : IDisposable
    {
        private List<string> _scripts = new List<string>();
        private List<string> _styles = new List<string>();
        public WebView2 WebView { get; private set; }

        public void Init(BrowserInstanceSettings settings)
        {
            if (WebView == null)
            {
                WebView = new WebView2();
                WebView.EnsureCoreWebView2Async(CoreWebView2Environment.CreateAsync(userDataFolder: $"UserDataFolder.{settings.Profile}").Result);
                WebView.NavigationCompleted += WebView_NavigationCompleted;
            }

            if (settings.Zoom.HasValue)
            {
                WebView.ZoomFactor = settings.Zoom.Value;
            }

            if (settings.Js?.Any() == true)
            {
                _scripts = settings.Js
                    .Select(script =>
                    {
                        if (File.Exists(script))
                        {
                            return File.ReadAllText(script);
                        }

                        return script;
                    })
                    .ToList();
            }

            if (settings.Css?.Any() == true)
            {
                _styles = settings.Css
                    .Select(style =>
                    {
                        if (File.Exists(style))
                        {
                            return File.ReadAllText(style);
                        }

                        return style;
                    })
                    .ToList();
            }

            if (settings.Refresh > 0)
            {
                _scripts.Add($"setTimeout(function() {{ location.reload(); }}, {settings.Refresh * 1000})");
            }

            WebView.Source = new Uri(settings.Url);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke( () =>
            {
                WebView.CoreWebView2.Reload();
            });
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (_styles != null)
            {
                foreach (var style in _styles)
                {
                    var styleScript = $@"
                        var parent = document.getElementsByTagName('head').item(0);
                        var style = document.createElement('style');
                        style.type = 'text/css';
                        style.innerHTML = window.atob('{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(style))}');
                        parent.appendChild(style);
                    ";
                    ((WebView2)sender).ExecuteScriptAsync(styleScript);
                }
            }

            if (_scripts != null)
            {
                foreach (var script in _scripts)
                {
                    ((WebView2)sender).ExecuteScriptAsync(script);
                }
            }
        }

        public void Dispose()
        {
            WebView?.Dispose();
        }
    }
}