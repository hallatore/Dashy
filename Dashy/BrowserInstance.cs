using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Dashy.Settings;
using Dashy.Utils;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Dashy
{
    public delegate void BadgeNumberUpdate(int number);

    public class BrowserInstance : IDisposable
    {
        private BrowserInstanceSettings _settings;
        private string _profilePath;
        private List<string> _scripts = new List<string>();
        private List<string> _styles = new List<string>();
        private readonly WebView2 _webView;

        public UIElement UIElement => _webView;
        public event BadgeNumberUpdate OnBadgeNumberUpdate;

        public BrowserInstance(BrowserInstanceSettings settings, string profilePath)
        {
            _settings = settings;
            _profilePath = profilePath;

            if (_webView == null)
            {
                _webView = new WebView2();
                _webView.EnsureCoreWebView2Async(CoreWebView2Environment.CreateAsync(userDataFolder: $"UserDataFolder.{settings.Profile}").GetAwaiter().GetResult());
                _webView.NavigationCompleted += WebView_NavigationCompleted;
                _webView.NavigationStarting += WebView_NavigationStarting;
                _webView.CoreWebView2Ready += WebView_CoreWebView2Ready;
            }
            
            LoadSettings();
            _webView.Source = settings.Url;
        }

        public void ReloadSettings(BrowserInstanceSettings settings)
        {
            _settings = settings;
            LoadSettings();
            Reload();
        }

        private void LoadSettings()
        {
            _scripts = _settings.Js
                .Select(script =>
                {
                    if (script.EndsWith(".js"))
                    {
                        var scriptPath = FileUtils.ResolvePath(script, _profilePath);
                        if (scriptPath != null && File.Exists(scriptPath))
                        {
                            return File.ReadAllText(scriptPath);
                        }
                    }

                    return script;
                })
                .ToList();

            _styles = _settings.Css
                .Select(style =>
                {
                    if (style.EndsWith(".css"))
                    {
                        var stylePath = FileUtils.ResolvePath(style, _profilePath);
                        if (stylePath != null && File.Exists(stylePath))
                        {
                            return File.ReadAllText(stylePath);
                        }
                    }

                    return style;
                })
                .ToList();

            if (_settings.Refresh > 0)
            {
                _scripts.Add($"setTimeout(function() {{ location.reload(); }}, {_settings.Refresh * 1000})");
            }

            _webView.ZoomFactor = _settings.Zoom;
        }

        public bool CanDoSoftReload(BrowserInstanceSettings newSettings)
        {
            return _settings.Profile == newSettings.Profile;
        }

        private void WebView_CoreWebView2Ready(object sender, EventArgs e)
        {
            _webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            _webView.CoreWebView2.WebMessageReceived += CoreWebView2OnWebMessageReceived;
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            Process.Start(new ProcessStartInfo { FileName = e.Uri, UseShellExecute = true });
        }

        private void CoreWebView2OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var webMessage = JsonSerializer.Deserialize<BrowserWebMessage>(e.WebMessageAsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (webMessage?.Type == "badgeNumber")
            {
                OnBadgeNumberUpdate?.Invoke(int.TryParse(webMessage.Value, out var number) ? number : 0);
            }
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

    public class BrowserWebMessage
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}