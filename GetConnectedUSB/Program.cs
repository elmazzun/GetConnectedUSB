using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetConnectedUSB
{
    class Program
    {
        public enum EventType
        {
            Inserted = 2,
            Removed = 3
        }
        static void Main(string[] args)
        {
            UsbDetector usbDetector = new UsbDetector();
            Console.ReadLine();
        }
    }
}
