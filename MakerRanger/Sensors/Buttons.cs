using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace MakerRanger.Sensors
{
    class Buttons
    {
        private DateTime ButtonALastEventDown;
        private DateTime ButtonBLastEventDown;
        private DateTime ButtonALastEventUp;
        private DateTime ButtonBLastEventUp;

        private InterruptPort buttonA = new InterruptPort(Pins.GPIO_PIN_A2, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
        private InterruptPort buttonB = new InterruptPort(Pins.GPIO_PIN_A3, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);

        public event NativeEventHandler OnButtonADown;
        public event NativeEventHandler OnButtonBDown;
        public event NativeEventHandler OnButtonAUp;
        public event NativeEventHandler OnButtonBUp;




        private const long DebounceTime = 10000000;
        public Buttons()
        {
            ButtonALastEventDown = DateTime.Now;
            ButtonBLastEventDown = DateTime.Now;
            ButtonALastEventUp = DateTime.Now;
            ButtonBLastEventUp = DateTime.Now;
            buttonA.OnInterrupt += new NativeEventHandler(buttonA_OnInterrupt);
            buttonB.OnInterrupt += new NativeEventHandler(buttonB_OnInterrupt);
        }

        void buttonA_OnInterrupt(uint Uint1, uint Unint2, DateTime Date)
        {
            //  Software debounce
            //  10,000 ticks = 1ms
            //  1000000 = 100ms

            if (Unint2 == 0)
            {
                if (ButtonALastEventDown.AddTicks(DebounceTime) < Date)
                {
                    Debug.Print("Button A Pressed down " + System.DateTime.Now.ToString());
                    NativeEventHandler ButtonADown = OnButtonADown;
                    if (OnButtonADown != null)
                    {
                        OnButtonADown((uint)0, (uint)0, DateTime.Now);
                    }
                ButtonALastEventDown = Date;
                }
            }
            else
            {
                if (ButtonALastEventUp.AddTicks(DebounceTime) < Date)
                {
                    Debug.Print("Button A Pressed up " + System.DateTime.Now.ToString());
                    NativeEventHandler ButtonAUp = OnButtonAUp;
                    if (OnButtonAUp != null)
                    {
                        OnButtonAUp((uint)0, (uint)0, DateTime.Now);
                    }
                    ButtonALastEventUp = Date;
                }
            }




        }

        void buttonB_OnInterrupt(uint Uint1, uint Unint2, DateTime Date)
        {
            //  Software debounce
            //  10,000 ticks = 1ms
            //  1000000 = 100ms
            // Unit2 =0 for down 1 for up
            if (Unint2 == 0)
            {
                if (ButtonBLastEventDown.AddTicks(DebounceTime) < Date)
                {
                    Debug.Print("Button B Pressed down " + System.DateTime.Now.ToString());
                    NativeEventHandler ButtonBDown = OnButtonBDown;
                    if (OnButtonBDown != null)
                    {
                        OnButtonBDown((uint)0, (uint)0, DateTime.Now);
                    }
                    ButtonBLastEventDown = Date;
                }
            }
            else
            {
                if (ButtonBLastEventUp.AddTicks(DebounceTime) < Date)
                {
                    Debug.Print("Button B Pressed up " + System.DateTime.Now.ToString());
                    NativeEventHandler ButtonBUp = OnButtonBUp;
                    if (OnButtonBUp != null)
                    {
                        OnButtonBUp((uint)0, (uint)0, DateTime.Now);
                    }
                    ButtonBLastEventUp = Date;
                }
            }


        }
    }
}
