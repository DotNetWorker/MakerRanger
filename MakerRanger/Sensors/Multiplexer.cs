using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System.IO.Ports;
using System.IO;

namespace MakerRanger
{
    // Class representing the hardware multiplexer that links all the sensors together
    //  this class will select the sensor we want to read by putting the correct data pins 
    //  high on the microprocessor
    class SensorMultiplexer
    {
        // The multiplexer uses three pins S0-S2 to select which pin is routed to the input of the
        // microprocessor
        private static OutputPort muxS0 = new OutputPort(Pins.GPIO_PIN_D2, false);
        private static  OutputPort muxS1 = new OutputPort(Pins.GPIO_PIN_D3, false);
        private static OutputPort muxS2 = new OutputPort(Pins.GPIO_PIN_D4, false);

        public void selectDigit(byte index){
            //We changed the order of the bits on the card so need to read in reverse
            index = (byte) (7 - index);
            
           
           
            //Debug.Print("Sensor: " +  index);
            if (index == 0)
            {

                //1
                writeToMultipler(true, false, true);

            }
            else if (index == 1)
            {
                //0
                writeToMultipler(false, false, true);


            }
            else if (index == 2)
            {

                //3
                writeToMultipler(true, true, false);

            }
            else if (index == 3)
            {
                //5
                writeToMultipler(false, true, true);



            }
            else if (index == 4)
            {
                //2
                writeToMultipler(false, false, false);

            }
            else if (index == 5)
            {
                //4
                writeToMultipler(true, false, false);

            }
            else if (index == 6)
            {
                //7
                writeToMultipler(true, true, true);


            }
            else if (index == 7)
            {
                //6
                writeToMultipler(false, true, false);
            }
            else
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }
        private void writeToMultipler(Boolean s0, Boolean s1, Boolean s2)
        {
            // Helper method does the actual write to the pins
            muxS0.Write(s0);
            muxS1.Write(s1);
            muxS2.Write(s2);
         
        }

    }
}
