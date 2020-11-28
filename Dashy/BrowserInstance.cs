using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Dashy.Settings;
using Dashy.Utils;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Dashy
{
    public class BrowserInstance : IDisposable
    {
        private readonly BrowserInstanceSettings _settings;
        private readonly List<string> _scripts = new List<string>();
        private readonly List<string> _styles = new List<string>();
        private readonly WebView2 _webView;

        public UIElement UIElement => _webView;

        public BrowserInstance(BrowserInstanceSettings settings, string profilePath)
        {
            _settings = settings;
            if (_webView == null)
            {
                _webView = new WebView2();
                _webView.EnsureCoreWebView2Async(CoreWebView2Environment.CreateAsync(userDataFolder: $"UserDataFolder.{settings.Profile}").GetAwaiter().GetResult());
                _webView.NavigationCompleted += WebView_NavigationCompleted;
                _webView.NavigationStarting += WebView_NavigationStarting;
                _webView.CoreWebView2Ready += WebView_CoreWebView2Ready;
            }

            if (settings.Js?.Any() == true)
            {
                _scripts = settings.Js
                    .Select(script =>
                    {
                        var scriptPath = FileUtils.ResolvePath(script, profilePath);
                        if (scriptPath != null)
                        {
                            return File.ReadAllText(scriptPath);
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
                        var stylePath = FileUtils.ResolvePath(style, profilePath);
                        if (stylePath != null)
                        {
                            return File.ReadAllText(stylePath);
                        }

                        return style;
                    })
                    .ToList();
            }

            if (settings.Refresh > 0)
            {
                _scripts.Add($"setTimeout(function() {{ location.reload(); }}, {settings.Refresh * 1000})");
            }

            _webView.ZoomFactor = settings.Zoom;
            _webView.Source = settings.Url;
        }

        private void WebView_CoreWebView2Ready(object sender, EventArgs e)
        {
            _webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            Process.Start(new ProcessStartInfo { FileName = e.Uri, UseShellExecute = true });
        }

        private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            var tempUri = new Uri(e.Uri);

            if ((!_settings.HandleInternalNavigation && tempUri != _settings.Url) ||
                (!_settings.HandleExternalNavigation && tempUri.IsAbsoluteUri && tempUri.Host != _settings.Url.Host))
            {
                e.Cancel = true;
                Process.Start(new ProcessStartInfo { FileName = e.Uri, UseShellExecute = true });
            }
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
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

                _webView.ExecuteScriptAsync(styleScript);
            }

            foreach (var script in _scripts)
            {
                _webView.ExecuteScriptAsync(script);
            }
        }

        public void Dispose()
        {
            _webView?.Dispose();
        }

        public void Reload()
        {
            if (_webView.Source != _settings.Url)
            {
                _webView.Source = _settings.Url;
            }
            else
            {
                _webView.Reload();
            }
        }
    }
}