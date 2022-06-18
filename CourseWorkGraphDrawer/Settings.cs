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
            get { return backgroundColor; }
            set
            {
                backgroundColor = value;
                BackgroundBrush = new SolidColorBrush(value);
                BackgroundColorChanged?.Invoke();
            }
        }
        public static Color AxisColor
        {
            get { return axisColor; }
            set
            {
                axisColor = value;
                AxisBrush = new SolidColorBrush(value);
                AxisColorChanged?.Invoke();
            }
        }
        public static Color IntersectionsdColor
        {
            get { return intersectionsColor; }
            set
            {
                intersectionsColor = value;
                IntersectionsBrush = new SolidColorBrush(value);
                IntersectionsColorChanged?.Invoke();
            }
        }
        public static Color GridColor
        {
            get { return gridColor; }
            set
            {
                gridColor = value;
                GridBrush = new SolidColorBrush(value);
                GridColorChanged?.Invoke();
            }
        }
        public delegate void ParameterChanged();

        public static event ParameterChanged BackgroundColorChanged;
        public static event ParameterChanged AxisColorChanged;
        public static event ParameterChanged IntersectionsColorChanged;
        public static event ParameterChanged GridColorChanged;


        public static Brush AxisBrush { get; private set; } = Brushes.Black;
        public static Brush IntersectionsBrush { get; private set; } = Brushes.Red;
        public static Brush BackgroundBrush { get; private set; } = Brushes.AntiqueWhite;
        public static Brush GridBrush { get; private set; } = new SolidColorBrush(Color.FromArgb(155, 77, 77, 77));

        private static Color backgroundColor;
        private static Color axisColor;
        private static Color intersectionsColor;
        private static Color gridColor;
    }
}
