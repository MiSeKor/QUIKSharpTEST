using QuikSharp;
using QuikSharp.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using Condition = QuikSharp.DataStructures.Condition;

namespace DemoTestWPF
{
    /// <summary>
    /// Логика взаимодействия для Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        MainWindow wnd = (MainWindow)App.Current.MainWindow;
        Strategy strategy = new Strategy();
        Tool GetTool ; 
        public Window2()
        {
            InitializeComponent();  
        }
         
        private void TextBoxStep_TextChanged(object sender, TextChangedEventArgs e)
        {

            // textBox1.TextChanged += TextBoxOnTextChanged;
            // textBox2.TextChanged += TextBoxOnTextChanged;
            // textBox3.TextChanged += TextBoxOnTextChanged;
            if (sender is TextBox textBox)
            {
                textBox.Text = new string
                (
                    textBox
                        .Text
                        .Where
                        (ch => (ch >= '0' && ch <= '9') || ch == ',')
                        .ToArray()
                );
                textBox.SelectionStart = e.Changes.First().Offset + 1;
                textBox.SelectionLength = 0;
            }


        }


        private void TextBoxStep_PreviewKeyUp(object sender, KeyEventArgs e)
        {

        }
 
        private void ButtonStrategy_Click_1(object sender, RoutedEventArgs e)
        {
             
            foreach (var t in wnd.ListTool)
            {
                if (t.SecurityCode == TextBoxSecCode.Text)
                {
                    GetTool = t;
                } 
            }

            if (ButtonStrategy.Content.ToString() == "STOP")
            {
                
               var S = strategy.Create_Strategy(GetTool
                    , ComboBoxBuySel.SelectedValue.ToString()
                    , TextBoxprice.Text, TextBoxQuantity.Text, TextBoxLevel.Text
                    , TextBoxStep.Text, TextBoxCels.Text);
                strategy.IsActive = true;
                ButtonStrategy.Content = "RUN";
                ButtonStrategy.Background = Brushes.Aqua;
                 runStrategyTask(S);

            }
            else// if (ButtonStrategy.Content == "RUN")
            {
                wnd.KillAllOrdersFunc(wnd.ListTool[wnd.DataGridTool.SelectedIndex]);
                strategy.IsActive = false;
                ButtonStrategy.Content = "STOP";
                ButtonStrategy.Background = Brushes.Crimson;
            
            }

            
        }
        async Task runStrategyTask(Strategy strategy)
        {
            var flag = true;
            decimal otstup = 0;
            while (IsActive)
            {  
                 decimal pr = strategy.Price;
                //Application.Current.Dispatcher.Invoke(new Action(() => { wnd.Log(s);}));
                if (strategy.Operation == Operation.Buy)
                {

                    for (int n = 0; n < strategy.Levels; n++)
                    {
                        StopOrder stopOrder = new StopOrder()
                        {
                            ClientCode = strategy.StrTool.СlientCode,
                            Account = strategy.StrTool.AccountID,
                            ClassCode = strategy.StrTool.ClassCode,
                            SecCode = strategy.StrTool.SecurityCode,
                            Offset = (decimal)0.01,//Math.Round(5 * tool.Step, tool.PriceAccuracy),
                            OffsetUnit = OffsetUnits.PERCENTS,
                            Spread = (decimal)0.01,//Math.Round(1 * tool.Step, tool.PriceAccuracy),
                            SpreadUnit = OffsetUnits.PERCENTS,
                            StopOrderType = StopOrderType.TakeProfit,
                            Condition = Condition.LessOrEqual,
                            ConditionPrice = Math.Round(pr, strategy.StrTool.PriceAccuracy),
                            Operation = strategy.Operation,
                            Quantity = strategy.Quantity,
                        };

                        await wnd._quik.StopOrders.CreateStopOrder(stopOrder).ConfigureAwait(false);
                        // await Task.Run(() => { тут метод/задачка   });
                        // await Task.Delay(500);
                        //wnd.Dispatcher.Invoke(new Action(() => {}));
                        //await Task.Run(() => { }); 
 
                        //     await Task.Run(() => {  });
                        if (flag)
                        {
                            otstup = pr * strategy.Step ;
                            var otstup1 = (otstup % strategy.StrTool.Step);
                            if (otstup1 != 0) otstup = otstup - otstup1; 
                            flag = false;
                        }

                        pr = pr - otstup;
                        // var pr11 = (pr % strategy.Step);
                        // if (pr11 != 0) pr = pr - pr11;
                        //pr = pr - strategy.Step;
                        //WndStrateg.Title = pr.ToString();
                         await Task.Delay(200);
                    };
                }
            }
        }
    }
}
