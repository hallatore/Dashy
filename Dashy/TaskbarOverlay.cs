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
        private readonly TextBlock _textBlock;
        private readonly GeometryDrawing _aGeometryDrawing;
        private int _number;

        public TaskbarOverlay(TaskbarItemInfo taskbarItemInfo)
        {
            _taskbarItemInfo = taskbarItemInfo;
            var ellipses = new GeometryGroup();
            ellipses.Children.Add(new RectangleGeometry(new Rect(new Size(16, 16))));

            _aGeometryDrawing = new GeometryDrawing();
            _aGeometryDrawing.Geometry = ellipses;

            _grid = new Grid();
            _grid.Width = 16;
            _grid.Height = 16;
            _grid.Children.Add(new Ellipse { Fill = new SolidColorBrush(Colors.Black) });
            _textBlock = new TextBlock { FontSize = 11, Foreground = new SolidColorBrush(Colors.White), VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Center};
            _grid.Children.Add(_textBlock);
            _aGeometryDrawing.Brush = new VisualBrush(_grid);
        }

        public void UpdateNumber(int number)
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
    }
}
