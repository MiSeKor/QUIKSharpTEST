using QuikSharp;
using QuikSharp.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DemoTestWPF
{ 
    public class Strategy
    {
        MainWindow wnd = (MainWindow)App.Current.MainWindow;
        

        /// <summary>
        /// Наименование стратегии
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Признак активности
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Номер стоп-заявки
        /// </summary>
        public long StopOrderNum { get; set; }

        /// <summary>
        /// Цена позиции
        /// </summary>
        public decimal PricePosition { get; set; }

        /// <summary>
        /// Операция стоп-заявки
        /// </summary>
        public Operation Operation { get; set; }

        /// <summary>
        /// Условие срабатывания стоп-заявки
        /// </summary>
        public Condition Condition { get; set; }

        /// <summary>
        /// Количество уровней
        /// </summary>
        public int Levels { get; set; }

        /// <summary>
        /// Шаг сетки
        /// </summary>
        public decimal Step { get; set; }

        /// <summary>
        /// Цель
        /// </summary>
        public decimal Cels { get; set; }

        /// <summary>
        /// Уровень тейка
        /// </summary>
        public decimal CondPrice { get; set; }

        /// <summary>
        /// Цена срабатывания стоп-заявки
        /// </summary>
        public decimal CondPrice2 { get; set; }

        /// <summary>
        /// Цена стоп-заявки
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Количество
        /// </summary>
        public int Quantity { get; set; }

        public Strategy Create_Strategy(string name, string BuySel, string price, string quantity
            , string level, string step, string cels)
        {
            Operation op; string stp;
            if (BuySel.ToString() == "Buy")
            {
                op = Operation.Buy;
            } else
            {
                op = Operation.Sell;
            }

         
            stp = step.Replace(".", ",");
            
            Strategy strategy = new Strategy()
            {
                Name = name,
                Operation = Operation.Buy,
                Price = Convert.ToDecimal(price),
                Quantity = Convert.ToInt32(quantity),
                Levels = Convert.ToInt32(level),
                Step = Convert.ToDecimal(stp),
                Cels = Convert.ToDecimal(cels)
            };
            return strategy;
        } 
    } 
}
