using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CourseWorkGraphDrawer
{
    
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void OnconfirmButtonClick(object sender, RoutedEventArgs e)
        {
            switch (targetComboBox.SelectedIndex)
            {
                case 0:
                    Settings.BackgroundColor = (Color)ColorPicker.SelectedColor;
                    break;
                case 1:
                    Settings.AxisColor = (Color)ColorPicker.SelectedColor;
                    break;
                case 2:
                    Settings.IntersectionsdColor = (Color)ColorPicker.SelectedColor;
                    break;
                case 3:
                    Settings.GridColor = (Color)ColorPicker.SelectedColor;
                    break;
                default:
                    MessageBox.Show("Не выбран элемент для изменения");
                    break;
            }
            MessageBox.Show("Изменено");
            this.Close();

        }
    }
}
