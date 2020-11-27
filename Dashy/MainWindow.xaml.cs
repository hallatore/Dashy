using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dashy.Settings;
using Microsoft.Web.WebView2.Wpf;

namespace Dashy
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            StateChanged += MainWindow_StateChanged;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Init()
        {
            var profile = ((App) App.Current).Profile;
            var settings = LoadSettings(profile);

            if (settings == null)
            {
                Close();
                return;
            }

            Width = settings.Width.Value;
            Height = settings.Height.Value;
            Title = settings.Title;
            TitleTextBlock.Text = Title;
            Topmost = settings.TopMost.Value;
            Application.Current.Resources["Background"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.Background));
            Application.Current.Resources["Foreground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(settings.Foreground));
            SetGridLayout(settings.Columns.Value, settings.Rows.Value);
            ResizeMode = (settings.CanResize == true) ? ResizeMode.CanResizeWithGrip : ResizeMode.CanMinimize;
            MaximizeButton.Visibility = (settings.CanResize == true) ? Visibility.Visible : Visibility.Collapsed;
            CloseButton.Visibility = (settings.HideClose == true) ? Visibility.Collapsed : Visibility.Visible;

            foreach (var viewSetting in settings.Views)
            {
                var instance = new BrowserInstance();
                instance.Init(viewSetting);
                AddWebControlToGrid(instance.WebView, viewSetting.ColIndex.Value, viewSetting.ColSpan.Value, viewSetting.RowIndex.Value, viewSetting.RowSpan.Value);
            }
        }

        private ContainerSettings LoadSettings(string profile)
        {
            var path = $"{profile}.json";

            if (File.Exists(path))
            {
                return JsonSerializer.Deserialize<ContainerSettings>(File.ReadAllBytes(path), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            MessageBox.Show($"File not found: {path}", "Settings file not found", MessageBoxButton.OK);
            return null;
        }

        private void AddWebControlToGrid(WebView2 webView, int colIndex, int colSpan, int rowIndex, int rowSpan)
        {
            Grid.SetColumn(webView, colIndex);
            Grid.SetColumnSpan(webView, colSpan);
            Grid.SetRow(webView, rowIndex);
            Grid.SetRowSpan(webView, rowSpan);
            GridContainer.Children.Add(webView);
        }

        private void SetGridLayout(int columns, int rows)
        {
            var star = new GridLength(1, GridUnitType.Star);

            for (var i = 0; i < columns; i++)
            {
                GridContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = star });
            }

            for (var i = 0; i < rows; i++)
            {
                GridContainer.RowDefinitions.Add(new RowDefinition { Height = star });
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnMaximize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void MainWindow_StateChanged(object sender, System.EventArgs e)
        {
            WindowGrid.Margin = WindowState == WindowState.Maximized ? new Thickness(5) : new Thickness(0);
        }

        private void OnMinimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Element_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ((UIElement)sender).Opacity = 1;
        }

        private void Element_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ((UIElement)sender).Opacity = 0.3;
        }
    }
}
