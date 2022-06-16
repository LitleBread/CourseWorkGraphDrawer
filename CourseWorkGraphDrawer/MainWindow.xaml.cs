using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CourseWorkGraphDrawer
{
    public partial class MainWindow : Window
    {
        private Point zero;
        private readonly Dictionary<Graph, Style> graphStyleCoordination;
        private double scaleFactor;
        private Point oldPos;

        private void ResetPositions()
        {
            scaleFactor = 24;
            zero = new Point(canvas.ActualWidth / 2, canvas.ActualHeight / 2);
            canvas.SetAxis(zero);
            canvas.CalculatePointsPositions(zero, scaleFactor);
        }

        public MainWindow()
        {
            InitializeComponent();

            canvas = new GraphCanvas();


            mousePosTextBlock = new TextBlock();
            graphStyleCoordination = new Dictionary<Graph, Style>();
            DashStyleComboBox.ItemsSource = new List<string>() { "Сплошная", "Штриховая", "Пунктирная", "Штрих-пунктирная" };

            mousePosTextBlock.Foreground = Brushes.Black;
            mousePosTextBlock.FontWeight = FontWeights.Bold;
            mousePosTextBlock.FontSize = 14;
            mousePosTextBlock.SetValue(Canvas.ZIndexProperty, 2);

            resetButton.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\IconsAndProframmImages\\restart.png"));
            saveImageButton.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\IconsAndProframmImages\\saveImage.png"));

            canvas.Children.Add(mousePosTextBlock);
            MainGrid.Children.Add(canvas);

            canvas.SetValue(Grid.ColumnProperty, 0);
            //canvas.SetValue(Canvas.ZIndexProperty, 2);

            DashStyleComboBox.SelectedIndex = 0;
            ThicknessTextBox.Text = "2";

            canvas.Background = Brushes.AntiqueWhite;
            canvas.MouseDown += OnGrapghGridMouseDown;
            canvas.MouseMove += OnGrapghPlaneMouseMove;
            canvas.MouseWheel += OnGrapghGridMouseWheel;
        }
        private Point GetPointRelatively(MouseEventArgs e, IInputElement relElem, Point relativePoint)
        {
            return new Point(e.GetPosition(relElem).X - relativePoint.X, e.GetPosition(relElem).Y - relativePoint.Y);
        }
        private void OnGrapghPlaneMouseMove(object sender, MouseEventArgs e)
        {
            Point MousePositionRelativeToGraph = GetPointRelatively(e, sender as IInputElement, zero);
            MousePositionRelativeToGraph.X /= scaleFactor;
            MousePositionRelativeToGraph.Y /= -scaleFactor;
            Point mPos = e.GetPosition(sender as IInputElement);
            mousePosTextBlock.SetValue(Canvas.LeftProperty, mPos.X - 55);
            mousePosTextBlock.SetValue(Canvas.TopProperty, mPos.Y - 20);
            mousePosTextBlock.Text = string.Format("{0:N4} ; {1:N4}", MousePositionRelativeToGraph.X, MousePositionRelativeToGraph.Y);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = GetPointRelatively(e, sender as IInputElement, oldPos);
                oldPos = e.GetPosition(sender as IInputElement);
                zero = new Point(zero.X + p.X, zero.Y + p.Y);
                canvas.SetAxis(zero);
                canvas.CalculatePointsPositions(zero, scaleFactor);
            }
        }

        private void OnDrawButtonClick(object sender, RoutedEventArgs e)
        {
            double.TryParse(ThicknessTextBox.Text.Replace(".", ","), out double thickness);

            Color color;
            if (ColorPicker.SelectedColor != null)
            {
                color = (Color)ColorPicker.SelectedColor;
            }
            else
            {
                color = Colors.Blue;
            }

            Style style = new Style(thickness, DashStyleComboBox.Text, color, true);

            string func = GraphEnterTBox.Text;
            bool isDec = (bool)Decart.IsChecked;

            try
            {
                Graph graph = new Graph(func, isDec, minVal.Text, maxVal.Text, step.Text);
                canvas.AddGraph(graph, style, zero, scaleFactor);
                graphStyleCoordination.Add(graph, style);
                GraphsList.ItemsSource = null;
                GraphsList.ItemsSource = graphStyleCoordination;


                GraphEnterTBox.Text = string.Empty;
            }
            catch
            {
                ErrorMessageTBLock.Text = "Неверно введена функция";
            }
        }

        private void OnGrapghGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            oldPos = e.GetPosition(sender as IInputElement);
        }

        private void OnGrapghGridMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double sf = 1 + e.Delta / 720.0;
            scaleFactor *= sf;

            Point p1 = e.GetPosition(sender as IInputElement);

            zero = new Point((zero.X - p1.X) * sf + p1.X, (zero.Y - p1.Y) * sf + p1.Y);
            canvas.SetAxis(zero);
            canvas.CalculatePointsPositions(zero, scaleFactor);
        }

        private void OndeleteGraphBtnClick(object sender, RoutedEventArgs e)
        {
            if (GraphsList.SelectedItem == null)
            {
                return;
            }
            canvas.RemoveGraph(((KeyValuePair<Graph, Style>)GraphsList.SelectedItem).Key);
            graphStyleCoordination.Remove(((KeyValuePair<Graph, Style>)GraphsList.SelectedItem).Key);
            GraphsList.ItemsSource = null;
            GraphsList.ItemsSource = graphStyleCoordination;
        }

        private void OnGraphsListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (GraphsList.SelectedItem == null)
                {
                    return;
                }
                canvas.RemoveGraph(((KeyValuePair<Graph, Style>)GraphsList.SelectedItem).Key);
                graphStyleCoordination.Remove(((KeyValuePair<Graph, Style>)GraphsList.SelectedItem).Key);
                GraphsList.ItemsSource = null;
                GraphsList.ItemsSource = graphStyleCoordination;
            }
        }

        private void OnGraphsListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GraphsList.SelectedItem != null)
            {
                GraphEnterTBox.Text = ((KeyValuePair<Graph, Style>)GraphsList.SelectedItem).Key.Function;
                canvas.HideIntersections();
                canvas.ShowIntersections(((KeyValuePair<Graph, Style>)GraphsList.SelectedItem).Key);
                canvas.CalculatePointsPositions(zero, scaleFactor);
            }
        }

        private void OnGraphToShowCheckBoxCheckChanges(object sender, RoutedEventArgs e)
        {
            if (!(bool)(sender as CheckBox).IsChecked)
            {
                canvas.HideGraph(((KeyValuePair<Graph, Style>)((sender as CheckBox).Parent as StackPanel).DataContext).Key);
            }
            else
            {
                canvas.ShowGraph(((KeyValuePair<Graph, Style>)((sender as CheckBox).Parent as StackPanel).DataContext).Key);
            }
        }

        private void OnresetButtonMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                ResetPositions();
            }
        }

        private void OnSaveImageButtonMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {

                string path = "image " + DateTime.Now.ToString("yyyy/MM/dd HH/mm/ss") + ".jpeg";

                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 1 / 96, 1 / 96, PixelFormats.Pbgra32);
                    bmp.Render(canvas);
                    BitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    encoder.Save(fs);
                    Clipboard.SetDataObject(encoder.Frames[0], true);
                }
                MessageBox.Show("Coppied");
            }
        }

        private void OnMWindowLoaded(object sender, RoutedEventArgs e)
        {
            ResetPositions();
        }

        private void OnMinValKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D || e.Key == Key.Left)
            {
                maxVal.Focus();
            }
        }

        private void OnMaxValKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D || e.Key == Key.Left)
            {
                step.Focus();
            }
            else if (e.Key == Key.A || e.Key == Key.Right)
            {
                minVal.Focus();
            }
        }

        private void OnStepKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A || e.Key == Key.Right)
            {
                maxVal.Focus();
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorMessageTBLock.Text = string.Empty;
        }

        private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            settings.Activate();
            settings.Show();
        }
    }

}
