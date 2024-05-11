using DemoTestWPF.Properties;
using QuikSharp;
using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using System.Threading.Tasks;
using Condition = QuikSharp.DataStructures.Condition;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Shapes;
using Label = QuikSharp.DataStructures.Label;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;

namespace DemoTestWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //  Уроки C# – Потоки, Thread, Invoke, Action, delegate, Parallel.Invoke – C#
        //          https://youtu.be/vHqHrf914TA?si=uV1qaiKzyIDmCEXf
        //***********************************************************
        public Quik _quik; 

        private Tool Sber, Vtbr, Rosn; 

        public List<Tool> ListTool = new List<Tool>();
        private Tool _tool;
        private string SC; 

        public MainWindow()
        {
            InitializeComponent();
           
        }  

        public void Log(string str)
        {
            //TextBoxLog.AppendText(str + Environment.NewLine); 
            try
            {
                if (Dispatcher.CheckAccess())
                {
                    TextBoxLog.AppendText(DateTime.Now.ToString("HH:mm:ss.ff") + " - " + str + Environment.NewLine);
                    TextBoxLog.ScrollToLine(TextBoxLog.LineCount - 1);
                }
                else
                {
                    this.Dispatcher.Invoke(() => Log(str));
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
         
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log("Подключаемся к терминалу Quik...");
                _quik = new Quik(Quik.DefaultPort, new InMemoryStorage());    // инициализируем объект Quik
            }
            catch
            {
                Log("Ошибка инициализации объекта Quik.");
            }

            if (_quik != null)
            {
                Log("Экземпляр Quik создан." );
                try
                {
                    Log("Получаем статус соединения с сервером..." ); 

                    if (_quik.Service.IsConnected().Result)
                    {
                        Log("Соединение с сервером установлено." );
                        ButtonConect.Content = "Ok";
                        ButtonConect.Background = Brushes.Aqua;
                        Run();
                    }
                    else
                    {
                        Log("Соединение с сервером НЕ установлено." );
                        ButtonConect.Content = "НЕ Ok";
                        ButtonConect.Background = Brushes.Crimson;
                    }
                }
                catch
                {
                    Log("Неудачная попытка получить статус соединения с сервером." );
                }

            }
        }

        private void Run()
        { 
            Sber = new Tool(_quik, "SBER"); ListTool.Add(Sber);
            Vtbr = new Tool(_quik, "VTBR"); ListTool.Add(Vtbr);
              Rosn = new Tool(_quik, "ROSN"); ListTool.Add(Rosn);
             var Rual = new Tool(_quik, "RUAL"); ListTool.Add(Rual);
            DataGridTool.ItemsSource = ListTool;
            DataGridTool.Items.Refresh(); 
        } 

        // КНОПКИ
        private void ButtonBUY_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTool.SelectedIndex >= 0)
                MarketOrder(ListTool[DataGridTool.SelectedIndex], Operation.Buy);
        }
        private void ButtonSEll_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTool.SelectedIndex >= 0)
                MarketOrder(ListTool[DataGridTool.SelectedIndex], Operation.Sell);
        }

        private async Task MarketOrder(Tool tool,Operation operation)
        {
            try
            {
                await _quik.Orders.SendMarketOrder(tool.ClassCode, tool.SecurityCode, tool.AccountID, operation, 1, ExecutionCondition.PUT_IN_QUEUE, tool.СlientCode).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Log("Ошибка транзакции ... " + exception);
            }
        }

        private async Task LimitOrder(Tool tool, Operation operation)
        {
            try
            {
                if (operation == Operation.Buy)
                {
                    decimal pricein = Math.Round(tool.LastPrice - 100 * tool.Step, tool.PriceAccuracy); 
                    _quik.Orders.SendLimitOrder(tool.ClassCode, tool.SecurityCode, tool.AccountID, operation, pricein, 1, ExecutionCondition.PUT_IN_QUEUE, tool.СlientCode).ConfigureAwait(false);

                }
                else
                {
                    decimal pricein = Math.Round(tool.LastPrice + 100 * tool.Step, tool.PriceAccuracy);
                    _quik.Orders.SendLimitOrder(tool.ClassCode, tool.SecurityCode, tool.AccountID, operation, pricein, 1, ExecutionCondition.PUT_IN_QUEUE, tool.СlientCode).ConfigureAwait(false);

                }
                //Log("Ордер № - " + ttt.Result.OrderNum + " " + ttt.Result.Operation + " TransID - " + ttt.Result.TransID);
            }
            catch (Exception exception)
            {
                Log("Ошибка покупки ... " + exception);
            }
        }


        private void ButtonSellLimit_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTool.SelectedIndex >= 0)
                LimitOrder(ListTool[DataGridTool.SelectedIndex], Operation.Sell);
        }

        private void ButtonBuyLimit_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTool.SelectedIndex >= 0)
                LimitOrder(ListTool[DataGridTool.SelectedIndex], Operation.Buy);
        }  

        async Task TakeProfit_StopLoss(Tool tool, int quty, decimal price, Operation Bue_Sell, StopOrderType sot)
        {
            https://youtu.be/HWpMYBZCUU4?si=6-wo40ymV2TQ1jF9
            try
            {
                decimal pr1, pr2, pr3; Condition UppLou; 
                if (Bue_Sell == Operation.Buy)  //если купить то по наименьшей цэне
                {
                    UppLou = Condition.LessOrEqual;
                    pr1 = (tool.LastPrice - (tool.LastPrice * (decimal)0.01));// для ТейкПрофит
                    var pr11 =(pr1 % tool.Step);
                    if (pr11 != 0) pr1 = pr1 - pr11; 

                    pr2 = (tool.LastPrice + (tool.LastPrice * (decimal)0.001));  //для СтопЛос стоп цена, остановить убыток для шора
                    var pr22 =(pr2 % tool.Step);
                    if (pr22 != 0) pr2 = pr2 - pr22;

                    pr3 = pr2 + (pr2 * (decimal)0.0002);      // для СтопЛос цена покупки, остановить убыток для шора
                    var pr33 = (pr3 % tool.Step);
                    if (pr33 != 0) pr3 = pr3 - pr33;
                }
                else                            //если продать то по наибольшей цэне
                {
                    UppLou = Condition.MoreOrEqual;
                    pr1 = (tool.LastPrice + (tool.LastPrice * (decimal)0.01));// для ТейкПрофит
                    var pr11 = (pr1 % tool.Step);
                    if (pr11 != 0) pr1 = pr1 - pr11;

                    pr2 = (tool.LastPrice - (tool.LastPrice * (decimal)0.001));  //для СтопЛос стоп цена, остановить убыток для шора
                    var pr22 = (pr2 % tool.Step);
                    if (pr22 != 0) pr2 = pr2 - pr22;

                    pr3 = pr2 - (pr2 * (decimal)0.0002);      // для СтопЛос цена покупки, остановить убыток для шора
                    var pr33 = (pr3 % tool.Step);
                    if (pr33 != 0) pr3 = pr3 - pr33;
                }

                StopOrder stopOrder = new StopOrder()
                {
                    ClientCode = tool.СlientCode,
                    Account = tool.AccountID,
                    ClassCode = tool.ClassCode,
                    SecCode = tool.SecurityCode,
                    Offset = 0, 
                    OffsetUnit = OffsetUnits.PRICE_UNITS,
                    Spread = 0, 
                    SpreadUnit = OffsetUnits.PRICE_UNITS,
                    StopOrderType = sot,
                    Condition = UppLou,
                    ConditionPrice = Math.Round(pr1, tool.PriceAccuracy),
                    ConditionPrice2 = Math.Round(pr2, tool.PriceAccuracy), //не нужна для тей-профит
                    Price = Math.Round(pr3, tool.PriceAccuracy),  //не нужна для тей-профит
                    Operation = Bue_Sell,
                    Quantity = quty,
                };

                await _quik.StopOrders.CreateStopOrder(stopOrder).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Log("Ошибка покупки ... " + exception);
            } 
        }
         
        private void TeikProf_StopLos_Sell_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTool.SelectedIndex >= 0)
                TakeProfit_StopLoss(ListTool[DataGridTool.SelectedIndex],1, ListTool[DataGridTool.SelectedIndex].LastPrice, Operation.Sell, StopOrderType.TakeProfitStopLimit);
        }
        private void Button_TeikProf_StopLos_Buy_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTool.SelectedIndex >= 0)
                TakeProfit_StopLoss(ListTool[DataGridTool.SelectedIndex], 1, ListTool[DataGridTool.SelectedIndex].LastPrice, Operation.Buy, StopOrderType.TakeProfitStopLimit);
        }


        private void ButtonBuyTProf_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTool.SelectedIndex >= 0)
                TakeProfit_StopLoss(ListTool[DataGridTool.SelectedIndex], 1, ListTool[DataGridTool.SelectedIndex].LastPrice, Operation.Buy,StopOrderType.TakeProfit);
            //BuyTakeProfit(tool.LastPrice);
        }
        private void ButtonSell_TPro_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTool.SelectedIndex >= 0)
                TakeProfit_StopLoss(ListTool[DataGridTool.SelectedIndex], 1, ListTool[DataGridTool.SelectedIndex].LastPrice, Operation.Sell, StopOrderType.TakeProfit);
            //SellTakeProfit(tool.LastPrice);
        }

        private void KillAllOrders_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTool.SelectedIndex >= 0)
                KillAllOrdersFunc(ListTool[DataGridTool.SelectedIndex]);
        }

        public async Task KillAllOrdersFunc(Tool tool)
        {
            if (tool != null)
            {
                var orders = _quik.Orders.GetOrders(tool.ClassCode, tool.SecurityCode).Result;
                //await Task.Delay(1000);
                foreach (var order in orders)
                {
                    if (order.State == State.Active)
                    {
                        await _quik.Orders.KillOrder(order).ConfigureAwait(true);
                    }
                }
                  
                var Stoporders = _quik.StopOrders.GetStopOrders(tool.ClassCode, tool.SecurityCode).Result;
                //await Task.Delay(1000);
                foreach (var stoporder in Stoporders)
                {
                    if (stoporder.State == State.Active)
                    {
                       await _quik.StopOrders.KillStopOrder(stoporder).ConfigureAwait(false);
                    }
                }

                if (orders.Count!= 0 && Stoporders.Count != 0)Log("Kill All Orders");
            }

        }

        private void ButtonAddTool_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox1.Text != "" || TextBox1.Text != null)
                ListTool.Add(Сreate(_quik, TextBox1.Text));
            DataGridTool.ItemsSource = ListTool;
            DataGridTool.Items.Refresh();
            TextBox1.Text = "";
        }
        
        Tool Сreate(Quik quik, string securityCode)
        {
             _tool = new Tool(quik, securityCode);
            return _tool;
        }

        private void DataGridTool_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                var _selectIndex = DataGridTool.SelectedIndex;
                if (_selectIndex != null || _selectIndex >= 0)
                    Log(_selectIndex.ToString()
                    +" "+ ListTool[DataGridTool.SelectedIndex].SecurityCode
                    + " " + ListTool[DataGridTool.SelectedIndex].LastPrice.ToString()
                    + " " + ListTool[DataGridTool.SelectedIndex].СlientCode); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception); 
            }

        }

        private void DataGridTool_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        { 
        }


        private async Task closeallpositionsFunc(Tool tool)
        {
            var KolLot = _quik.Trading.GetDepo(tool.СlientCode, tool.FirmID, tool.SecurityCode, tool.AccountID).Result.DepoCurrentBalance / tool.Lot;
            //await Task.Delay(1000);

            if (KolLot != 0)
            {
                if (KolLot!= null && KolLot > 0)
                {
                    await _quik.Orders.SendMarketOrder(tool.ClassCode, tool.SecurityCode, tool.AccountID, Operation.Sell, (int)KolLot).ConfigureAwait(false);
                }
                else if (KolLot < 0)
                {
                    await _quik.Orders.SendMarketOrder(tool.ClassCode, tool.SecurityCode, tool.AccountID, Operation.Buy, (int)-KolLot).ConfigureAwait(false);
                }
            }
 
            Log("закрываетие = "+KolLot.ToString()+" позиций");
        }

        private void closeallpositions_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTool.SelectedIndex >= 0)
                closeallpositionsFunc(ListTool[DataGridTool.SelectedIndex]);
        }

        protected void OnClosing(ConsoleCancelEventArgs e)
        {
            if (_quik == null)
            {
                _quik.StopService();
                 this.OnClosing(e);
            }
        }


        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // SBER = new MyTick();
            // SBER.NameTool = "SBER";
            // VTBR = new MyTick();
            // VTBR.NameTool = "VTBR";
            // RIM4 = new MyTick();
            // RIM4.NameTool = "RIM4";
            // listMyTicks.Add(SBER);
            // listMyTicks.Add(VTBR);
            // listMyTicks.Add(RIM4);
            //DataGridTool.ItemsSource = listMyTicks;
            //DataGridTool.Items.Refresh();
        }

    }
}
