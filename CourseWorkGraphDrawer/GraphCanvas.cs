using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace CourseWorkGraphDrawer
{
    internal class GraphCanvas : Canvas
    {
        private List<Polyline> lines;
        private List<Graph> graphs;

        private TextBlock[,] coordinates;
        private Line[,] coordinatesGrid;

        private List<Border> intersections;
        private List<Point> intersectionsOrigin;

        private Line xAxis;
        private Line yAxis;

        public GraphCanvas()
        {
            lines = new List<Polyline>();
            graphs = new List<Graph>();
            intersections = new List<Border>();
            intersectionsOrigin = new List<Point>();
            
            int gridSize = 150;
            coordinates = new TextBlock[2, gridSize];
            coordinatesGrid = new Line[2, gridSize];
            for (int i = 0; i < coordinates.GetLength(0); i++)
            {
                for (int j = 0; j < coordinates.GetLength(1); j++)
                {
                    coordinates[i, j] = new TextBlock();
                    coordinatesGrid[i, j] = new Line();

                    coordinates[i, j].FontSize = 15;
                    coordinatesGrid[i, j].Stroke = Settings.GridBrush;
                    Children.Add(coordinates[i, j]);
                    Children.Add(coordinatesGrid[i, j]);
                }
            }

            xAxis = new Line();
            yAxis = new Line();

            Children.Add(yAxis);
            Children.Add(xAxis);

            xAxis.Stroke = Settings.AxisBrush;
            xAxis.StrokeThickness = 2;

            yAxis.Stroke = Settings.AxisBrush;
            yAxis.StrokeThickness = 2;


            Settings.BackgroundColorChanged += OnBackgroundColorChanged;
            Settings.AxisColorChanged += OnAxisColorChanged;
            Settings.IntersectionsColorChanged += OnIntersectionsColorChanged;
            Settings.GridColorChanged += OnGridColorChanged;

        }
        
        public void CalculatePointsPositions(Point zero, double scaleFactor)
        {
            //подсчет новых позиций осей
            xAxis.X1 = -1;
            xAxis.X2 = ActualWidth;
            xAxis.Y1 = zero.Y;
            xAxis.Y2 = zero.Y;
            yAxis.Y1 = -1;
            yAxis.Y2 = ActualHeight;
            yAxis.X1 = zero.X;
            yAxis.X2 = zero.X;
            //новые экранные координаты точек графиков
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = 0; j < lines[i].Points.Count; j++)
                {
                    double newX = graphs[i].Points[j].X * scaleFactor + zero.X;
                    double newY = graphs[i].Points[j].Y * scaleFactor + zero.Y;

                    lines[i].Points[j] = new Point(newX, newY);
                }
            }
            //если показаны пересечения, то подсчет новых экранных позиций пересечений
            if (intersections.Count > 0)
            {
                for (int i = 0; i < intersections.Count; i++)
                {
                    intersections[i].SetValue(Canvas.LeftProperty, intersectionsOrigin[i].X * scaleFactor + zero.X - 5);
                    intersections[i].SetValue(Canvas.TopProperty, intersectionsOrigin[i].Y * scaleFactor + zero.Y - 5);
                }
            }
            //пересчет экранных позиций сетки (отдельно по х и у)
            for (int x = 0; x < coordinates.GetLength(1); x++)
            {
                double yPos = zero.Y > 0 ? zero.Y - 25 : 15;
                yPos = yPos < ActualHeight ? yPos : ActualHeight - 25;

                coordinates[0, x].SetValue(Canvas.TopProperty, yPos);


                double xPos = zero.X + (x < coordinates.GetLength(1) / 2 ? -1 : 1) *
                    (x < coordinates.GetLength(1) / 2 ? x : x - coordinates.GetLength(1) / 2) * scaleFactor - 5;
                coordinates[0, x].SetValue(Canvas.LeftProperty, xPos);
                coordinatesGrid[0, x].X1 = xPos + 5;
                coordinatesGrid[0, x].X2 = xPos + 5;
                coordinatesGrid[0, x].Y1 = 0;
                coordinatesGrid[0, x].Y2 = ActualHeight;

                Point position = new Point((double)coordinates[0, x].GetValue(Canvas.LeftProperty) / -scaleFactor + zero.X / scaleFactor - 5 / scaleFactor, (double)coordinates[0, x].GetValue(Canvas.TopProperty) / (-scaleFactor) + zero.Y / scaleFactor);
                coordinates[0, x].Text = string.Format("{0:N0}", -position.X);
            }
            for (int y = 0; y < coordinates.GetLength(1); y++)
            {
                double xPos = zero.X > 0 ? zero.X + 15 : 15;
                xPos = xPos < ActualWidth ? xPos : ActualWidth - 35;

                coordinates[1, y].SetValue(Canvas.LeftProperty, xPos);


                double yPos = zero.Y + ((y < coordinates.GetLength(1) / 2) ? -1 : 1) *
                    ((y < coordinates.GetLength(1) / 2) ? y : y - coordinates.GetLength(1) / 2) * scaleFactor - 5;
                coordinates[1, y].SetValue(Canvas.TopProperty, yPos);
                coordinatesGrid[1, y].X1 = 0;
                coordinatesGrid[1, y].X2 = ActualWidth;
                coordinatesGrid[1, y].Y1 = yPos + 5;
                coordinatesGrid[1, y].Y2 = yPos + 5;


                Point position = new Point((double)coordinates[1, y].GetValue(Canvas.LeftProperty) / scaleFactor + zero.X / scaleFactor, (double)coordinates[1, y].GetValue(Canvas.TopProperty) / (-scaleFactor) + zero.Y / scaleFactor - 5 / scaleFactor);
                coordinates[1, y].Text = string.Format("{0:N0}", position.Y);
            }
        }
        public void AddGraph(Graph graph, Style style)
        {
            Polyline polyline = new Polyline
            {
                Stroke = style.Brush,
                StrokeDashArray = style.DashPattern,
                StrokeThickness = style.Thickness,
                Focusable = true
            };
            polyline.GotFocus += OnPolylineGotFocus;
            polyline.LostFocus += OnPolylineLostFocus;

            foreach (Point item in graph.Points)
            {
                polyline.Points.Add(new Point(item.X , item.Y));
            }
            lines.Add(polyline);
            graphs.Add(graph);
            Children.Add(polyline);

        }
        public void RemoveGraph(Graph graph)
        {
            int toRemoveIndex = graphs.IndexOf(graph);
            graphs.RemoveAt(toRemoveIndex);
            Children.Remove(lines[toRemoveIndex]);
            lines.RemoveAt(toRemoveIndex);

        }
        public void HideGraph(Graph graph)
        {
            int toHideIndex = graphs.IndexOf(graph);
            Children.Remove(lines[toHideIndex]);
        }
        public void ShowGraph(Graph graph)
        {
            int toShowIndex = graphs.IndexOf(graph);
            Children.Add(lines[toShowIndex]);
            try
            {
                foreach (Border item in intersections)
                {
                    Children.Add(item);
                }
            }
            catch { }
        }
        public void ShowIntersections(Graph graph)
        {
            FindIntersections(graph);
            foreach (Border item in intersections)
            {
                try
                {
                    Children.Add(item);
                }
                catch { }
            }
        }
        public void HideIntersections()
        {
            foreach (Border item in intersections)
            {
                try
                {
                    Children.Remove(item);
                }
                catch
                { }
            }
            intersections.Clear();
            intersectionsOrigin.Clear();
        }
        private void FindIntersections(Graph graph)
        {

            List<Border> intersectionPoints = new List<Border>();
            List<Point> intersectionPointsOrigin = new List<Point>();

            Polyline plToFocus = lines[graphs.IndexOf(graph)];
            plToFocus.Focus();

            foreach (Graph item in graphs)
            {
                if (item.Equals(graph))
                {
                    continue;
                }

                for (int i = 0; i < graph.Points.Count - 1; i++)
                {
                    for (int j = 0; j < item.Points.Count - 1; j++)
                    {

                        Point intersectionPoint = GetIntersectionPoint(
                        graph.Points[i].X, graph.Points[i].Y,
                        graph.Points[i + 1].X, graph.Points[i + 1].Y,
                        item.Points[j].X, item.Points[j].Y,
                        item.Points[j + 1].X, item.Points[j + 1].Y);
                        if (intersectionPoint == default)
                        {
                            continue;
                        }

                        Border border = new Border();
                        int radius = 10;
                        border.CornerRadius = new CornerRadius(radius / 2);


                        border.Width = radius;
                        border.Height = radius;
                        border.SetValue(Canvas.LeftProperty, intersectionPoint.X - radius / 2);
                        border.SetValue(Canvas.TopProperty, intersectionPoint.Y - radius / 2);
                        border.SetValue(Canvas.ZIndexProperty, 1);
                        border.Background = Settings.IntersectionsBrush;

                        intersectionPoints.Add(border);
                        intersectionPointsOrigin.Add(intersectionPoint);

                    }
                }
            }




            if (intersectionPoints.Count > 0)
            {
                intersections.AddRange(intersectionPoints);
                intersectionsOrigin.AddRange(intersectionPointsOrigin);
            }

        }
        private Point GetIntersectionPoint(double x1, double y1, double x2, double y2,
           double x3, double y3, double x4, double y4)
        {
            double x = (
                (x1 * y2 - y1 * x2)
                * (x3 - x4) -
                (x1 - x2) * ((x3 * y4) -
                (y3 * x4))) /
                (((x1 - x2) * (y3 - y4)) - ((y1 - y2) * (x3 - x4)));
            double y = (
                (x1 * y2 - y1 * x2)
                * (y3 - y4) -
                (y1 - y2) * ((x3 * y4) -
                (y3 * x4))) /
                (((x1 - x2) * (y3 - y4)) - ((y1 - y2) * (x3 - x4)));
            if (double.IsInfinity(x) || double.IsInfinity(y))
            {
                return default;
            }

            if ((
                (((x1 <= x && x <= x2) || (x1 >= x && x >= x2)) && ((x3 <= x && x <= x4) || (x3 >= x && x >= x4)))
                ||
                (((y1 <= y && y <= y2) || (y1 >= y && y >= y2)) && ((y3 <= y && y <= y4) || (y3 >= y && y >= y4)))
                ))
            {
                return new Point(x, y);
            }

            return default;
            //return default;

        }

        private void OnIntersectionsColorChanged()
        {
            foreach (Border item in intersections)
            {
                item.Background = Settings.IntersectionsBrush;
            }
        }
        private void OnAxisColorChanged()
        {
            xAxis.Stroke = Settings.AxisBrush;
            yAxis.Stroke = Settings.AxisBrush;

        }
        private void OnBackgroundColorChanged()
        {
            Background = Settings.BackgroundBrush;
        }
        private void OnGridColorChanged()
        {
            foreach (Line item in coordinatesGrid)
            {
                item.Stroke = Settings.GridBrush;
            }
        }

        private void OnPolylineGotFocus(object sender, RoutedEventArgs e)
        {
            (sender as Polyline).StrokeThickness *= 1.5;
        }
        private void OnPolylineLostFocus(object sender, RoutedEventArgs e)
        {
            (sender as Polyline).StrokeThickness /= 1.5;
        }
    }
}
