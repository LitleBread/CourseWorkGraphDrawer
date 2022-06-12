using System.Collections.Generic;
using System.Windows.Media;


namespace CourseWorkGraphDrawer
{
    class Style
    {
        private static Dictionary<string, DoubleCollection> dashPatterns = new Dictionary<string, DoubleCollection>();
        static Style()
        {
            dashPatterns.Add("Сплошная", null);
            dashPatterns.Add("Штриховая", new DoubleCollection(new double[] { 5, 5 }));
            dashPatterns.Add("Пунктирная", new DoubleCollection(new double[] { 2, 4 }));
            dashPatterns.Add("Штрих-пунктирная", new DoubleCollection(new double[] { 5, 1, 1, 5 }));
        }

        public Style(double thickness, string dashPattern, Color color, bool toShow)
        {
            Thickness = thickness;
            DashPattern = dashPatterns[dashPattern];
            Brush = new SolidColorBrush(color);
            ToShow = toShow;
        }

        public double Thickness { get; set; }
        public DoubleCollection DashPattern { get; set; }
        public Brush Brush { get; set; }
        public bool ToShow { get; set; }

    }
}
