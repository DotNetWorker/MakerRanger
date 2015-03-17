using System.Collections;
using System.Threading;
using NeonMika.Webserver;

namespace NeonMikaWebserverExecuteable
{
    public class WebserverXMLMethods
    {
        public static void Wave(Request e, Hashtable h)
        {
            Thread WaveThread = new Thread(new ThreadStart(() =>
            {
                PinManagement.SwitchDigitalPinState(0);
                Thread.Sleep(50);
                PinManagement.SwitchDigitalPinState(1);
                Thread.Sleep(50);

                PinManagement.SwitchDigitalPinState(4);
                Thread.Sleep(50);
                PinManagement.SwitchDigitalPinState(5);
                Thread.Sleep(50);

                PinManagement.SwitchDigitalPinState(8);
                Thread.Sleep(50);
                PinManagement.SwitchDigitalPinState(9);
                Thread.Sleep(250);

                PinManagement.SwitchDigitalPinState(9);
                Thread.Sleep(50);
                PinManagement.SwitchDigitalPinState(8);
                Thread.Sleep(50);

                PinManagement.SwitchDigitalPinState(5);
                Thread.Sleep(50);
                PinManagement.SwitchDigitalPinState(4);
                Thread.Sleep(50);

                PinManagement.SwitchDigitalPinState(1);
                Thread.Sleep(50);
                PinManagement.SwitchDigitalPinState(0);
                Thread.Sleep(50);
            }));
            WaveThread.Start();               

            h.Add("wave", "1");
        }
    }
}
