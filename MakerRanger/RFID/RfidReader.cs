using Mfrc522Lib;
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System.Threading;
namespace MakerRanger
{
    //Enabled
    //Detected
    //Lost
    class RfidReader
    {
        private Mfrc522 mfrc;

        private RFID.RFIDIdentityDictionary oRFIDIdentityDictionary = new RFID.RFIDIdentityDictionary();

        private bool _enabled = false;
        public bool enabled
        {
            get { return _enabled; }
            set
            {
                if (value == true)
                {
                    //Start a new thread to run the monitor on
                    if (MonitorThread == null)
                    {
                        MonitorThread = new Thread(ScanForTags);
                        MonitorThread.Start();

                    }
                }
                _enabled = value;
            }
        }



        public delegate void RFIDEventHandler(object sender, MakerRanger.RFID.RFIDEventArgs e);
        public event RFIDEventHandler TagDetected;
        public event NativeEventHandler TagLost;

        private Thread MonitorThread;

        public RfidReader(Cpu.Pin ss, SPI SPIInstance, Cpu.Pin ResetPin)
        {
            oRFIDIdentityDictionary.LoadFromFile();
            this.enabled = false;
            mfrc = new Mfrc522(SPIInstance, ss, ResetPin);
        }

        public RfidReader(Cpu.Pin ss, SPI SPIInstance)
        {
            oRFIDIdentityDictionary.LoadFromFile();
            this.enabled = false;
            mfrc = new Mfrc522(SPIInstance, ss);
        }

        //The reset line is tied to all readers, so reset will reset all readers, 
        // Init must be called on each reader after reset on one
        public void ResetReaders()
        {
            mfrc.Reset();
        }

        public void InitReader()
        {
            mfrc.Init();
        }

        private void ScanForTags()
        {
            Uid PreviousUid = null;
            bool keeplooping = true;
            while (keeplooping)
            {
                if (mfrc.IsTagPresent())
                {

                    //check to see if this tag is the same as last one
                    //
                    string TagIDHexValue = RFID.Utility.HexToString(mfrc.ReadUid().Bytes);
                    string TagIDDetected = oRFIDIdentityDictionary.GetName(TagIDHexValue);
                    Debug.Print("Tag: " + TagIDHexValue + "  " + TagIDDetected);
                    if (!(TagIDDetected == null))
                    {
                        TagDetected(this, new RFID.RFIDEventArgs(TagIDDetected, DateTime.Now));
                    }
                }
                else
                {
                    if (!(PreviousUid == null))
                    {
                        TagLost((uint)0, (uint)0, DateTime.Now);
                        PreviousUid = null;
                    }
                }
                mfrc.HaltTag(); //can't remember what this is doing, need to check docs
                if (!this.enabled) { keeplooping = false; }
                Thread.Sleep(100); //no need to do more than every 100ms
            }

        }

    }
}
