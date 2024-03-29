﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dashy.Settings;
using Dashy.Utils;

namespace Dashy
{
    public partial class MainWindow : Window
    {
        private readonly List<BrowserInstance> _browserInstances = new();
        private WindowMenuHelper _windowMenuHelper;
        private TaskbarOverlay _taskbarOverlay;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
            _taskbarOverlay = new TaskbarOverlay(TaskbarItemInfo);

            _windowMenuHelper = new WindowMenuHelper(this);
            _windowMenuHelper.InsertSeparator();
            _windowMenuHelper.InsertMenuItem("Refresh", () => OnRefreshViews(this, new RoutedEventArgs()));
            _windowMenuHelper.InsertMenuItem("Reload settings", () => OnReloadSettings(this, new RoutedEventArgs()));
        }

        private void Init()
        {
            var settingsPath = ((App)App.Current).SettingsPath;

            if (settingsPath == null)
            {
                settingsPath = CreateShortWithSettingsPath();
            }

            if (settingsPath?.EndsWith(".json") == false)
            {
                settingsPath += "\\settings.json";
            }

            var resolvedSettingsPath = FileUtils.ResolvePath(settingsPath);
            var settings = LoadSettingsFromPath(resolvedSettingsPath);

            if (settings == null)
            {
                MessageBox.Show($"Failed to load settings file: {settingsPath}", "Settings file not found", MessageBoxButton.OK);
                Close();
                return;
            }

            var iconPath = SettingsUtils.TryResolveIconPath(resolvedSettingsPath);

            if (File.Exists(iconPath))
            {
                Icon = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
            }

            Width = settings.Width;
            Height = settings.Height;
            Title = settings.Title;
            Topmost = settings.TopMost;
            ResizeMode = settings.CanResize ? ResizeMode.CanResizeWithGrip : ResizeMode.CanMinimize;
            ThemeUtils.UseImmersiveDarkMode(new WindowInteropHelper(this).Handle, settings.DarkTheme);
            SetGridLayout(settings.Columns, settings.Rows);
            Application.Current.Resources["Background"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.Background));
            Application.Current.Resources["Foreground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.Foreground));
            Visibility = Visibility.Visible;

            if (settings.Views.Count == _browserInstances.Count &&
                _browserInstances
                    .Select((instance, index) => instance.CanDoSoftReload(settings.Views[index]))
                    .All(canDo => canDo))
            {
                for (var i = 0; i < settings.Views.Count; i++)
                {
                    var viewSetting = settings.Views[i];
                    _browserInstances[i].ReloadSettings(viewSetting);
                    SetGridSettings(_browserInstances[i].UIElement, viewSetting.ColIndex, viewSetting.ColSpan, viewSetting.RowIndex, viewSetting.RowSpan);
                }
            }
            else
            {
                foreach (var browserInstance in _browserInstances)
                {
                    GridContainer.Children.Remove(browserInstance.UIElement);
                    browserInstance.Dispose();
                }

                _browserInstances.Clear();

                foreach (var viewSetting in settings.Views)
                {
                    var instance = new BrowserInstance(viewSetting, resolvedSettingsPath);
                    instance.OnBadgeNumberUpdate += SetBadgeNumber;
                    instance.OnBadgeTypeUpdate += SetBadgeType;
                    instance.OnTitleUpdate += SetTitle;
                    instance.OnNavigate += OnNavigate;
                    SetGridSettings(instance.UIElement, viewSetting.ColIndex, viewSetting.ColSpan, viewSetting.RowIndex, viewSetting.RowSpan);
                    instance.UIElement.ClipToBounds = true;
                    GridContainer.Children.Add(instance.UIElement);
                    _browserInstances.Add(instance);
                }
            }
        }

        private void OnNavigate(string url)
        {
            var availableView = _browserInstances.FirstOrDefault(instance => instance.HandleUrl(url));

            if (availableView != null)
            {
                availableView.Navigate(url);
            }
            else
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
        }

        private void SetBadgeNumber(int number)
        {
            Dispatcher.Invoke(() => { _taskbarOverlay.SetNumber(number); });
        }

        private void SetBadgeType(OverlayType type)
        {
            Dispatcher.Invoke(() => { _taskbarOverlay.SetOverlayType(type); });
        }

        private void SetTitle(string value)
        {
            Dispatcher.Invoke(() => { Title = value; });
        }

        private string CreateShortWithSettingsPath()
        {
            var settingsPath = SettingsUtils.GetSettingsPath(out var originalPath);

            if (settingsPath == null)
            {
                return null;
            }

            SettingsUtils.CreateShortcut(settingsPath, originalPath);
            return settingsPath;
        }

        private ContainerSettings LoadSettingsFromPath(string path)
        {
            if (File.Exists(path))
            {
                return JsonSerializer.Deserialize<ContainerSettings>(File.ReadAllBytes(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return null;
        }

        private void SetGridSettings(UIElement element, uint colIndex, uint colSpan, uint rowIndex, uint rowSpan)
        {
            Grid.SetColumn(element, (int)colIndex);
            Grid.SetColumnSpan(element, (int)colSpan);
            Grid.SetRow(element, (int)rowIndex);
            Grid.SetRowSpan(element, (int)rowSpan);
        }

        private void SetGridLayout(string[] columns, string[] rows)
        {
            GridContainer.ColumnDefinitions.Clear();
            GridContainer.RowDefinitions.Clear();

            if (columns != null)
            {
                foreach (var column in columns)
                {
                    GridContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = ParseLength(column) });
                }
            }

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    GridContainer.RowDefinitions.Add(new RowDefinition { Height = ParseLength(row) });
                }
            }
        }

        private GridLength ParseLength(string inputValue)
        {
            if (inputValue.EndsWith("*") &&
                double.TryParse(
                    inputValue.Substring(0, inputValue.Length - 1),
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out var starValue))
            {
                return new GridLength(starValue, GridUnitType.Star);
            }

            if (double.TryParse(inputValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
            {
                return new GridLength(value, GridUnitType.Pixel);
            }

            return new GridLength(1, GridUnitType.Star);
        }

        private void OnReloadSettings(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void OnRefreshViews(object sender, RoutedEventArgs e)
        {
            foreach (var instance in _browserInstances)
            {
                instance.Reload();
            }
        }
    }
}