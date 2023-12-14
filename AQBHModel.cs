using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AQBHLib
{
    public class AQBHModel
    {
        public byte[] InitCmd = new byte[] { 0xEB, 0x00, 0x55, 0xF5, 0x00, 0x00 };
        public double Power { get; set; }
    }
}
