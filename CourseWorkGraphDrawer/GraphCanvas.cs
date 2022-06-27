using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CourseWorkGraphDrawer
{
    internal class GraphCanvas : Canvas
    {
        private Dictionary<Graph, Polyline> graphMap;

        private TextBlock[,] coordinates;
        private Line[,] coordinatesGrid;

        
        private Dictionary<Graph, List<Border>> foundIntersections;
        private Dictionary<Graph, List<Point>> foundIntersectionsOrigins;

        private Line xAxis;
        private Line yAxis;

        

        public GraphCanvas()
        {
            graphMap = new Dictionary<Graph, Polyline>();
            
            foundIntersections = new Dictionary<Graph, List<Border>>();
            foundIntersectionsOrigins = new Dictionary<Graph, List<Point>>();
            
            int gridSize = 100;

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
            foreach (var item in graphMap)
            {
                for (int i = 0; i < item.Value.Points.Count; i++)
                {
                    double newX = item.Key.Points[i].X * scaleFactor + zero.X;
                    double newY = item.Key.Points[i].Y * scaleFactor + zero.Y;
                    item.Value.Points[i] = new Point(newX, newY);
                }
            }
            //подсчет новых экранных позиций пересечений

            foreach (var item in foundIntersections)
            {
                int i = 0;
                foreach (Border itemBorder in item.Value)
                {
                    itemBorder.SetValue(Canvas.LeftProperty, foundIntersectionsOrigins[item.Key][i].X * scaleFactor + zero.X - 5);
                    itemBorder.SetValue(Canvas.TopProperty, foundIntersectionsOrigins[item.Key][i].Y * scaleFactor + zero.Y - 5);
                    i++;
                }
            }
            
            //пересчет экранных позиций сетки (отдельно по х и у)
            for (int x = 0; x < coordinates.GetLength(1); x++)
            {
                double yPos = zero.Y > 0 ? zero.Y - 25 : 15;
                yPos = yPos < ActualHeight ? yPos : ActualHeight - 25;

                coordinates[0, x].SetValue(Canvas.TopProperty, yPos);


                double xPos = zero.X + GetScreenPosition(zero, scaleFactor, x);
                coordinates[0, x].SetValue(Canvas.LeftProperty, xPos);
                coordinatesGrid[0, x].X1 = xPos + 5;
                coordinatesGrid[0, x].X2 = xPos + 5;
                coordinatesGrid[0, x].Y1 = 0;
                coordinatesGrid[0, x].Y2 = ActualHeight;

                Point position = GetMathPosition(zero, (double)coordinates[0, x].GetValue(Canvas.LeftProperty), (double)coordinates[0, x].GetValue(Canvas.TopProperty), scaleFactor);

                coordinates[0, x].Text = string.Format("{0:N0}", -position.X);
            }
            for (int y = 0; y < coordinates.GetLength(1); y++)
            {
                double xPos = zero.X > 0 ? zero.X + 15 : 15;
                xPos = xPos < ActualWidth ? xPos : ActualWidth - 35;

                coordinates[1, y].SetValue(Canvas.LeftProperty, xPos);


                double yPos = zero.Y + GetScreenPosition(zero, scaleFactor, y);
                coordinates[1, y].SetValue(Canvas.TopProperty, yPos);
                coordinatesGrid[1, y].X1 = 0;
                coordinatesGrid[1, y].X2 = ActualWidth;
                coordinatesGrid[1, y].Y1 = yPos + 5;
                coordinatesGrid[1, y].Y2 = yPos + 5;


                Point position = GetMathPosition(zero, (double)coordinates[1, y].GetValue(Canvas.LeftProperty), (double)coordinates[1, y].GetValue(Canvas.TopProperty), scaleFactor);
                coordinates[1, y].Text = string.Format("{0:N0}", position.Y);
            }
        }

        public void AddGraph(Graph graph, Style style)
        {
            HideIntersections();
            foundIntersections.Clear();
            foundIntersectionsOrigins.Clear();
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
            graphMap.Add(graph, polyline);
            
            Children.Add(polyline);

        }
        public void RemoveGraph(Graph graph)
        {
            Children.Remove(graphMap[graph]);
            graphMap.Remove(graph);

        }

        public void HideGraph(Graph graph)
        {
            if (foundIntersections.ContainsKey(graph))
            {
                HideIntersections(graph);
            }
            Children.Remove(graphMap[graph]);
        }
        public void ShowGraph(Graph graph)
        {
            
            Children.Add(graphMap[graph]);
            try
            {
                foreach (Border item in foundIntersections[graph])
                {
                    Children.Add(item);
                }
            }
            catch { }
        }

        public void ShowIntersections(Graph graph)
        {
            if (foundIntersections.ContainsKey(graph))
            {
                foreach (var item in foundIntersections[graph])
                {
                    try
                    {
                        Children.Add(item);
                    }
                    catch { }
                }
                graphMap[graph].Focus();
                return;
            }
            FindIntersections(graph);
            foreach (Border item in foundIntersections[graph])
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
            foreach (var item in foundIntersections)
            {
                foreach (Border itemBorder in item.Value)
                {
                    try
                    {
                        Children.Remove(itemBorder);
                    }
                    catch { }
                }
            }
            
        }
        public void HideIntersections(Graph graph)
        {

            foreach (Border itemBorder in foundIntersections[graph])
            {
                try
                {
                    Children.Remove(itemBorder);
                }
                catch { }
            }


        }

        private double GetScreenPosition(Point zero, double scaleFactor, int arrayPosition)
        {
            int centerPosition = coordinates.GetLength(1) / 2;
            double res = (arrayPosition < centerPosition ? -1 : 1) * (arrayPosition < centerPosition ? arrayPosition : arrayPosition - centerPosition) * scaleFactor - 5;
            return res;
        }

        private Point GetMathPosition(Point zero, double xOffset, double yOffset, double scaleFactor)
        {


            double x = xOffset / (-scaleFactor) + (zero.X - 5) / scaleFactor;
            double y = yOffset / (-scaleFactor) + (zero.Y - 5) / scaleFactor;
            return new Point(x, y);
        }

        private void FindIntersections(Graph graph)
        {

            List<Border> intersectionBorders = new List<Border>();
            List<Point> intersectionPoints = new List<Point>();

            Polyline plToFocus = graphMap[graph];
            plToFocus.Focus();

            foreach (KeyValuePair<Graph, Polyline> item in graphMap)
            {
                if (item.Key.Equals(graph))
                {
                    continue;
                }

                for (int i = 0; i < graph.Points.Count - 1; i++)
                {
                    for (int j = 0; j < item.Key.Points.Count - 1; j++)
                    {

                        Point intersectionPoint = GetIntersectionPoint(
                        graph.Points[i].X, graph.Points[i].Y,
                        graph.Points[i + 1].X, graph.Points[i + 1].Y,
                        item.Key.Points[j].X, item.Key.Points[j].Y,
                        item.Key.Points[j + 1].X, item.Key.Points[j + 1].Y);
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

                        intersectionBorders.Add(border);
                        intersectionPoints.Add(intersectionPoint);

                    }
                }
            }
            
            foundIntersections.Add(graph, intersectionBorders);
            foundIntersectionsOrigins.Add(graph, intersectionPoints);
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
            

        }

        private void OnIntersectionsColorChanged(Brush brush)
        {
            foreach (var item in foundIntersections)
            {
                foreach (Border intersection in item.Value)
                {
                    intersection.Background = brush;
                }
            }
        }
        private void OnAxisColorChanged(Brush brush)
        {
            xAxis.Stroke = brush;
            yAxis.Stroke = brush;

        }
        private void OnBackgroundColorChanged(Brush brush)
        {
            Background = Settings.BackgroundBrush;
        }
        private void OnGridColorChanged(Brush brush)
        {
            foreach (Line item in coordinatesGrid)
            {
                item.Stroke = brush;
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
