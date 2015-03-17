using System;
using Microsoft.SPOT;
using System.Threading;
using Microsoft.SPOT.Hardware;


namespace MakerRanger
{
    class LabelPrinting
    {

        public event NativeEventHandler StickerPrinted;

        //Zebra printer using EPL is hidden behind the serial port helper
        // Defaults to: SerialPorts.COM1 (uses D0 and D1), 9600, Parity.None, 8, StopBits.One
        static SerialPortHelper serialPortHelper = new SerialPortHelper();

        //public void StoreFormsOnPrinter(PersistedStorage oPersistedStorage)
        //{
        //    //Clear all forms from printer memory
        //    serialPortHelper.PrintLine(@"FK""*""");
        //    foreach (var item in oPersistedStorage.GetLabelFormNamesFromSD())
        //    {
        //        StoreForm((string)item, oPersistedStorage);
        //    }
        //}
        //public void StoreForm(string FormName, PersistedStorage oPersistedStorage )
        //{
        //    serialPortHelper.PrintLine("");
        //    serialPortHelper.PrintLine(@"FS""" + FormName + @"""");
        //    //Get Form text from SD
        //    serialPortHelper.PrintLine(oPersistedStorage.GetFormText(FormName));
        //    //End of Form
        //    serialPortHelper.PrintLine(@"FE");
        //    serialPortHelper.PrintLine("");
        //}
        public void RecallFormAndPrint(int pauseTime,string FormName, PersistedStorage oPersistedStorage, string GuessValue, Boolean Winner)
        {
            System.Threading.Thread.Sleep(pauseTime);

            serialPortHelper.PrintLine("\n");
            serialPortHelper.PrintLine(oPersistedStorage.GetFormText(FormName));

            if (FormName == @"03")
            {
                if (Winner)
                {
                    serialPortHelper.PrintLine(@"A70,300,0,1,2,2,R,""I guessed it!""");
                }
                else
                {
                    serialPortHelper.PrintLine(@"A175,332,0,1,1,1,N,""GUESS""");
                    serialPortHelper.PrintLine(@"A172,348,0,3,2,2,R,""" + GuessValue + @"""");
                }
            }
            else if (FormName == @"04")
            {
                if (Winner)
                {
                    serialPortHelper.PrintLine(@"A70,268,0,1,2,2,R,""I guessed it!""");
                }
                else
                {
                    serialPortHelper.PrintLine(@"A175,332,0,1,1,1,N,""GUESS""");
                    serialPortHelper.PrintLine(@"A172,348,0,3,2,2,R,""" + GuessValue + @"""");
                }
                
                //Print Graphics if required
                // GW top left width height
                serialPortHelper.Print(@"GW90,10,25,200,");
                PersistedStorage.SendFileToSerial(serialPortHelper, @"SD\transfertmp.tmp");

                serialPortHelper.PrintLine(@"");
            }



            //Number of labels to print
            serialPortHelper.PrintLine(@"P1,1");
            StickerPrinted(new uint(), new uint(), new DateTime());
                
        }

        public void ResetPrinter()
        {
            serialPortHelper.PrintLine("\n");
            serialPortHelper.PrintLine(@"^@");
        }

        public void TestPrinter()
        {
            serialPortHelper.PrintLine(@"");
            serialPortHelper.PrintLine(@"");
            serialPortHelper.PrintLine(@"U");
            serialPortHelper.PrintLine(@"");
        }
        public void TestPrintLabel()
        {
            serialPortHelper.PrintLine("\n");
            serialPortHelper.PrintLine(@"N");
            serialPortHelper.PrintLine(@"q609");
            serialPortHelper.PrintLine(@"Q203,26");
            serialPortHelper.PrintLine(@"B26,26,0,UA0,2,2,152,B,""603679025109""");
            serialPortHelper.PrintLine(@"A253,26,0,3,1,1,N,""SKU 6205518 MFG 6354""");
            serialPortHelper.PrintLine(@"A253,56,0,3,1,1,N,""2XIST TROPICAL BEACH""");
            serialPortHelper.PrintLine(@"A253,86,0,3,1,1,N,""STRIPE SQUARE CUT TRUNK""");
            serialPortHelper.PrintLine(@"A253,116,0,3,1,1,N,""BRICK""");
            serialPortHelper.PrintLine(@"A253,146,0,3,1,1,N,""X-LARGE""");
            serialPortHelper.PrintLine(@"P1,1");
        }
    }
}
