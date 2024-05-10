using QuikSharp;
using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
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
        private Strategy S;
        object locker = new object();
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
                
                 strategy.Create_Strategy(GetTool
                    , ComboBoxBuySel.SelectedValue.ToString()
                    , TextBoxprice.Text, TextBoxQuantity.Text, TextBoxLevel.Text
                    , TextBoxStep.Text, TextBoxCels.Text);
                 strategy.IsActive = true;
                ButtonStrategy.Content = "RUN";
                ButtonStrategy.Background = Brushes.Aqua;
                 runStrategyTask(strategy);
                 S = strategy; 
            }
            else// if (ButtonStrategy.Content == "RUN")
            {
                wnd.KillAllOrdersFunc(GetTool);
                strategy.IsActive = false;
                ButtonStrategy.Content = "STOP";
                ButtonStrategy.Background = Brushes.Crimson;
            }
        }

        //var condition = order.Operation == Operation.Sell ? Condition.MoreOrEqual : Condition.LessOrEqual;
        // private void UpdateLabelStrategy()
        // {
        //     string content = strategy.IsActive ? "Активна" : "Неактивна";
        //
        //     Action action = () => LabelStrategyStatus.Content = content;
        //     Dispatcher.Invoke(action);
        // }
        private async Task<long> CreateStopOrder(decimal pr, Strategy strategy)
        {
            StopOrder stopOrder = new StopOrder()
            {
                ClientCode = strategy.StrTool.СlientCode,
                Account = strategy.StrTool.AccountID,
                ClassCode = strategy.StrTool.ClassCode,
                SecCode = strategy.StrTool.SecurityCode,
                Offset = 0,
                OffsetUnit = OffsetUnits.PRICE_UNITS,
                Spread = 0,
                SpreadUnit = OffsetUnits.PRICE_UNITS,
                StopOrderType = StopOrderType.TakeProfit,
                Condition = Condition.LessOrEqual,
                ConditionPrice = Math.Round(pr, strategy.StrTool.PriceAccuracy),
                Operation = strategy.Operation,
                Quantity = strategy.Quantity,
            };
            var so = await wnd._quik.StopOrders.CreateStopOrder(stopOrder).ConfigureAwait(false);
            return so;
        }

        private async Task<List<long>> SetUpNetwork(Strategy strategy)
        {
            List<long> ListTrId = new List<long>();
            bool flag = true;
            decimal otstup = 0;
            decimal pr = strategy.Price;
            for (int n = 0; n < strategy.Levels; n++)
            {
                //Application.Current.Dispatcher.Invoke(new Action(() => { wnd.Log(s);}));
                // await Task.Run(() => { тут метод/задачка   });
                // await Task.Delay(500);
                //wnd.Dispatcher.Invoke(new Action(() => {}));
                //await Task.Run(() => { });  
                ListTrId.Add(CreateStopOrder(pr, strategy).Result);
                //     await Task.Run(() => {  });
                if (flag)
                {
                    otstup = pr * strategy.Step;
                    var otstup1 = (otstup % strategy.StrTool.Step);
                    if (otstup1 != 0) otstup = otstup - otstup1;
                    flag = false;
                }

                pr = pr - otstup;
                // var pr11 = (pr % strategy.Step);
                // if (pr11 != 0) pr = pr - pr11;
                //pr = pr - strategy.Step;
                //WndStrateg.Title = pr.ToString();
                //await Task.Delay(200);
            }

            return ListTrId;
        }

        public async Task<StopOrder> UpDatStopOrd(StopOrder stopOrder)
        {
            return stopOrder;
        }

        private async Task runStrategyTask(Strategy strategy)
        { 
            if (strategy.Operation == Operation.Buy)
            {
                strategy._ListTrId = SetUpNetwork(strategy).Result;

               while (strategy.IsActive)
               {
                   var listStopOrders = wnd._quik.StopOrders.GetStopOrders().Result;

                   if (listStopOrders.Count > 0 || listStopOrders.Count != strategy._ListTrId.Count)
                       foreach (StopOrder stopOrder in listStopOrders)
                       {
                           if (stopOrder.TransId != 0 || stopOrder.State == State.Completed)
                           {
                               //CreateStopOrder(stopOrder.Price, strategy);
                               await Task.Delay(2000);
                            }
                       }
               }
            }


        }

        private void WndStrateg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            wnd.KillAllOrdersFunc(GetTool);
            strategy.IsActive = false;
        }
 
    }
}
