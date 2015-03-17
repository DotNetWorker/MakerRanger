using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace MakerRanger
{
    // Class holds an array of sensors - each sensor representing one analogue input on the
    //  microprocessor
    class OpticalSensorArray
    {
        public int ThresholdLevel { get; set; }
       //Multiplexer takes care of selecting the right sensor line to listen to
        private SensorMultiplexer oSensorMultiplexer = new SensorMultiplexer();
        //The output of the multiplexer comes out on GPIO_PIN_A5, set it up as an input
        //private static AnalogInput ReflectiveSensor = new AnalogInput(AnalogChannels.ANALOG_PIN_A5);
        private static AnalogInput ReflectiveSensor = new AnalogInput(AnalogChannels.ANALOG_PIN_A5);
        
        public event NativeEventHandler CardRead;

        
        //Scans thru the sensors using the multiplexer to construct the value on the inserted 
        // card
        public void ReadValue()
        {
            //ReflectiveSensor.SetRange(0, 1024);
            // Temp variables to hold the first and second digit from the punch card
            short measuredvalue0 = 0;
            short measuredvalue1 = 0;
            // We need to scan through the 8 sensors constructing the value they represent
            int loopcounter = 1;
            do
            {
               for (byte i = 0; i < 8; i++)
            {
                Debug.Print("Select sensor " + i.ToString());
                oSensorMultiplexer.selectDigit(i);
                //System.Threading.Thread.Sleep(20);
                int DummyReading = ReflectiveSensor.ReadRaw();
                
                int[] SensorReadings = new int[2];
                for (int arrayindex = 0; arrayindex < 2; arrayindex++)
                {
                    System.Threading.Thread.Sleep(2);
                    SensorReadings[arrayindex] = ReflectiveSensor.ReadRaw();
                    Debug.Print("Raw: " + SensorReadings[arrayindex].ToString());
                }
                              
                int CurrentSensorReading  =AverageElements(SensorReadings,2);
               
                Debug.Print("Sensor " + i.ToString() + ": " + CurrentSensorReading.ToString());
                if (CurrentSensorReading > this.ThresholdLevel)
                {
                    // If bit is present set it otherwise do nothing as the base mask is zero anyhow
                    // Use bit mask to set the relevant bits
                    if (i <= 3)
                    {
                        //First number set bit position to 1
                        measuredvalue0 |= (short)(System.Math.Pow(2, i));
                    }
                    else
                    {
                        //Second number set bit position to 1
                        measuredvalue1 |= (short)(System.Math.Pow(2, i - 4));
                    }
                }
            }
              // Debug.Print("CardReads:" + (measuredvalue0.ToString() + "  " + measuredvalue1.ToString()));
                loopcounter -= 1;
            } while (loopcounter>0);
           
            // As this is not a "real" binary number, rather two individual numbers, combine them again to 
            // make the actual number and raise event
            Debug.Print("CardReads: " + (measuredvalue0.ToString() + "  " + measuredvalue1.ToString()));
            CardRead((uint) measuredvalue0, (uint) measuredvalue1, new DateTime()); 
        }


        public int AverageElements(int[] arr, int size)
        {
            int sum = 0;
            int average = 0;
            for (int i = 0; i < size; i++)
            {
                sum += arr[i];
            }
            average = sum / size; // sum divided by total elements in array
            return average;
        }


        public OpticalSensorArray(int Threshold)
        {
            this.ThresholdLevel = Threshold;
        }
    }
}
