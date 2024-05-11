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

namespace DemoTestWPF
{
    //      https://metanit.com/sharp/wpf/20.2.php Взаимодействие между окнами
    /// <summary>
    /// Логика взаимодействия для TaskWindow.xaml
    /// </summary>
    public partial class TaskWindow : Window
    {
        //private MainWindow mainWindow = new MainWindow();
        public string ViewModel { get; set; } = "TEXT";
        public TaskWindow()
        {
            InitializeComponent();
        }
        public void ShowViewModel()
        {
            MessageBox.Show(ViewModel);
        }
        public void ChageOwnerBackground()
        {

        }
    }
}
