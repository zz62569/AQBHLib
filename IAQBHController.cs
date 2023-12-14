using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AQBHLib
{
    public interface IAQBHController
    {
        Task AQBHPortLink(string com, int boudrate);
        Task AQBHPortClose();
        AQBHModel AQBHModel { set; get; }
        Task<double> AQBHReadData(int address);
        //Task<double[]> AQBHReadData(int address1, int address2);
        Task AQBHSendData();
        double[] SendData { get; set; }
        int Range_1 { get; set; }
        int Range_2 { get; set; }
        bool runstate { get; set; }
        bool _disposed { get; set; }

        double Hsv1 { get; set; }
        double Hsv2 { get; set; }
        double offset_sensor_1 { get; set; }
        double offset_sensor_2 { get; set; }
        //List<double> Sensor_AngleList { get; set; }
        //List<double> Sensor_AngleList2 { get; set; }
        double AngleBeZero(double nowangle);
    }
}
