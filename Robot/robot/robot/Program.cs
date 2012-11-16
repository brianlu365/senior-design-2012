using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            System.IO.Ports.SerialPort wixel = new System.IO.Ports.SerialPort("COM20", 57600);
            wixel.Open();
            wixel.Write("hello");
            wixel.Close();
        }
    }
}
