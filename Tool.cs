﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

    public void Log(string s) 
    {
        Application.Current.Dispatcher.Invoke(new Action(() => { wnd.Log(s);}));
        //wnd.Log(s);
    }

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
    }

    private void Events_OnOrder(Order order)
    { 
        Console.WriteLine(order.SecCode + "Events_OnOrder"); 
    }

    private void Events_OnQuote(OrderBook orderbook)
    {
        if (orderbook.sec_code == SecurityCode)
        {
            var bestBuy = orderbook.bid[orderbook.bid.Length - 1];
            var bestSell = orderbook.offer[0];
            Console.WriteLine(orderbook.sec_code + ":  bestBuy - " + bestBuy.price + " = " + bestBuy.quantity + " bestSell - " + bestSell.price + " = " + bestSell.quantity);
            Log(orderbook.sec_code + ":  bestBuy - " + bestBuy.price + " = " + bestBuy.quantity + " bestSell - " + bestSell.price + " = " + bestSell.quantity);
        } 
    }

    private void CandlesOnNewCandle(Candle candle)
    {
        Console.WriteLine(candle.SecCode + "  "+candle.ToString());
    }

    #region Свойства

    /// <summary>
    ///     Краткое наименование инструмента (бумаги)
    /// </summary>
    public string Name { get; private set; }

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

    #endregion
}