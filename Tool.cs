﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using DemoTestWPF;
using Microsoft.SqlServer.Server;
using QuikSharp;
using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;

//
//  Уроки C# – Синтаксис, Директивы, Классы, Методы – Урок 2 
//***********************************************************
public class Tool //: MainWindow // <--наследование https://youtu.be/MZ0og1DNcCg?si=XMlLSsIsn5CqSXCU&t=1920
{

    private readonly Quik _quik;
    private readonly char separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
    //public MainWindow wnd => (MainWindow)Application.Current.MainWindow;
    MainWindow wnd = (MainWindow)App.Current.MainWindow; // рабочий вариант пользования метода Log()
    //public Window2 wnd2 => (Window2)Application.Current.MainWindow;
    private decimal lastPrice;

    /// <summary>
    ///     Конструктор класса
    /// </summary>
    /// <param name="_quik"></param>
    /// <param name="securityCode">Код инструмента</param> 
    public Tool(Quik quik, string securityCode)
    {
        if (quik == null)
        {
            Console.WriteLine("Конструктор класса Quik = Null");
            return;
        }
        _quik = quik;
        GetBaseParam(quik, securityCode);
    }

    // public void Log(string s) // рабочие вариант пользования метода Log()
    // {
    //     Application.Current.Dispatcher.Invoke(new Action(() => { wnd.Log(s);}));
    //     //wnd.Log(s);
    // }
    //public void Log(string s) => wnd.Log(s);

    private void GetBaseParam(Quik quik, string secCode)
    {
        try
        {
            SecurityCode = secCode;
            ClassCode = quik.Class.GetSecurityClass("SPBFUT,TQBR,TQBS,TQNL,TQLV,TQNE,TQOB,SPBXM,QJSIM", secCode).Result;

            var codes = quik.Class.GetClientCodes().Result;
            if (codes.Count == 1)
                СlientCode = codes[0]; // для демо
            else
                СlientCode = codes[1]; // для боевого

            if (quik != null)
            {
                if (ClassCode != null && ClassCode != "")
                {
                    Name = quik.Class.GetSecurityInfo(ClassCode, SecurityCode).Result.ShortName;
                    AccountID = quik.Class.GetTradeAccount(ClassCode).Result;
                    FirmID = quik.Class.GetClassInfo(ClassCode).Result.FirmId;
                    Step = Convert.ToDecimal(quik.Trading.GetParamEx(ClassCode, SecurityCode, ParamNames.SEC_PRICE_STEP).Result.ParamValue.Replace('.', separator));
                    PriceAccuracy = Convert.ToInt32(Convert.ToDouble(quik.Trading.GetParamEx(ClassCode, SecurityCode, ParamNames.SEC_SCALE).Result.ParamValue.Replace('.', separator)));

                    if (ClassCode == "SPBFUT")
                    {
                        Console.WriteLine("Получаем 'guaranteeProviding'.");
                        Lot = 1;
                        GuaranteeProviding = Convert.ToDouble(quik.Trading.GetParamEx(ClassCode, SecurityCode, ParamNames.BUYDEPO).Result.ParamValue.Replace('.', separator));
                    }
                    else
                    {
                        Console.WriteLine("Получаем 'lot'.");
                        Lot = Convert.ToInt32(Convert.ToDouble(quik.Trading.GetParamEx(ClassCode, SecurityCode, ParamNames.LOTSIZE).Result.ParamValue.Replace('.', separator)));
                        GuaranteeProviding = 0;
                    }
                    GetDepoLimit();
                }
                else
                {
                    Console.WriteLine("Tool.GetBaseParam. Ошибка: classCode не определен.");
                    Lot = 0;
                    GuaranteeProviding = 0;
                }
            }
            else
            {
                Console.WriteLine("Tool.GetBaseParam. quik = null !");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Ошибка в методе GetBaseParam: " + e.Message);
        }

        quik.Candles.Subscribe(ClassCode, secCode, CandleInterval.M1).Wait();
        if (quik.Candles.IsSubscribed(ClassCode, secCode, CandleInterval.M1).Result)
        {
            Debug.WriteLine("Подписались на 1 минуту " + secCode + " ...");
            quik.Candles.NewCandle += CandlesOnNewCandle;
        }

        Console.WriteLine("Подписываемся на стакан...");
        quik.OrderBook.Subscribe(ClassCode, SecurityCode).Wait();
        if (quik.OrderBook.IsSubscribed(ClassCode, SecurityCode).Result)
        {
            var toolOrderBook = new OrderBook();
            Console.WriteLine("Подписка на стакан прошла успешно.");
            _quik.Events.OnQuote += Events_OnQuote; ;
            Console.WriteLine("Подписываемся на колбэк 'OnQuote'...");
            Console.WriteLine(SecurityCode + " Все ОК");
        }
        else Console.WriteLine(SecurityCode + " Все ПЛОХО");

        _quik.Events.OnOrder += Events_OnOrder;
        _quik.Events.OnStopOrder += Events_OnStopOrder;
        _quik.Events.OnTransReply += Events_OnTransReply;
        _quik.Events.OnParam += Events_OnParam;
        _quik.Events.OnDepoLimit += Events_OnDepoLimit;
        // _quik.Events.OnMoneyLimit +
        // _quik.Events.OnAllTrade + 
        // _quik.Events.OnTrade + 
    }

    public async Task Refresh()
    {
        wnd.Dispatcher.Invoke(() => wnd.DataGridTool.Items.Refresh());
    }

    private void GetDepoLimit()
    {
        Positions = Convert.ToDecimal(_quik.Trading.GetDepo(СlientCode, this.FirmID,
            this.SecurityCode, this.AccountID).Result.DepoCurrentBalance / this.Lot);
        Refresh();
        //testMethod();
    }

    private void Events_OnDepoLimit(DepoLimitEx dLimit)
    {
        if (dLimit.SecCode == SecurityCode)
        {
            GetDepoLimit();
        }
    }

    private void testMethod()
    {
        var t = _quik.Trading.GetParamEx(this.ClassCode, this.SecurityCode
             , ParamNames.R_SETTLEPRICE, 1).Result;
    }
    private void Events_OnParam(Param par)
    {
        if (par.SecCode == SecurityCode)
        {
            Refresh();
        }
    }

    private void Events_OnTransReply(TransactionReply transReply)
    {//https://youtu.be/vVehZG3trQ4?si=axTF5vwzTvpA4MMA
        if (transReply.SecCode == SecurityCode)
        {
            if (transReply.Status == 0) wnd.Log("Status " + transReply.Status + " Транзакция отправлена серверу");
            if (transReply.Status == 1) wnd.Log("Status " + transReply.Status + " Транзакция получена на сервер");
            if (transReply.Status == 2) wnd.Log("Status " + transReply.Status + " Ошибка при передаче Транзакции");
            if (transReply.Status == 3)
            {
                wnd.Log(" Reply ордер № " + transReply.OrderNum + "  TransID - " + transReply.TransID + " Цена: " + transReply.Price + " Объём: " + transReply.Quantity);
            }
            if (transReply.Status > 3)
            {
                wnd.Log("ОШИБКА " + transReply.TransID + " - " + transReply.ResultMsg);
            }
        }
    }

    private void Events_OnStopOrder(StopOrder stopOrder)
    {
        if (stopOrder.SecCode == SecurityCode)
        {
            wnd.Log("Стоп-Ордер № - " + stopOrder.OrderNum + ", TransID - " + stopOrder.TransId + ",  SecCode - " + stopOrder.SecCode + " - " + stopOrder.Operation + ", State - " + stopOrder.State);
        }
    }

    private void Events_OnOrder(Order order)
    {
        if (order.SecCode == SecurityCode)
            wnd.Log("Оrder № - " + order.OrderNum + ", TransID - " + order.TransID + ",  SecCode - " + order.SecCode + " - " + order.Operation + ", State - " + order.State);
    }

    private void Events_OnQuote(OrderBook orderbook)
    {
        // if (orderbook.sec_code == SecurityCode)
        // {
        //     var bestBuy = orderbook.bid[orderbook.bid.Length - 1];
        //     var bestSell = orderbook.offer[0];
        //     Console.WriteLine(orderbook.sec_code + ":  bestBuy - " + bestBuy.price + " = " + bestBuy.quantity + " bestSell - " + bestSell.price + " = " + bestSell.quantity);
        //     wnd.Log(orderbook.sec_code + ":  bestBuy - " + bestBuy.price + " = " + bestBuy.quantity + " bestSell - " + bestSell.price + " = " + bestSell.quantity+ ",  this.LastPrice = " +this.LastPrice.ToString());
        //
        // } 
    }

    private void CandlesOnNewCandle(Candle candle)
    {
        //Console.WriteLine(candle.SecCode + "  "+candle.ToString());
    }

    #region Свойства

    /// <summary>
    ///     Краткое наименование инструмента (бумаги)
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    ///     Цена последней сделки
    /// </summary>
    public decimal LastPrice
    {
        get
        {
            lastPrice = Convert.ToDecimal(_quik.Trading.GetParamEx(ClassCode, SecurityCode, "LAST").Result.ParamValue
                .Replace('.', separator));
            return lastPrice;
        }
    }

    /// <summary>
    ///     Позиция
    /// </summary>
    public decimal Positions { get; private set; }

    /// <summary>
    ///     Код инструмента (бумаги)
    /// </summary>
    public string SecurityCode { get; private set; }

    /// <summary>
    ///     Код класса инструмента (бумаги)
    /// </summary>
    public string ClassCode { get; private set; }

    /// <summary>
    ///     Код клиента
    /// </summary>
    public string СlientCode { get; private set; }

    /// <summary>
    ///     Счет клиента
    /// </summary>
    public string AccountID { get; private set; }

    /// <summary>
    ///     Код фирмы
    /// </summary>
    public string FirmID { get; private set; }

    /// <summary>
    ///     Количество акций в одном лоте
    ///     Для инструментов класса SPBFUT = 1
    /// </summary>
    public int Lot { get; private set; }

    /// <summary>
    ///     Точность цены (количество знаков после запятой)
    /// </summary>
    public int PriceAccuracy { get; private set; }

    /// <summary>
    ///     Шаг цены
    /// </summary>
    public decimal Step { get; private set; }

    /// <summary>
    ///     Гарантийное обеспечение (только для срочного рынка)
    ///     для фондовой секции = 0
    /// </summary>
    public double GuaranteeProviding { get; private set; }

    #endregion
}


public class Strategy
{
    /// <summary>
    /// Наименование стратегии
    /// </summary>
    public static Tool StrTool { get; set; }

    /// <summary>
    /// Признак активности
    /// </summary>
    public bool IsActive { get; set; } = false;

    /// <summary>
    /// Операция стоп-заявки
    /// </summary>
    public Operation Operation { get; set; } = Operation.Buy;

    /// <summary>
    /// Количество уровней
    /// </summary>
    public int Levels { get; set; } = 5;

    /// <summary>
    /// Шаг сетки
    /// </summary>
    public decimal Step { get; set; } = (decimal)0.001;

    /// <summary>
    /// Цель
    /// </summary>
    public decimal Cels { get; set; } = 1;

    /// <summary>
    /// Цена стоп-заявки
    /// </summary>
    public decimal Price { get; set; } = StrTool.LastPrice;

    /// <summary>
    /// Количество
    /// </summary>
    public int Quantity { get; set; } = 5;

    /// <summary>
    /// Лист Стоп-Ордеров
    /// </summary>
    public List<long> _ListTrId { get; set; } = new List<long>();

    public Strategy(Tool t, Operation o, decimal price, int Qty, int lvl, decimal step, decimal cels)
    {
        StrTool = t;
        Operation = o;
        Price = price;
        Quantity = Qty;
        Levels = lvl;
        Step = step;
        Cels = cels;
    }
}
