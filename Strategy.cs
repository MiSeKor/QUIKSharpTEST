using QuikSharp.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoTestWPF
{
    internal class Strategy
    {
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
        public int Step { get; set; }

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
    } 
}
