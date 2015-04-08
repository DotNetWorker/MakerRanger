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

        private RFID.RFIDIdentityDictionary oRFIDIdentityDictionary;

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
        public event RFIDEventHandler TagChangeDetected;
        public event NativeEventHandler TagLost;

        private Thread MonitorThread;

        public RfidReader(RFID.RFIDIdentityDictionary RFIDIdentityDic, Cpu.Pin ss, SPI SPIInstance, Cpu.Pin ResetPin)
        {
            oRFIDIdentityDictionary = RFIDIdentityDic;
            this.enabled = false;
            mfrc = new Mfrc522(SPIInstance, ss, ResetPin);
        }

        public RfidReader(RFID.RFIDIdentityDictionary RFIDIdentityDic, Cpu.Pin ss, SPI SPIInstance)
        {
            oRFIDIdentityDictionary = RFIDIdentityDic;
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

        //ToDO change this into a rfid changed event on the id changing
        private void ScanForTags()
        {
            bool TagPreviouslyPresent = false;
            string PreviousUid = string.Empty;

           
            bool keeplooping = true;
            while (keeplooping)
            {
                if (mfrc.IsTagPresent() )
                {
                    //Debug.Print("Tag Present");
                    TagPreviouslyPresent = true;
                    //check to see if this tag is the same as last one
                    //

                    string TagIDHexValue = RFID.Utility.HexToString(mfrc.ReadUid().Bytes);

                    //Block any bounce reads only interested in tag changing
                    if (TagIDHexValue != PreviousUid)
                    {
                        //Block any mis reads
                        if (TagIDHexValue != "00000000")
                        {
                            string TagTextDetected = oRFIDIdentityDictionary.GetName(TagIDHexValue);
                            Debug.Print("Tag: " + TagIDHexValue + "  " + TagTextDetected);
                            if (!(TagTextDetected == null))
                            {
                                PreviousUid = TagIDHexValue;
                                string[] TempSplitDescription = oRFIDIdentityDictionary.GetName(TagIDHexValue).Split('|');
                                if (TempSplitDescription.Length == 2)
                                {
                                    TagChangeDetected(this, new RFID.RFIDEventArgs(TempSplitDescription[1], short.Parse(TempSplitDescription[0]), DateTime.Now));
                                }


                            }
                        }
                    }


                }
                else
                {
                    if (TagPreviouslyPresent)
                    {
                        Debug.Print("Tag Lost");
                        if (!(TagLost == null))
                        {
                            TagLost((uint)0, (uint)0, DateTime.Now);
                        }
                        TagPreviouslyPresent = false;
                        PreviousUid = string.Empty;
                    }
                }
                mfrc.HaltTag(); //can't remember what this is doing, need to check docs
                if (!this.enabled) { keeplooping = false; }
                Thread.Sleep(100); //no need to do more than every 100ms
            }

        }

    }
}
