using System.Collections;
using System.Threading;
using netduino.helpers.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using Microsoft.SPOT.Hardware;

namespace LEDMatrix
{
    public class LEDMatrixReaderDisplayController
    {
        private static Max72197221 _max;
        private static ArrayList _displayList;
        private SPI SPIInstance;
        public void StartLEDMatrixThread()
        {

           ArrowBitMapArray();
            _max = new Max72197221(ref SPIInstance, chipSelect: Pins.GPIO_PIN_D5);
            DisplayTestMode();
            ShutdownTestMode();
            AnimateArrow();
            Thread.Sleep(0);

            //while (true)
            //{
            //    Thread.Sleep(10000);
            //}
        }

        public  void ShutdownDisplayDriver()
        {
            _max.SetDecodeMode(Max72197221.DecodeModeRegister.NoDecodeMode);
            _max.Shutdown();
        }

        public void StartupDisplayDriver()
        {
            _max.Shutdown(Max72197221.ShutdownRegister.NormalOperation);
        }


        private static void ShutdownTestMode()
        {
            _max.SetDecodeMode(Max72197221.DecodeModeRegister.NoDecodeMode);
            _max.SetDigitScanLimit(7);
            _max.SetIntensity(3);

            _max.Display(new byte[] { 255, 129, 189, 165, 165, 189, 129, 255 });

            for (int I = 0; I < 2; I++)
            {
                Thread.Sleep(300);
                _max.Shutdown();
                Thread.Sleep(300);
                _max.Shutdown(Max72197221.ShutdownRegister.NormalOperation);
            }
        }

        public static void DisplayTestMode()
        {
            _max.SetDisplayTest(Max72197221.DisplayTestRegister.DisplayTestMode);
            Thread.Sleep(1000);
            _max.SetDisplayTest(Max72197221.DisplayTestRegister.NormalOperation);
        }

        private static void ArrowBitMapArray()
        {
            _displayList = new ArrayList {
                new byte[] {0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x10},
                new byte[] {0x00,0x00,0x00,0x00,0x00,0x00,0x10,0x38},
                new byte[] {0x00,0x00,0x00,0x00,0x00,0x10,0x38,0x7c},
                new byte[] {0x00,0x00,0x00,0x00,0x10,0x38,0x7c,0xfe},
                new byte[] {0x00,0x00,0x00,0x10,0x38,0x7c,0xfe,0x38},
                new byte[] {0x00,0x00,0x10,0x38,0x7c,0xfe,0x38,0x38},
                new byte[] {0x00,0x10,0x38,0x7c,0xfe,0x38,0x38,0x38},
                new byte[] {0x10,0x38,0x7c,0xfe,0x38,0x38,0x38,0x38},
                new byte[] {0x38,0x7c,0xfe,0x38,0x38,0x38,0x38,0x00},
                new byte[] {0x7c,0xfe,0x38,0x38,0x38,0x38,0x00,0x00},
                new byte[] {0xfe,0x38,0x38,0x38,0x38,0x00,0x00,0x00},
                new byte[] {0x38,0x38,0x38,0x38,0x00,0x00,0x00,0x00},
                new byte[] {0x38,0x38,0x38,0x00,0x00,0x00,0x00,0x00},
                new byte[] {0x38,0x38,0x00,0x00,0x00,0x00,0x00,0x00},
                new byte[] {0x38,0x00,0x00,0x00,0x00,0x00,0x00,0x00},
                new byte[] {0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}
                };
        }

        private static void AnimateArrow()
        {
            _max.SetIntensity(15);
            while (true)
            {
                int Arrayindex =0;
                foreach (byte[] matrix in _displayList)
                {
                    // when arrow gets to end then make it bright
                    
                    _max.SetIntensity(15);
	                _max.Display(matrix);
                  
                    Thread.Sleep(120);
                    //Bounce the arrow at the end
                    if (Arrayindex == 7)
                    {
                        short BounceCount = 1;
                        do
                        {
                         //Thread.Sleep(80);
                        _max.Display( (byte[]) _displayList[6]);
                        Thread.Sleep(200);
                        _max.Display((byte[]) _displayList[7]);
                        Thread.Sleep(500);
                        _max.SetIntensity(0);
                        Thread.Sleep(250);
                        _max.SetIntensity(15);
                        Thread.Sleep(500);
                        _max.SetIntensity(0);
                        Thread.Sleep(250);
                        _max.SetIntensity(15); 
                        Thread.Sleep(500);
                        _max.SetIntensity(0);
                        Thread.Sleep(250);
                        _max.SetIntensity(15);
                        BounceCount -= 1;
                        } while (BounceCount>0);
                    } 
                    Arrayindex +=1;
                }
                Thread.Sleep(1000);
            }
        }

        public LEDMatrixReaderDisplayController(ref SPI SPIInstance)
        {
            this.SPIInstance = SPIInstance;
        }
    }
}
