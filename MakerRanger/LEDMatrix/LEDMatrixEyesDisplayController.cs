using System.Collections;
using System.Threading;
using netduino.helpers.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using Microsoft.SPOT.Hardware;

namespace LEDMatrix {
    public class LEDMatrixControllerEyesDisplay {
        private static Max72197221 _max;
        private static ArrayList _displayList = new ArrayList();
           
        private SPI SPIInstance;

        public enum MatrixModeType
        {
            WinnerSequence,
            LooserSequence,
            eyes,
            PreviousGuesses,
            ThisGuess
        }

        public MatrixModeType CurrentDisplayMode = MatrixModeType.eyes;

        public ArrayList PreviousGuesses { get; set; }
        public byte LastGuessValue;

        public void StartLEDMatrixThread()
            {
            InitializeBitmapArray();
            _max = new Max72197221(ref SPIInstance, chipSelect: Pins.GPIO_PIN_D10,DualDisplay: true);
            
            DisplayTestMode();
            ShutdownTestMode();

            //Thread.Sleep(0);
           // var DisplayThread = new Thread(NumberSpinner);

            var DisplayThread = new Thread(DisplayLoop);
            DisplayThread.Start();
            //while (true)
            //{
            //    Thread.Sleep(Timeout.Infinite);
            //}
            Thread.Sleep(Timeout.Infinite);
        }

        private static void ShutdownTestMode() {
            _max.SetDecodeMode(Max72197221.DecodeModeRegister.NoDecodeMode);
            _max.SetDigitScanLimit(7);
            _max.SetIntensity(3);

            _max.Display(new byte[] { 255, 129, 189, 165, 165, 189, 129, 255 }, new byte[] { 255, 129, 189, 165, 165, 189, 129, 255 });

            for(int I = 0; I < 1; I++) {
                Thread.Sleep(300); 
                _max.Shutdown();
                Thread.Sleep(300);
                _max.Shutdown(Max72197221.ShutdownRegister.NormalOperation);
            }
        }

        private static void DisplayTestMode() {
            _max.SetDisplayTest(Max72197221.DisplayTestRegister.DisplayTestMode);
            Thread.Sleep(1000);
            _max.SetDisplayTest(Max72197221.DisplayTestRegister.NormalOperation);
        }

        //{0x7c,0xfe,0x9a,0xb2,0xfe,0x7c,0x00,0x00}, // 0
        //{0x42,0x42,0xfe,0xfe,0x02,0x02,0x00,0x00}, // 1
        //{0x46,0xce,0x9a,0x92,0xf6,0x66,0x00,0x00}, // 2
        //{0x44,0xc6,0x92,0x92,0xfe,0x6c,0x00,0x00}, // 3
        //{0x18,0x38,0x68,0xc8,0xfe,0xfe,0x08,0x00}, // 4
        //{0xe4,0xe6,0xa2,0xa2,0xbe,0x9c,0x00,0x00}, // 5
        //{0x3c,0x7e,0xd2,0x92,0x9e,0x0c,0x00,0x00}, // 6
        //{0xc0,0xc6,0x8e,0x98,0xf0,0xe0,0x00,0x00}, // 7
        //{0x6c,0xfe,0x92,0x92,0xfe,0x6c,0x00,0x00}, // 8
        //{0x60,0xf2,0x92,0x96,0xfc,0x78,0x00,0x00}, // 9

        //Byte array
        private static void InitializeBitmapArray() {
            _displayList = new ArrayList {
                //0-9 characters as bitmaps
                new byte[] {0x7c,0xfe,0x9a,0xb2,0xfe,0x7c,0x00,0x00},
                new byte[] {0x42,0x42,0xfe,0xfe,0x02,0x02,0x00,0x00},
                new byte[] {0x46,0xce,0x9a,0x92,0xf6,0x66,0x00,0x00},
                new byte[] {0x44,0xc6,0x92,0x92,0xfe,0x6c,0x00,0x00},
                new byte[] {0x18,0x38,0x68,0xc8,0xfe,0xfe,0x08,0x00},
                new byte[] {0xe4,0xe6,0xa2,0xa2,0xbe,0x9c,0x00,0x00},
                new byte[] {0x3c,0x7e,0xd2,0x92,0x9e,0x0c,0x00,0x00},
                new byte[] {0xc0,0xc6,0x8e,0x98,0xf0,0xe0,0x00,0x00},
                new byte[] {0x6c,0xfe,0x92,0x92,0xfe,0x6c,0x00,0x00},
                new byte[] {0x60,0xf2,0x92,0x96,0xfc,0x78,0x00,0x00},
                // empty 10
                new byte[] {0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00},
                //Spinner
                new byte[] {1, 2, 4, 8, 16, 32, 64, 128},
                new byte[] {0, 0, 0, 255, 0, 0, 0, 0},
                new byte[] {128, 64, 32, 16, 8, 4, 2, 1},
                new byte[] {16, 16, 16, 16, 16, 16, 16, 16},
               //happy vader 15
                new byte[] {0x1f,0x3d,0xa8,0x7e,0x7e,0xa8,0x3d,0x1f},
                 //sad vader 16
                new byte[] {0x1f,0x3d,0xa8,0x7b,0x7b,0xa8,0x3d,0x1f},
                //eyes 17
                new byte[] {0x60,0xf2,0x92,0x96,0xfc,0x78,0x00,0x00},
                //heart 18
                new byte[] {0x38,0x7c,0x7e,0x3f,0x3f,0x7e,0x7c,0x38},
                //Cross wrong 19
                new byte[] {0x00,0x42,0x24,0x18,0x18,0x24,0x42,0x00},
                //new byte[] {0x08,0x04,0x72,0x02,0x02,0x72,0x04,0x08},
                // frown 20
                new byte[] {0x00,0x32,0x34,0x04,0x04,0x34,0x32,0x00},
                // smile 21
                new byte[] {0x08,0x04,0x72,0x02,0x02,0x72,0x04,0x08}
            };
        }

        private static void SpinSpinner()
        {
            for (int r = 0; r < 3; r++)
            {
                for (int i = 0; i < 4; i++)
                {
                    _max.Display((byte[])_displayList[i+11], (byte[])_displayList[14-i]);
                    Thread.Sleep(60);
                }
            }
                


        }

        private static void NumberSpinner() {
            _max.SetIntensity(15);
            // NumberDisplayTest();
            while (true) {
                foreach (byte[] matrix in _displayList) {
                    _max.Display(matrix, matrix);
                    Thread.Sleep(700);
                }
            }
        }

        private System.DateTime LastShowedGuesses = System.DateTime.Now;
        private System.DateTime LastShowedEyes = System.DateTime.Now;
        private  void DisplayLoop()
        {
            _max.SetIntensity(15);
            while (true)
            {
                // Show the guessed numbers and occasional "eyes"
                if (this.CurrentDisplayMode == MatrixModeType.eyes) {
                    DisplayEyesSequence();
                }
                else if (this.CurrentDisplayMode == MatrixModeType.LooserSequence)
                {
                    DisplayLooserSequence();
                }
                else if (this.CurrentDisplayMode == MatrixModeType.PreviousGuesses)
                {
                    DisplayPreviousGuesses();
                }
                else if (this.CurrentDisplayMode == MatrixModeType.WinnerSequence)
                {
                    DisplayWinnerSequence();
                }
                else if (this.CurrentDisplayMode == MatrixModeType.ThisGuess)
                {
                    DisplayThisGuess();
                }
                else
                {
                    throw new System.Exception("unknown mode");
                }

                Thread.Sleep(100);
            }
        }

        private void DisplayEyesSequence()
        {
            //show happy vader and love sign
            _max.Display((byte[])_displayList[18], (byte[])_displayList[15]);
            for (int i = 15; i > 0; i--)
            {
                if  (this.CurrentDisplayMode != MatrixModeType.eyes){
                    break;
                }
                _max.SetIntensity((byte)i, 15);
                Thread.Sleep(50);
                
            }
            for (int ii = 0; ii < 15; ii++)
                {
                    if (this.CurrentDisplayMode != MatrixModeType.eyes)
                    {
                        break;
                    }
                    _max.SetIntensity((byte)ii, 15);
                    Thread.Sleep(50);
                }
            _max.SetIntensity(15, 15);
            
            
            this.LastShowedEyes = System.DateTime.Now;
            // if more than 5 seconds of showing eyes, start showing guesses
            if (( LastShowedGuesses< System.DateTime.Now.AddSeconds(-5)) & !(this.CurrentDisplayMode == MatrixModeType.ThisGuess)){
                this.CurrentDisplayMode = MatrixModeType.PreviousGuesses;
            }
        }

        private void DisplayWinnerSequence()
        {
            _max.Display( (byte[])_displayList[21], (byte[]) _displayList[21]);
            Thread.Sleep(10000);
                this.CurrentDisplayMode = MatrixModeType.eyes;
            
        }

        private void DisplayLooserSequence()
        {
            _max.SetIntensity(0);
            _max.Display((byte[])_displayList[20], (byte[])_displayList[20]);
            for (int i = 0; i < 6; i++)
            {
                _max.SetIntensity((byte)i);
                //if (!(this.CurrentDisplayMode == MatrixModeType.LooserSequence)){
                //    break;
                //}
                Thread.Sleep(125);
            }
            Thread.Sleep(2000); 
            _max.Display((byte[])_displayList[19], (byte[])_displayList[19]);
             Thread.Sleep(2000); 
             this.CurrentDisplayMode = MatrixModeType.eyes;
        }

        private void DisplayThisGuess()
        {
            //if (this.PreviousGuesses.Count == 0)
            //{
            // //no previous guesses show eyes instead
            //    this.CurrentDisplayMode = MatrixModeType.eyes;
            //    DisplayEyesSequence();  
            //}
            //else
            //{
            //    DisplayNumberAndFlash( this.LastGuessValue, MatrixModeType.ThisGuess);
            //    Thread.Sleep(500);
            //    DisplayLooserSequence();
            //    this.CurrentDisplayMode = MatrixModeType.eyes;
            //}
            SpinSpinner();
            DisplayNumberAndFlash(this.LastGuessValue, MatrixModeType.ThisGuess);
            Thread.Sleep(500);
            //DisplayLooserSequence();
            this.CurrentDisplayMode = MatrixModeType.LooserSequence;
        }

        private void DisplayPreviousGuesses()
        {
            //check to see if its been more than 10 seconds since last showed the eyes
            if ((this.PreviousGuesses.Count > 0) & (this.LastShowedEyes > System.DateTime.Now.AddSeconds(-10))  ) 
            {
            System.Random oRandGen = new System.Random();
            if (PreviousGuesses.Count == 0) {
                //do nothing no previous guesses to show
            
            }
            else if (PreviousGuesses.Count == 1) {
                //Show the one we have
                ShowPreviousGuess((byte)this.PreviousGuesses[0]);
            }
            else
            {
                //Show a random previous guess
                ShowPreviousGuess(byte.Parse(this.PreviousGuesses[(int)(oRandGen.Next(this.PreviousGuesses.Count - 1))].ToString()));
            }
                
            this.LastShowedGuesses = System.DateTime.Now;
            }
            else
            { 
                //no previous guesses show eyes instead
                if (!(this.CurrentDisplayMode == MatrixModeType.ThisGuess)){
                 this.CurrentDisplayMode = MatrixModeType.eyes;
                }
                //DisplayEyesSequence();
            }
           
        }

        private void ShowPreviousGuess(byte NumberToDisplay)
        {
                DisplayBlankCharacter();
                for (int i = 0; i < 2; i++)
                {
                    //Lots of checks to get out of this mode quick if required
                    if (ExitDisplayAndFlash(MatrixModeType.PreviousGuesses))
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }
                if (!ExitDisplayAndFlash(MatrixModeType.PreviousGuesses))
                {
                        DisplayNumber(NumberToDisplay);
                        // 22 previous delay changed to make it quicker
                        for (int i = 0; i < 18; i++)
                        {
                            //Lots of checks to get out of this mode quick if required
                            if (ExitDisplayAndFlash(MatrixModeType.PreviousGuesses))
                            {
                                break;
                            }
                            Thread.Sleep(100);
                        }
                }
        }

        private static void NumberDisplayTest()
        {
            _max.SetIntensity(15);
            for (byte i = 0; i < 99; i++)
            {
                DisplayNumber(i);
                Thread.Sleep(100);
            }
        }


        private static void DisplayBlankCharacter()
        {
            //Send blank  to display
            _max.Display((byte[])_displayList[10], (byte[])_displayList[10]);
        }

        private static void DisplayNumber(byte Number)
        {
            string NumberAsString = Number.ToString();
            if (NumberAsString.Length == 1)
            { 
                //pad single character numbers
                NumberAsString = "0" + NumberAsString;
            }
                byte[] Digit1;
                byte[] Digit2;
                // _displayList contains the characters index lines up with actual number
                Digit1 = (byte[]) _displayList[ int.Parse( NumberAsString.Substring(0,1))];
                Digit2 = (byte[]) _displayList[int.Parse(NumberAsString.Substring(1, 1))];
                //Send to display
                _max.Display(Digit1, Digit2);
       
        }

        private  void DisplayNumberAndFlash(byte Number,  MatrixModeType MessageSource)
        {
            //now flash s the numbers
                for (int i = 0; i < 8; i++)
                {   DisplayNumber(Number);
                    //Lots of checks to get out of this mode quick if required
                    if (ExitDisplayAndFlash(MessageSource))
                    {
                        break;
                    }
                    Thread.Sleep(200);
                   
                    //Blank display
                    DisplayBlankCharacter();

                    if (ExitDisplayAndFlash(MessageSource))
                    {
                        break;
                    }
                    Thread.Sleep(200);
                    if (ExitDisplayAndFlash(MessageSource))
                    {
                        break;
                    }
                }
        }

        public bool ExitDisplayAndFlash(MatrixModeType MessageSource)
        {
            if (!(this.CurrentDisplayMode == MessageSource) & (MessageSource == MatrixModeType.PreviousGuesses))
            {
                return true;
            } 
            else{
                return false;
            }
        }

        public static byte[] InvertBits(byte[] Values)
        {
            //Invert all bits in the byte array
            for (int i = 0; i < Values.Length; i++)
                Values[i] ^= 0xFF;
            return Values;
        }

        public LEDMatrixControllerEyesDisplay(ref SPI SPIInstance)
        {
            this.SPIInstance = SPIInstance;
            this.PreviousGuesses = new ArrayList();
        }
    }
}
