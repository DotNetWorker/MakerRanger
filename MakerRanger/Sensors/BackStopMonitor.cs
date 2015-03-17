using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using SecretLabs.NETMF.Hardware;

namespace MakerRanger
{
    // Class to raise events whenever a card is inserted to the back of the reader
    public delegate void AnalogEventHandler();

    class BackStopMonitor
    {
        //Event that will be raised
        public event NativeEventHandler AnalogInterrupt;
        public event NativeEventHandler AnalogInterruptCardRemoved;

        // Analogue input that is attached to the optical sensor that we are monitoring
        public AnalogInput AnalogueInputPin { get; set; }
        // The voltage threshold at which the sensor should trigger
        public short VoltageThresholdLevel { get; set; }
        // The number of times per second that the analogue input should be checked to see if 
        //   it matches the threshold voltage
        public int Timerinterval { get; set; }
        // The timer that will poll the voltage
        private ExtendedTimer _pollingTimer;
        // Don't raise events too close together
        private DateTime EnableBackStopEventAfter = DateTime.Now;
        // Card Present flag
        private Boolean CardRemoved = true;

        public BackStopMonitor(AnalogInput AnalogueInputPin, int TimerInterval, short VoltageThreshold)
        {
            this.VoltageThresholdLevel = VoltageThreshold;
            this.AnalogueInputPin = AnalogueInputPin;
           // this.AnalogueInputPin.SetRange(0, 1024);
            this.Timerinterval = TimerInterval;
            _pollingTimer = new ExtendedTimer(new System.Threading.TimerCallback(PollingInputPin), null, Timerinterval, TimerInterval);
        }

        private void PollingInputPin(object o)
        {
            //Read and raise an event only if one was not raised in last 2 seconds, simple attempt to debounce and
            // limit events raised to main thread
            //might need to consider overflow on next statement
            //Debug.Print("WaitTil: " + EnableBackStopEventAfter.ToString() + " Now: " + DateTime.Now );

            if (DateTime.Now > EnableBackStopEventAfter)
            {
                // Debug.Print("reading");
                // Debug.Print("Last: " + LastBackStopEventRaised.ToString() + " Now: " + DateTime.Now + " allowed");
                // If the reading from the analogue pin is lower than threshold raise event
                int ThisRead = this.AnalogueInputPin.ReadRaw();
            //    Debug.Print("Backstop read: " + ThisRead.ToString() + "  " +this.AnalogueInputPin.Read());
                if ((ThisRead < VoltageThresholdLevel))
                {
                    if (CardRemoved)
                    {
                    //  Debug.Print("Triggered");
                    //System.Threading.Thread.Sleep(200);
                    EnableBackStopEventAfter = DateTime.Now.AddSeconds(3);
                    CardRemoved = false;
                    Debug.Print("Backstop raise event");
                    AnalogInterrupt(new uint(), new uint(), new DateTime());
                    }
                }
                else
                {
                    if (CardRemoved == false)
                    {
                        AnalogInterruptCardRemoved(new uint(), new uint(), new DateTime());
                        Debug.Print("Backstop card removed event");
                    }
                    CardRemoved = true;
                }
            }
            else
            {
           // Debug.Print("NotTime");
            }


        }
    }
}
