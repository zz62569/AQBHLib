using NModbus;
using NModbus.Serial;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO.Ports;
using System.Net;


namespace AQBHLib
{
    public class AQBHController : IAQBHController
    {
        private static IModbusMaster? master { get; set; }
        public SerialPort _port { set; get; } = new SerialPort();
        public AQBHModel AQBHModel { get; set; } = new AQBHModel();
        public bool _disposed { get; set; }
        public double[] SendData { get; set; } = new double[2];
        #region Port Link
        /// <summary>
        /// AQBH Model Port Link
        /// </summary>
        public Task AQBHPortLink(string com, int boudrate)
        {
            if (!_port.IsOpen)
            {
                _port.Close();
            }
            _port = new SerialPort(com, boudrate, Parity.None, 8, StopBits.One);
            _port.ReadBufferSize = 8100;
            _port.ReadTimeout = 1000;
            _port.WriteTimeout = 1000;
            var factory = new ModbusFactory();
            master = factory.CreateRtuMaster(_port);
            _port.Open();
            AQReadData();
            _disposed = true;
            return Task.CompletedTask;
        }
        #endregion
        #region Port Close
        /// <summary>
        /// AQBH Model Port Close
        /// </summary>
        public Task AQBHPortClose()
        {
            _disposed = false;
            _port.Close();
            return Task.CompletedTask;
        }
        #endregion
        #region Read Data
        public int Range_1 { get; set; } = 0;
        public int Range_2 { get; set; } = 0;
        public bool runstate { get; set; } = true;
        private double oldvoltage_1 { get; set; } = 0;
        private double oldvoltage_2 { get; set; } = 0;
        public double Hsv1 { get; set; } = 0;
        public double Hsv2 { get; set; } = 0;
        public double offset_sensor_1 { get; set; } = 0;
        public double offset_sensor_2 { get; set; } = 0;

        private double k1 = 0;
        private double k2 = 0;
        
        private void AQReadData()
        {
             Task.Run(async() => { 
                while (true)
                {
                    if(_disposed)
                    {
                         try
                         {
                             ushort[] mbfifo = await master!.ReadInputRegistersAsync(
                                     0x1,
                                     0x0,
                                     //0x100,
                                     2
                                     );
                             SendData[0] = mbfifo[0] / 4096.0;
                             SendData[1] = mbfifo[1] / 4096.0;
                             //SendData[0] = mbfifo[0] / 65536.0;
                             //SendData[1] = mbfifo[1] / 65536.0;
                             //if (oldvoltage_1 != 0)
                             //{
                             //    if (runstate)
                             //    {
                             //        var _  = oldvoltage_1 / SendData[0];
                             //        //二阶比较
                             //        if (_ > 1.2 && k1 < 1.2)
                             //            Range_1 += 1;
                             //        k1 = _;

                             //    }
                             //    else
                             //    {
                             //        var _ =  SendData[0] / oldvoltage_1;
                             //        if (_ > 1.2 && k1 < 1.2)
                             //            Range_1 -= 1;
                             //        k1 = _;
                             //    }
                             //    Hsv1 = Calculateangle(SendData[0], 20, 4, 4, offset_sensor_1, Range_1);
                             //    //Trace.WriteLine(oldvoltage_1 + DateTime.Now.ToString() + "\r\n");
                             //}
                             //if (oldvoltage_2 != 0)
                             //{
                             //    if (runstate)
                             //    {
                             //        var _ = oldvoltage_2 / SendData[1];
                             //        if (_ > 1.2 && k1 < 1.2)
                             //            Range_2 += 1;
                             //        k2 = _;
                             //    }
                             //    else
                             //    {
                             //        var _ = SendData[1] / oldvoltage_2;
                             //        if (_ > 1.2 && k1 < 1.2)
                             //            Range_2 -= 1;
                             //        k2 = _;
                             //    }
                             //    Hsv2 = Calculateangle(SendData[1], 20, 4, 200, offset_sensor_2, Range_2);
                             //}

                             //if (SendData[0] != 1)
                             //    oldvoltage_1 = SendData[0];
                             //if (SendData[1] != 1)
                             //    oldvoltage_2 = SendData[1];
                         }
                         catch { }
                     }
                     await Task.Delay(10);
                }            
            });
        }
        public double Calculateangle(double voltage, double max, double min,
            double sensorgear, double offset, int range)
        {
            //新老对比，出现4 跳变到 20 则 多圈数 +1 出现
            var dt = ((voltage * max + 0.01 - min) /
                (max - min) * 360.0 + range * 360) / 19.2 * sensorgear - offset;
            return dt;
        }

        public double AngleBeZero(double nowangle)
        {
            return nowangle;
        }

        /// <summary>
        /// AQBH Read Data
        /// </summary>
        /// <returns></returns>
        public async Task<double> AQBHReadData(int address)
        {
            ushort[] mbfifo = await master!.ReadInputRegistersAsync(
                    0x1,
                    0x100,
                    2
                    );
            return await Task.FromResult(mbfifo[address] / 65535.0);
        }
        public async Task<double[]> AQBHReadData(int address1, int address2)
        {
            double[] data = new double[4];
            ushort[] mbfifo = await master!.ReadInputRegistersAsync(
                    0x1,
                    0x106,
                    4
                    );
            data[0] = mbfifo[0] / 65535.0;
            data[1] = mbfifo[1] / 65535.0;
            return await Task.FromResult(data);
        }
        #endregion
        #region Write Data
        public async Task AQBHSendData()
        {
            _port.Write(AQBHModel.InitCmd, 0, AQBHModel.InitCmd.Length);
            await Task.Delay(100);
        }
        #endregion
    }
}
