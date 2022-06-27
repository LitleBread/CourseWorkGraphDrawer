using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
        private  Dictionary<Graph, Style> graphStyleCoordination { get; set; }
        private double scaleFactor;
        private Point oldMousePosition;
        private Regex functionRegex = new Regex(@"[^0-9.\-a-zA-Z\(\)\*\^\+\\\&]");
        private Regex restrictionRegex = new Regex(@"[^0-9\.\-]");

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

        private bool IsFunctionTextAllowed(string text)
        {
            return functionRegex.IsMatch(text);
        }
        private bool IsRestrictionTextAllowed(string text)
        {
            return restrictionRegex.IsMatch(text);
        }
        private Point GetPointRelatively(MouseEventArgs e, IInputElement relElem, Point relativePoint)
        {
            return new Point(e.GetPosition(relElem).X - relativePoint.X, e.GetPosition(relElem).Y - relativePoint.Y);
        }
        private void ResetPositions()
        {
            scaleFactor = 48;
            zero = new Point(canvas.ActualWidth / 2, canvas.ActualHeight / 2);
            canvas.CalculatePointsPositions(zero, scaleFactor);
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

            (sender as Canvas).Cursor = Cursors.Arrow;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                (sender as Canvas).Cursor = Cursors.SizeAll;
                Point p = GetPointRelatively(e, sender as IInputElement, oldMousePosition);
                oldMousePosition = e.GetPosition(sender as IInputElement);
                zero = new Point(zero.X + p.X, zero.Y + p.Y);
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

            string func = FunctionTextBox.Text;
            bool isDec = (bool)Decart.IsChecked;
            
            try
            {
                double.TryParse(minVal.Text, out double min);
                double.TryParse(maxVal.Text, out double max);
                double.TryParse(stepVal.Text, out double step);

                Graph graph = new Graph(func, isDec, min, max, step);

                canvas.AddGraph(graph, style);
                canvas.CalculatePointsPositions(zero, scaleFactor);

                graphStyleCoordination.Add(graph, style);
                GraphsList.ItemsSource = null;
                GraphsList.ItemsSource = graphStyleCoordination;

                FunctionTextBox.Text = string.Empty;
            }
            catch(ParserException exc)
            {
                ErrorMessageTBLock.Text = exc.Message;
            }
            catch
            {
                ErrorMessageTBLock.Text = "Неверно введена функция";
            }
        }
        private void OnFunctionTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OnDrawButtonClick(null, null);
            }
        }
        private void OnGrapghGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            oldMousePosition = e.GetPosition(sender as IInputElement);
        }
        private void OnGrapghGridMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double sf = 1 + e.Delta / 720.0;
            scaleFactor *= sf;

            Point mousePositionInWindowCoordinates = e.GetPosition(sender as IInputElement);

            zero = new Point((zero.X - mousePositionInWindowCoordinates.X) * sf + mousePositionInWindowCoordinates.X, 
                (zero.Y - mousePositionInWindowCoordinates.Y) * sf + mousePositionInWindowCoordinates.Y);
            canvas.CalculatePointsPositions(zero, scaleFactor);
        }
        private void OnClearGraphListButtonClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in graphStyleCoordination)
            {
                canvas.HideGraph(item.Key);
                canvas.RemoveGraph(item.Key);
            }
            canvas.HideIntersections();
            graphStyleCoordination.Clear();
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
                FunctionTextBox.Text = ((KeyValuePair<Graph, Style>)GraphsList.SelectedItem).Key.Function;
                canvas.HideIntersections();
                canvas.ShowIntersections(((KeyValuePair<Graph, Style>)GraphsList.SelectedItem).Key);
                canvas.CalculatePointsPositions(zero, scaleFactor);
            }
        }
        private void OnGraphToShowCheckBoxCheckChanges(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(bool)(sender as CheckBox).IsChecked && (sender as CheckBox).IsChecked != null)
                {
                    canvas.HideGraph(((KeyValuePair<Graph, Style>)((sender as CheckBox).Parent as StackPanel).DataContext).Key);
                }
                else
                {
                    canvas.ShowGraph(((KeyValuePair<Graph, Style>)((sender as CheckBox).Parent as StackPanel).DataContext).Key);
                }
            }
            catch
            {

            }
        }
        private void OnResetButtonMouseDown(object sender, MouseButtonEventArgs e)
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

                string path = Environment.CurrentDirectory +"\\image" + DateTime.Now.ToString("yyyy,MM,ddHH,mm,ss") + ".jpeg";

                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 1 / 96, 1 / 96, PixelFormats.Pbgra32);
                    bmp.Render(canvas);
                    BitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    encoder.Save(fs);
                    Clipboard.SetDataObject(encoder.Frames[0], true);
                }
                MessageBox.Show("Saved");
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
                stepVal.Focus();
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
        private void OnInfoButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(@"Программа поддерживает стандартные операции(+ - * / ^). 
Также поддерживаются такие операции как:
синус(sin), косинус(cos), такнгенс(tan), 
обратные от них(arcsin, arccos, arctan)
и их гиперболические версии(sinh, cosh, tanh), 
логарифм по основанию е(log), 
логарифм по основанию 10(lg),
взятие квадратного корня числа(sqrt), 
взятие модуля числа(abs).
Если необходимо построить график типа операция(-значение), 
то нужно заменить ""-значение"" на ""0-значение"", 
иначе построение может быть некорректным
======Примеры графиков======
cos(x) - косинус
x^2 или x*x - порабола
1 / x - гипербола
abs(0-x)
sqrt(abs(cos(x))) * cos(300x) + sqrt(abs(x)) - 0.7 - сердечко", "Справка", MessageBoxButton.OK, MessageBoxImage.Question);
        }
        private void OnFunctionPreviewInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsFunctionTextAllowed(e.Text);
        }
        private void OnRestrictionPreviewInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsRestrictionTextAllowed(e.Text);
        }
        private void OnGraphTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                canvas.CalculatePointsPositions(zero, scaleFactor);
            }
        }

        private void OnGraphTextChanged(object sender, TextChangedEventArgs e)
        {
            canvas.CalculatePointsPositions(zero, scaleFactor);
        }

        private void OnMWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvas.CalculatePointsPositions(zero, scaleFactor);
        }
    }
}