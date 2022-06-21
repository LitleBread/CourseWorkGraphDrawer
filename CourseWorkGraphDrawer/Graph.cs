using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace CourseWorkGraphDrawer
{
    class Graph
    {
        public string Function { 
            get 
            { return function; }
            set 
            {
                function = value;
                Calculate();
            }
        }
        public string CoordinateSystem { get; private set; }
        public List<Point> Points { get; private set; } = new List<Point>();
        private string function;
        private bool isDecart;
        private double min, max, step;

        public Graph(string funcString, bool isDecart, double min, double max, double step)
        {
            this.min = min;
            this.max = max;
            this.step = step;
            this.isDecart = isDecart;

            if (step <= 0)
            {
                step = 1e-5;
            }
            this.Function = funcString;
            CoordinateSystem = isDecart ? "Декартовая" : "Полярная";
            
        }

        private void Calculate()
        {
            Func<double, double> func = Compiler.GetDelegate(function);
            Points = new List<Point>();
            double yTemp;
            if (!isDecart)
            {
                double x, y;
                min *= Math.PI / 180;
                max *= Math.PI / 180;
                step *= Math.PI / 180;

                for (double f = min; f <= max; f += step)
                {
                    try
                    {
                        yTemp = func(f);
                        ToDekart(yTemp, f, out x, out y);
                        if (Math.Abs(x) > 100000 || Math.Abs(y) > 100000 || double.IsNaN(y))
                            continue;
                        Points.Add(new Point(-x, y));
                    }
                    catch
                    {
                        continue;
                    }

                }
            }
            else
            {
                for (double x = min; x <= max; x += step)
                {
                    try
                    {
                        yTemp = func(x);
                        if (Math.Abs(x) > 100000 || Math.Abs(yTemp) > 100000 || double.IsNaN(yTemp))
                            continue;
                        Points.Add(new Point(x, yTemp));
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }
        private void ToDekart(double r, double f, out double x, out double y)
        {
            x = r * Math.Cos(f);
            y = r * Math.Sin(f);
        }


    }
}
