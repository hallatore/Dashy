using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace Dashy
{
    public class TaskbarOverlay
    {
        private readonly TaskbarItemInfo _taskbarItemInfo;
        private readonly Grid _grid;
        private Ellipse _circle;
        private readonly TextBlock _textBlock;
        private readonly GeometryDrawing _aGeometryDrawing;
        private int _number;
        private OverlayType _type;

        public TaskbarOverlay(TaskbarItemInfo taskbarItemInfo)
        {
            _taskbarItemInfo = taskbarItemInfo;
            var geometryGroup = new GeometryGroup();
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(new Size(16, 16))));

            _aGeometryDrawing = new GeometryDrawing();
            _aGeometryDrawing.Geometry = geometryGroup;

            _grid = new Grid { Width = 16, Height = 16, Visibility = Visibility.Hidden };
            _circle = new Ellipse { Fill = new SolidColorBrush(Colors.Black) };
            _textBlock = new TextBlock { FontSize = 11, Foreground = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Center};
            _grid.Children.Add(_circle);
            _grid.Children.Add(_textBlock);
            _aGeometryDrawing.Brush = new VisualBrush(_grid);
            
        }

        public void SetNumber(int number)
        {
            if (_number == number)
            {
                return;
            }

            _number = number;
            _textBlock.Text = number.ToString();
            _grid.Visibility = number > 0 ? Visibility.Visible : Visibility.Hidden;
            _grid.UpdateLayout();
            _taskbarItemInfo.Overlay = new DrawingImage(_aGeometryDrawing);
        }

        public void SetOverlayType(OverlayType type)
        {
            if (_type == type)
            {
                return;
            }

            _circle.Fill = type switch
            {
                OverlayType.Warning => new SolidColorBrush((Color) ColorConverter.ConvertFromString("#f0a30a")),
                OverlayType.Error => new SolidColorBrush((Color) ColorConverter.ConvertFromString("#e51400")),
                _ => new SolidColorBrush(Colors.Black)
            };

            _type = type;
            _circle.UpdateLayout();
            _taskbarItemInfo.Overlay = new DrawingImage(_aGeometryDrawing);
        }
    }

    public enum OverlayType
    {
        Default,
        Warning,
        Error
    }
}
