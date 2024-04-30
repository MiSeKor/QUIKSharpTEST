using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoTestWPF
{
    internal class MyTick
    {
    public long Id { get; set;}
    public string NameTool { get; set;}
    public string Time { get; set;}
    public decimal Price { get; set; }
    public int Volume { get; set; }
    public string Buy_Sell { get; set; }
    public string Kol_Pok { get; set; }
    public string Kol_Prd { get; set; }
    public int OI { get; set; }
    }
}
