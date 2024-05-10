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
        bool RobotRun = false;
        bool RobotRun2 = false;
        bool _isServerConnected;
        // string secCode = "SBER"; //VTBR   SBER
        // string classCode;
        // string clientCode;//= "S00B4L3"; 
        // private int pozit;
        private Tool Sber, Vtbr, Rosn;
        private decimal TopLine, CentrLine, BottomLine; // линии индикатора 

        private MyTick SBER;
        private MyTick VTBR;
        private MyTick RIM4;
        private List<MyTick> listMyTicks = new List<MyTick>();
        public List<Tool> ListTool = new List<Tool>();
        private Tool _tool;
        private string SC;
        //Window2 WndStrateg = new Window2(); 
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

        public void LogTik(string str)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    // TextBoxTik.AppendText(str + Environment.NewLine);
                    // TextBoxTik.ScrollToLine(TextBoxTik.LineCount - 1); 
                });

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
                //_quik = new Quik(34136, new InMemoryStorage());    // отладочный вариант
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
                    _isServerConnected = _quik.Service.IsConnected().Result;
                    if (_isServerConnected)
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

            // //_quik.Candles.Subscribe(classCode,secCode, CandleInterval.M1);
            // _quik.Candles.NewCandle += Candles_NewCandle;
            //_quik.Events.OnQuote += Events_OnQuote; 
            //_quik.Events.OnOrder += Events_OnOrder;
            //_quik.Events.OnStopOrder += Events_OnStopOrder;
            //_quik.Events.OnTransReply += Events_OnTransReply;
            // _quik.Events.OnAllTrade += Events_OnAllTrade;
            // _quik.Events.OnTrade += Events_OnTrade; 
            // _quik.Events.OnDepoLimit += Events_OnDepoLimit;
            // _quik.Events.OnMoneyLimit += Events_OnMoneyLimit;
            // _quik.Events.OnParam += Events_OnParam;
        }

        

        private void Events_OnTrade(Trade trade)
        {//https://youtu.be/vVehZG3trQ4?si=axTF5vwzTvpA4MMA
               
        }

        private void Events_OnTransReply(TransactionReply transReply)
        {//https://youtu.be/vVehZG3trQ4?si=axTF5vwzTvpA4MMA
            if (transReply.Status == 0) Log("Status " + transReply.Status + " Транзакция отправлена серверу");
            if (transReply.Status == 1) Log("Status " + transReply.Status + " Транзакция получена на сервер");
            if (transReply.Status == 2) Log("Status " + transReply.Status + " Ошибка при передаче Транзакции");
            if (transReply.Status == 3)
            {
                Log("Транзакция № - " + transReply.TransID + " Выставлен ордер № "+transReply.OrderNum + " Цена: "+ transReply.Price+ " Объём: " + transReply.Quantity);
            }
            if (transReply.Status > 3)
            {
                Log("ОШИБКА " + transReply.TransID + " Тест ошибки " + transReply.ResultMsg);
            }

        }

        private void Events_OnMoneyLimit(MoneyLimitEx mLimit)
        { 
            //Log("mLimit - изм. поз. по деньгам. Текущий ост. по деньгам: "+mLimit.CurrentBal+" Входящий ост. по деньгам"+ mLimit.OpenBal + " Заблокировано" + mLimit.Locked);
        }

        private void Events_OnDepoLimit(DepoLimitEx dLimit)
        {
            //Log("dLimit - изм. поз. по бумагам. Текущий ост. по бумагам: " + dLimit.SecCode+" Текущ. ост."+dLimit.CurrentBalance + " Средняя цена сделки" +dLimit.AweragePositionPrice);
        }

        async void Events_OnParam(Param par) // все изменения по инструменту
        { 
            // if (par.SecCode == SBER.NameTool)
            //     await ChengеParam(SBER, par).ConfigureAwait(true);
            // if (par.SecCode == VTBR.NameTool)
            //     await ChengеParam(VTBR,par).ConfigureAwait(true);
            // if (par.SecCode == secCode)
            // {
            //     var OI = _quik.Trading.GetParamEx(par.ClassCode,par.SecCode,ParamNames.NUMCONTRACTS).Result.ParamImage;
            //     var Kol_pok = _quik.Trading.GetParamEx(par.ClassCode, par.SecCode, ParamNames.NUMBIDS).Result.ParamImage;
            //     var Kol_pro = _quik.Trading.GetParamEx(par.ClassCode, par.SecCode, ParamNames.NUMOFFERS).Result.ParamImage;
            //     //Log(OI+" | "+Kol_pok + " | " +Kol_pro);
            // }

        }

        async Task ChengеParam(MyTick Mt, Param par)
        { 
            Mt.Kol_Pok = _quik.Trading.GetParamEx(par.ClassCode, par.SecCode, ParamNames.NUMBIDS).Result.ParamImage;
            Mt.Kol_Prd = _quik.Trading.GetParamEx(par.ClassCode, par.SecCode, ParamNames.NUMOFFERS).Result.ParamImage;
            this.Dispatcher.Invoke(() => DataGridTool.Items.Refresh());
        }

        private void Events_OnAccountPosition(AccountPosition accPos)
        {
            throw new NotImplementedException();
        }

        private void Events_OnAccountBalance(AccountBalance accBal)
        {
            throw new NotImplementedException();
        }

        private void Events_OnQuote(OrderBook orderbook)
        {
            if (orderbook.sec_code == ListTool[DataGridTool.SelectedIndex].SecurityCode)
            {
                var bestBuy = orderbook.bid[orderbook.bid.Length - 1];
                var bestSell = orderbook.offer[0];
                //Log("bestBuy - " + bestBuy.price +" = " + bestBuy.quantity + " bestSell - " + bestSell.price + " = " + bestSell.quantity);
            } 
        }

        async void Events_OnAllTrade(AllTrade allTrade) //все обезличенные сделки
        { 
            foreach (var SC in listMyTicks)
            {
                if (allTrade.SecCode == SC.NameTool)
                    await LogChengеTickForEvents_OnAllTrade(SC, allTrade).ConfigureAwait(false);
            } 
        } 

        async Task OnAllTrade(AllTrade allTrade)
        {
            // string flag;
            // if (allTrade.Flags == (AllTradeFlags)1026)
            // {
            //     flag = "Buy";
            // }
            // else
            // {
            //     flag = "Sell";
            // }

            //await Task.Run(() =>LogTik("TradeNum - " + allTrade.TradeNum + ", SecCode - " + allTrade.SecCode + ", Price - " + allTrade.Price+ ", Количество - " + allTrade.Qty+ ", Flags - " + flag));


        }

        async Task LogChengеTickForEvents_OnAllTrade(MyTick myTick, AllTrade allTrade)
        {
            myTick.NameTool = allTrade.SecCode;
            myTick.Time = allTrade.Datetime.hour.ToString("00") + ":" + allTrade.Datetime.min.ToString("00") + ":" + allTrade.Datetime.sec.ToString("00");
            myTick.Price = (decimal)allTrade.Price;
            myTick.Volume = (int)allTrade.Qty;
            myTick.Id = allTrade.TradeNum;
            myTick.OI = (int)allTrade.OpenInterest;
            if (allTrade.Flags == (AllTradeFlags)1026) myTick.Buy_Sell = "Buy"; else myTick.Buy_Sell = "Sell"; 
            this.Dispatcher.Invoke(() => DataGridTool.Items.Refresh());
        }

        async void MethodName()
        {
            // await Task.Run(() => { тут метод/задачка   });
            // await Task.Delay(500);
        }

        private void Candles_NewCandle(Candle candle)
        {
            LogNewCandle(candle);
        }

        private void LogNewCandle(Candle candle)
        {
            foreach (var SC in listMyTicks)
            {
                if (candle.SecCode == SC.NameTool)
                {
                    Log("Свечка № - " + candle.SecCode + " Datetime - " + TimeToString(candle.Datetime) + " - " + candle.ToString());

                    // индикатор
                    /*if (candle.SecCode == secCode)
                    {
                        string tag = "IND";
                        var N = testMethod()GetNumCandles(tag).Result;
                        TopLine = _quik.Candles.GetCandles(tag, 0, N - 2, 2).Result[0].Close;
                        CentrLine = _quik.Candles.GetCandles(tag, 1, N - 2, 2).Result[0].Close;
                        BottomLine = _quik.Candles.GetCandles(tag, 2, N - 2, 2).Result[0].Close;
                        Log(tag + " - " + N.ToString() + " - " + TopLine + " - " + CentrLine + " - " + BottomLine);

                        if (RobotRun)
                        {
                            if (NotInMarket())
                            {
                                StopLoss(TopLine,Operation.Buy);
                                StopLoss(BottomLine, Operation.Sell);
                                Log("ЗАЯВКИ РАССТАВЛЕНЫ");
                            }
                            else
                            {
                                Log("Я в рынке");
                            }
                        } 
                    }*/
                }
                
            } 
        }

        /*
        private bool NotInMarket()
        {
            var rez = true;

            var KolLotov = _quik.Trading.GetDepoLimits(secCode).Result[1].CurrentBalance / tool.Lot; 

            if (KolLotov != 0)
            {
                rez = false;
            }

            
            var orders = _quik.Orders.GetOrders(classCode, secCode).Result;
            foreach (var order in orders)
            {
                if (order.State == State.Active)
                {
                    rez = false; 
                }
            }
            var Stoporders = _quik.StopOrders.GetStopOrders(classCode, secCode).Result;

            foreach (var stoporder in Stoporders)
            {
                if (stoporder.State == State.Active)
                {
                    rez = false; 
                }
            }
            /*#1#

            return rez;
        }*/
        private void Timer_Elapsed() // для включения робота в нужное время, проти по ссылке доделать функцию
        {//https://youtu.be/0C_CajSlGoI?si=0vHdRY1nl8iKFeYo&t=1147
            DateTime t0 = DateTime.Now;
            DateTime t1 = new DateTime(); 
            DateTime t2 = new DateTime();
        }

        private string TimeToString(QuikDateTime T)
        {
            string str = T.hour.ToString("00") + ":" + T.min.ToString("00") + ":" + T.sec.ToString("00");
            return str; 
        }

        private void Events_OnStopOrder(StopOrder stopOrder)
        {
            Log("Стоп-Ордер № - " + stopOrder.OrderNum + ", " + stopOrder.Operation + ", TransID - " + stopOrder.TransId + ", State - " + stopOrder.State);
            
            /*
            Log("Account= " + stopOrder.Account + Environment.NewLine
                                  + "Comment= " + stopOrder.Comment + Environment.NewLine
                                  + "TransId= " + stopOrder.TransId + Environment.NewLine
                                  + "State= " + stopOrder.State + Environment.NewLine
                                  + "ClassCode= " + stopOrder.ClassCode + Environment.NewLine
                                  + "Spread= " + stopOrder.Spread + Environment.NewLine
                                  + "Offset= " + stopOrder.Offset + Environment.NewLine
                                  + "ConditionPrice2= " + stopOrder.ConditionPrice2 + Environment.NewLine
                                  + "Operation= " + stopOrder.Operation + Environment.NewLine 
                                  + "Quantity= " + stopOrder.Quantity + Environment.NewLine
                                  + "ClientCode= " + stopOrder.ClientCode + Environment.NewLine
                                  + "Condition= " + stopOrder.Condition + Environment.NewLine
                                  + "ConditionInt= " + stopOrder.ConditionInt + Environment.NewLine
                                  + "ConditionPrice= " + stopOrder.ConditionPrice + Environment.NewLine
                                  + "FilledQuantity= " + stopOrder.FilledQuantity + Environment.NewLine
                                  + "Flags= " + stopOrder.Flags + Environment.NewLine
                                  + "IsWaitingActivation= " + stopOrder.IsWaitingActivation + Environment.NewLine
                                  + "LinkedOrder= " + stopOrder.LinkedOrder + Environment.NewLine
                                  + "LuaTimeStamp= " + stopOrder.LuaTimeStamp + Environment.NewLine
                                  + "OffsetUnit= " + stopOrder.OffsetUnit + Environment.NewLine
                                  + "OrderNum= " + stopOrder.OrderNum + Environment.NewLine
                                  + "Price= " + stopOrder.Price + Environment.NewLine
                                  + "SecCode= " + stopOrder.SecCode + Environment.NewLine
                                  + "SpreadUnit= " + stopOrder.SpreadUnit + Environment.NewLine
                                  + "StopOrderType= " + stopOrder.StopOrderType + Environment.NewLine
                                  + "StopOrderTypeInt= " + stopOrder.StopOrderTypeInt + Environment.NewLine);
        */
        }

        private void Events_OnOrder(Order order)
        {
            if (order.TransID != 0) //фильтр дублированных сообщений
                Log("Ордер № - " + order.OrderNum + " " + order.Operation + " TransID - " + order.TransID + ", State - " + order.State);
            if (order.TransID != 0 && order.State == State.Completed && order.Operation == Operation.Buy)
            {
                //SellTakeProfit(order.AwgPrice);
            } 
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
         
        private void ButtonRobot_Click(object sender, RoutedEventArgs e)
        {
            if (!RobotRun)
            {
                Log("RobotRun = true");
                ButtonRobot.Background = Brushes.Aqua;
                RobotRun = true;
            }
            else 
            {
                Log("RobotRun = false");
                closeallpositionsFunc(Sber);
                ButtonRobot.Background = Brushes.LightGray;
                RobotRun = false;
            } 
        } 
        private void ButtonRobot2_Click(object sender, RoutedEventArgs e)
        {
            if (!RobotRun2)
            {
                Log("RobotRun = true");
                ButtonRobot2.Background = Brushes.Aqua;
                RobotRun2 = true;
            }
            else
            {
                Log("RobotRun = false");
                closeallpositionsFunc(Sber);
                ButtonRobot2.Background = Brushes.LightGray;
                RobotRun2 = false;
            }
        }

        async Task StopLoss(Tool tool, decimal price, Operation Bue_Sell)
        {
            https://youtu.be/HWpMYBZCUU4?si=6-wo40ymV2TQ1jF9

            decimal pr; Condition UppLou;
            
            if (Bue_Sell == Operation.Sell) //если цена идет против то надо продать немного ниже последней цены
            {
                UppLou = Condition.LessOrEqual;
                pr = price - (price * (decimal)0.00002);
            }
            else                            //если цена идет против то надо купить немного выше последней цены
            {
                UppLou = Condition.MoreOrEqual;
                pr = price + (price * (decimal)0.00002);
            }

            StopOrder stopOrder = new StopOrder()
            { 
                ClientCode = tool.СlientCode,
                Account = tool.AccountID,
                ClassCode = tool.ClassCode,
                SecCode = tool.SecurityCode, 
                StopOrderType = StopOrderType.StopLimit ,
                Condition = UppLou,
                ConditionPrice = Math.Round(price, tool.PriceAccuracy),
                Price = Math.Round(pr, tool.PriceAccuracy),
                Operation = Bue_Sell,
                Quantity = 1,
            };

            try
            {
                await _quik.StopOrders.CreateStopOrder(stopOrder).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Log("Ошибка покупки ... " + exception);
            }

        }
        public async Task TakeProfit_StopLoss(Tool tool, int quty, decimal price, Operation Bue_Sell, StopOrderType sot)
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
                    Offset = (decimal)0.01,//Math.Round(5 * tool.Step, tool.PriceAccuracy),
                    OffsetUnit = OffsetUnits.PERCENTS,
                    Spread = (decimal)0.01,//Math.Round(1 * tool.Step, tool.PriceAccuracy),
                    SpreadUnit = OffsetUnits.PERCENTS,
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

        private void AddLabel(decimal price)// ни**ра кроме удаления не работает
        {
            /*Label label_params = new Label
            {
                Text = "какой то текст",
                Alignment = "LEFT",
                StrDate = DateTime.Now.ToString("HH:mm:ss.ff"),
                //StrTime = TimeToString(),
                Red = "0",
                Green = "0",
                Blue = "0",
                Transparency = "90",
                FontHeight = "10",
                TranBackgrnd = "1",
                YValue = String.Empty,
                Hint = "texthint"
            };*/
            //("какой то текст", "LEFT", "data", "time", 0, 0, 0, 90, 10, price, "hint"); 

            var TAG = "IND";
            //var t =_quik.Service.AddLabel((double)price, DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString()
            //    , "hint","", "IND","LEFT",0);
           var gl =  _quik.Service.AddLabel("IND", price, DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString()
                ," string text   ", "string imagePath   ", "string alignment   ", "string hint   ",
             -1,  -1,  -1,  -1, -1, "string fontName   ",  -1);
           var result = _quik.Service.GetLabelParams(TAG, gl.Id).Result;
           //_quik.Service.DelAllLabels(TAG);
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

            Log("Kill All Orders");
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

            var window = new Window2 ();
            window.TextBoxName.Text = ListTool[DataGridTool.SelectedIndex].SecurityCode;
            window.TextBoxprice.Text = ListTool[DataGridTool.SelectedIndex].LastPrice.ToString();
            window.ShowDialog();
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
