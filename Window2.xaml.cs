using QuikSharp.DataStructures;
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
    /// <summary>
    /// Логика взаимодействия для Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();
        }

        public void Create_Strategy(Operation BuySel, decimal price, int quantity, int level, int step, decimal cels)
        {
            Operation o;
            if (BuySel != null)
            {
                Strategy strategy = new Strategy()
                {
                    Operation = BuySel,
                    Price = Convert.ToDecimal(TextBoxprice),
                    Quantity = Convert.ToInt32(TextBoxQuantity),
                    Levels = Convert.ToInt32(TextBoxLevel),
                    Step = Convert.ToInt32(TextBoxStep),
                    Cels = Convert.ToDecimal(TextBoxCels)
                };
            }

        } 
    }
}
