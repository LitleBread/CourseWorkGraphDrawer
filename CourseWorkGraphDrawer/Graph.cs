﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace CourseWorkGraphDrawer
{
    class Graph
    {
        public string Function { get; private set; }
        public string CoordinateSystem { get; private set; }
        public List<Point> Points { get; private set; } = new List<Point>();

        public Graph(string funcString, bool isDecart, string sMin, string sMax, string sStep)
        {
            double.TryParse(sMin.Replace(".", ","), out double min);
            double.TryParse(sMax.Replace(".", ","), out double max);
            double.TryParse(sStep.Replace(".", ","), out double step);

            if (step <= 0)
            {
                step = 1e-5;
            }


            this.Function = funcString;
            Func<double, double> func = Compiler.GetDelegate(Parser.Parse(funcString));
            Points = new List<Point>();
            CoordinateSystem = isDecart ? "Декартовая" : "Полярная";



            if (Function.ToLower().Contains("log") && min <= 0)
            {
                min = 1e-6;
            }

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
                        if (Math.Abs(x) > 100000 || Math.Abs(y) > 100000)
                            continue;
                        Points.Add(new Point(-x, y));
                    }
                    catch
                    {
                        MessageBox.Show("Error polar");
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
                        if (Math.Abs(x) > 100000 || Math.Abs(yTemp) > 100000)
                            continue;
                        Points.Add(new Point(x, yTemp));
                    }
                    catch
                    {
                        MessageBox.Show("Error decart");
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