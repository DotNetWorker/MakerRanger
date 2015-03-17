using System;
using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace MakerRanger.LEDStripLighting
{
    class LEDStrips
    {
        private static OutputPort LeftAnt = new OutputPort(Pins.GPIO_PIN_D7, false);
        private static OutputPort RightAnt = new OutputPort(Pins.GPIO_PIN_D8, false);
        private static OutputPort InternalLights = new OutputPort(Pins.GPIO_PIN_D9, false);

        public LEDStrips()
        {
            Boolean state = true;
            for (int i = 0; i < 6; i++)
            {
                state = !state;
                LeftAnt.Write(state);
                RightAnt.Write(state);
                InternalLights.Write(state);
                Thread.Sleep(200);
            }
            LeftAnt.Write(false);
            RightAnt.Write(false);
            InternalLights.Write(false);
        }

        public void WinnerSequence()
        { 
            // left right left right both both both
            bool state = false;
            for (int i = 0; i < 250; i++)
            {
            LeftAnt.Write(!state);
            RightAnt.Write(state);
            Thread.Sleep(55);
            state = !state;
            }

            for (int i = 0; i < 10; i++)
            {
                state = true;
                LeftAnt.Write(!state);
                RightAnt.Write(state);
                Thread.Sleep(500);
                state = false;
                LeftAnt.Write(!state);
                RightAnt.Write(state);
                Thread.Sleep(500);
            }
            Normal();
        }

        public void LooserSequence()
        {
            //strobe
            bool stateleft = false;
            bool stateright = false;
            LeftAnt.Write(false);
            RightAnt.Write(false);
            Thread.Sleep(4000);
            for (int i = 0; i < 60; i++)
            {
                Random oRandomNumber = new Random();
                stateleft = ToBoolean(oRandomNumber.Next(2)-1);
                stateright = ToBoolean(oRandomNumber.Next(2)-1);
                LeftAnt.Write(stateleft);
                RightAnt.Write(stateright);
                Thread.Sleep(oRandomNumber.Next(10)*10);
            }
            Normal();
        }

        public static bool ToBoolean(int value)
        {
            return (value != 0);
        }

        public void Normal(){
            LeftAnt.Write(true);
            RightAnt.Write(true);
        }

        public void CaseLights(bool state)
        {
            Debug.Print("Case lights: " + state.ToString());
            InternalLights.Write(state);
        }

      
    }
}