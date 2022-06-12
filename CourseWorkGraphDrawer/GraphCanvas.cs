using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CourseWorkGraphDrawer
{
    class GraphCanvas : Canvas
    {
        public List<Polyline> Lines { get; set; }
        private List<Graph> graphs;
        private TextBlock[,] coordinates;

        List<Border> intersections;
        List<Point> intersectionsOrigin;

        private readonly Line xAxis;
        private readonly Line yAxis;

        public GraphCanvas()
        {
            Lines = new List<Polyline>();
            graphs = new List<Graph>();
            intersections = new List<Border>();
            intersectionsOrigin = new List<Point>();

            coordinates = new TextBlock[2, 20];
            for (int i = 0; i < coordinates.GetLength(0); i++)
            {
                for (int j = 0; j < coordinates.GetLength(1); j++)
                {
                    coordinates[i, j] = new TextBlock();
                    coordinates[i, j].FontSize = 10;
                    Children.Add(coordinates[i, j]);
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

        }
        public void SetAxis(Point zero)
        {
            xAxis.X1 = zero.X < 0 || zero.X > 1000 ? -1 : zero.X - 4000;
            xAxis.X2 = zero.X < 0 || zero.X > 1000 ? 10000 : zero.X + 4000;
            xAxis.Y1 = zero.Y;
            xAxis.Y2 = zero.Y;
            yAxis.Y1 = zero.Y < 0 || zero.Y > 1000 ? -1 : zero.Y - 4000;
            yAxis.Y2 = zero.Y < 0 || zero.Y > 1000 ? 10000 : zero.Y + 4000;
            yAxis.X1 = zero.X;
            yAxis.X2 = zero.X;


        }
        public void CalculatePointsPositions(Point zero, double scaleFactor)
        {

            for (int i = 0; i < Lines.Count; i++)
            {
                for (int j = 0; j < Lines[i].Points.Count; j++)
                {
                    double newX = graphs[i].Points[j].X * scaleFactor + zero.X;
                    double newY = graphs[i].Points[j].Y * scaleFactor + zero.Y;

                    Lines[i].Points[j] = new Point(newX, newY);
                }

            }
            if (intersections.Count > 0)
            {
                for (int i = 0; i < intersections.Count; i++)
                {
                    intersections[i].SetValue(Canvas.LeftProperty, intersectionsOrigin[i].X * scaleFactor + zero.X - 5);
                    intersections[i].SetValue(Canvas.TopProperty, intersectionsOrigin[i].Y * scaleFactor + zero.Y - 5);
                }
            }
            for (int x = 0; x < coordinates.GetLength(1); x++)
            {
                double yPos = zero.Y > 0 ? zero.Y - 15 : 15;
                yPos = yPos < ActualHeight ? yPos : ActualHeight - 15;
                coordinates[0, x].SetValue(Canvas.TopProperty, yPos);
                coordinates[0, x].SetValue(Canvas.LeftProperty, 15 + zero.X + ((x < coordinates.GetLength(1) / 2) ? ActualWidth / coordinates.GetLength(1) * x : ActualWidth / coordinates.GetLength(1) * -(coordinates.GetLength(1) - x)));
                Point position = new Point((double)coordinates[0, x].GetValue(Canvas.LeftProperty) / -scaleFactor + zero.X / scaleFactor - 5 / scaleFactor, (double)coordinates[0, x].GetValue(Canvas.TopProperty) / (-scaleFactor) + zero.Y / scaleFactor);
                coordinates[0, x].Text = string.Format("{0:N2}", -position.X);
            }
            for (int y = 0; y < coordinates.GetLength(1); y++)
            {
                double xPos = zero.X > 0 ? zero.X + 15 : 15;
                xPos = xPos < ActualWidth ? xPos : ActualWidth - 15;

                coordinates[1, y].SetValue(Canvas.LeftProperty, xPos);
                double yPos = 15 + zero.Y + ((y < coordinates.GetLength(1) / 2) ? ActualHeight
                    / coordinates.GetLength(1) * y : ActualHeight / coordinates.GetLength(1) *
                    -(coordinates.GetLength(1) - y));
                coordinates[1, y].SetValue(Canvas.TopProperty, yPos);
                Point position = new Point((double)coordinates[1, y].GetValue(Canvas.LeftProperty) / scaleFactor + zero.X / scaleFactor, (double)coordinates[1, y].GetValue(Canvas.TopProperty) / (-scaleFactor) + zero.Y / scaleFactor - 5 / scaleFactor);
                coordinates[1, y].Text = string.Format("{0:N2}", position.Y);
            }
        }

        public void AddGraph(Graph graph, Style style, Point zero, double scaleFactor)
        {
            Polyline polyline = new Polyline();
            polyline.Stroke = style.Brush;
            polyline.StrokeDashArray = style.DashPattern;
            polyline.StrokeThickness = style.Thickness;

            foreach (var item in graph.Points)
            {

                polyline.Points.Add(new Point(item.X * scaleFactor + zero.X, item.Y * scaleFactor + zero.Y));

            }
            Lines.Add(polyline);
            graphs.Add(graph);
            this.Children.Add(polyline);

        }

        public void RemoveGraph(Graph graph)
        {
            var toRemoveIndex = graphs.IndexOf(graph);
            graphs.RemoveAt(toRemoveIndex);
            this.Children.Remove(Lines[toRemoveIndex]);
            Lines.RemoveAt(toRemoveIndex);

        }
        public void HideGraph(Graph graph)
        {
            var toHideIndex = graphs.IndexOf(graph);
            this.Children.Remove(Lines[toHideIndex]);
        }
        public void ShowGraph(Graph graph)
        {
            var toShowIndex = graphs.IndexOf(graph);
            this.Children.Add(Lines[toShowIndex]);
            try
            {
                foreach (var item in intersections)
                {
                    Children.Add(item);
                }
            }
            catch { }
        }
        public void ShowIntersections(Graph graph)
        {
            FindIntersections(graph);
            foreach (var item in intersections)
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
            foreach (var item in intersections)
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

            foreach (var item in graphs)
            {
                if (item.Equals(graph))
                    continue;
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
                            continue;
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
                return default;
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
            this.Background = Settings.BackgroundBrush;
        }


    }
}
