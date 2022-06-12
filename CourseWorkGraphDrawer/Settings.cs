using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
namespace CourseWorkGraphDrawer
{
    static class Settings
    {
        public static Color BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                BackgroundBrush = new SolidColorBrush(value);
                BackgroundColorChanged?.Invoke();
            }
        }
        public static Color AxisColor
        {
            get { return _axisColor; }
            set
            {
                _axisColor = value;
                AxisBrush = new SolidColorBrush(value);
                AxisColorChanged?.Invoke();
            }
        }
        public static Color IntersectionsdColor
        {
            get { return _intersectionsColor; }
            set
            {
                _intersectionsColor = value;
                IntersectionsBrush = new SolidColorBrush(value);
                IntersectionsColorChanged?.Invoke();
            }
        }
        public delegate void ParameterChanged();

        public static event ParameterChanged BackgroundColorChanged;
        public static event ParameterChanged AxisColorChanged;
        public static event ParameterChanged IntersectionsColorChanged;


        public static Brush AxisBrush { get; private set; } = Brushes.Black;
        public static Brush IntersectionsBrush { get; private set; } = Brushes.Red;
        public static Brush BackgroundBrush { get; private set; } = Brushes.AntiqueWhite;

        private static Color _backgroundColor;
        private static Color _axisColor;
        private static Color _intersectionsColor;
    }
}
