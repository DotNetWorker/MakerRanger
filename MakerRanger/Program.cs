using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;                                                                                                                                                                                                   
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System.IO.Ports;
using System.IO;
using NeonMika.Webserver;



namespace MakerRanger
{
	public class Program
	{
		//static BackStopMonitor oBackStopMonitor;
		static PersistedStorage oPersistedStorage;
		//static MakerRanger.LEDStripLighting.LEDStrips oLEDStrips = new LEDStripLighting.LEDStrips();
		//static OpticalSensorArray oOpticalSensorArray = new OpticalSensorArray(250); original used for makerfaire newcastle
		//static OpticalSensorArray oOpticalSensorArray = new OpticalSensorArray(200); original used for manchester makerfaire
		static OpticalSensorArray oOpticalSensorArray = new OpticalSensorArray(800);

		static SPI SPIInstance = new SPI(new SPI.Configuration(Cpu.Pin.GPIO_NONE, //latchPin,
				false, // active state
				0,     // setup time
				0,     // hold time 
				false, // clock idle state
				true,  /// clock edge
				10000,   // clock rate
				SPI.SPI_module.SPI1));
		static Settings.Settings oSettings;
        static LCDScreen oLCDScreenA = new LCDScreen(ref SPIInstance, Pins.GPIO_PIN_D7);
        static LCDScreen oLCDScreenB = new LCDScreen(ref SPIInstance, Pins.GPIO_PIN_D10);

		static Thread LCDThreadA;
        static Thread LCDThreadB;

        //static LEDMatrix.LEDMatrixControllerEyesDisplay oLEDMatrixController = new LEDMatrix.LEDMatrixControllerEyesDisplay(ref SPIInstance);
        //static Thread LEDMatrixThread;
        //static LEDMatrix.LEDMatrixReaderDisplayController oLEDMatrixReaderDisplayController = new LEDMatrix.LEDMatrixReaderDisplayController(ref SPIInstance);
        //static Thread LEDMatrixReaderDisplayThread;
		//static Thread LEDStripThead;
		
		static LabelPrinting oLabelPrinting;

		static Person.Person PersonScanned;

        static MakerRanger.Sensors.Buttons oButtons = new MakerRanger.Sensors.Buttons();

        static MakerRanger.Game.StartGame StartGameController = new Game.StartGame();

        private static RfidReader rfidReadera;
        private static RfidReader rfidReaderb;

		public static void Main()
		{
			Debug.EnableGCMessages(true);
          

			// Tie analogue ports high that are not used

			OutputPort dummy1 = new OutputPort(Pins.GPIO_PIN_A1, true);
			//OutputPort dummy3 = new OutputPort(Pins.GPIO_PIN_A3, true);
			OutputPort dummy4 = new OutputPort(Pins.GPIO_PIN_A4, true);

			//for (int i = 0; i < 10; i++)
			//{
			//    Thread.Sleep(1000);
			//    FanOutput.Write(! FanOutput.Read());
			//}

			//Switch lights on
			//oLEDStrips.Normal();

			//Start up the LCD thread first

            ThreadStart tsLCDB = new ThreadStart(oLCDScreenB.StartLCDThread);
            LCDThreadB = new Thread(tsLCDB);
            LCDThreadB.Start();
            oLCDScreenB.AddMessage(LCDScreen.LCDStates.Startup);
           // Thread.Sleep(10000);
			ThreadStart tsLCDA = new ThreadStart(oLCDScreenA.StartLCDThread);
			LCDThreadA = new Thread(tsLCDA);
            LCDThreadA.Start();
			oLCDScreenA.AddMessage(LCDScreen.LCDStates.Startup);

            oButtons.OnButtonADown += ButtonADown;
            oButtons.OnButtonBDown += ButtonBDown;

			//Create label printing object
			oLabelPrinting = new LabelPrinting();
			//Clear the printer
			oLabelPrinting.ResetPrinter();
			//2 Seconds after reset before printer available

            StartGameController.PlayerAisReady += PlayerAisReady;
            StartGameController.PlayerBisReady += PlayerBisReady;
            StartGameController.OnPlayersAreReady += PlayersAreReady;
            StartGameController.Enabled = true;


			// Add event handler for sticker take message
			oLabelPrinting.StickerPrinted += new NativeEventHandler(StickerPrintedEvent);

            rfidReadera = new RfidReader(Pins.GPIO_PIN_D9, SPIInstance, Pins.GPIO_PIN_A0);
            rfidReaderb = new RfidReader(Pins.GPIO_PIN_D8, SPIInstance);
            rfidReadera.ResetReaders();
            rfidReadera.InitReader();
            rfidReaderb.InitReader();


            rfidReadera.TagDetected += new RfidReader.RFIDEventHandler(TagDetecteda);
            rfidReaderb.TagDetected += new RfidReader.RFIDEventHandler(TagDetectedb);


            rfidReadera.enabled = true;
            rfidReaderb.enabled = true;



            //Set webserver up listening
            //Server oWebServer = new Server(80, false, "192.168.1.118", "255.255.255.0", "192.168.1.254", "NETDUINOPLUS");
            //oWebServer.AfterFileRecieved += new FileProcessed(FileToProcess);
            //oWebServer.AfterSettingsValueSet += new SetSettingsValue(SettingsSetFromEthernet);
		  

			//Start up the LED Matrix for eyes
            //ThreadStart tsLEDMatrix = new ThreadStart(oLEDMatrixController.StartLEDMatrixThread);
            //LEDMatrixThread = new Thread(tsLEDMatrix);
            //LEDMatrixThread.Start();

			//Start up the LED Matrix for reader
            //ThreadStart tsLEDMatrixReaderDisplay = new ThreadStart(oLEDMatrixReaderDisplayController.StartLEDMatrixThread);
            //LEDMatrixReaderDisplayThread = new Thread(tsLEDMatrixReaderDisplay);
            //LEDMatrixReaderDisplayThread.Start();

            //int imaxGuessValue = 99;
            //byte bqtyMaxNumber = 3;

		
            //oSettings = new Settings.Settings();
            //oSettings.Load("iniSettings",false);
            //// Get settings defaults
            //if (oSettings.GetValue("standard",@"maxGuessValue") == string.Empty)
            //{
            //     oSettings.SetValue("standard",@"maxGuessValue",imaxGuessValue);
            //}
            //if (oSettings.GetValue("standard", @"qtyMaxNumber") == string.Empty)
            //     {
            //    oSettings.SetValue("standard", @"qtyMaxNumber", bqtyMaxNumber);
            //}
            //oSettings.Save("iniSettings");

			//oLCDScreenA.MaxChooseValue = oSettings.GetValue("standard", @"maxGuessValue");
		   
			// Gets any previous guesses from SD card storage and/or initialises the secret number
		//	oPersistedStorage = new PersistedStorage(Int32.Parse( oSettings.GetValue("standard", @"maxGuessValue")), Byte.Parse( oSettings.GetValue("standard", @"qtyMaxNumber")));
			//oLEDMatrixController.PreviousGuesses = oPersistedStorage.GuessHistory;

			//Back stop monitor raises events for the card reaching the back of the reader timer Threshold voltage
			//oBackStopMonitor = new BackStopMonitor(new AnalogInput(AnalogChannels.ANALOG_PIN_A0), 300, 600);

			//Wire up event handlers
            //oBackStopMonitor.AnalogInterrupt += new NativeEventHandler(CardInsertedEvent);
            //oBackStopMonitor.AnalogInterruptCardRemoved += new NativeEventHandler(CardRemovedEvent);

			//oOpticalSensorArray.CardRead += new NativeEventHandler(CardReadEvent);
			
			//Sleep forever to keep static objects alive forever
			Thread.Sleep(Timeout.Infinite);
		}


        private static void PlayerAisReady(uint uint1, uint unit2, DateTime date)
        {
            oLCDScreenA.AddMessage(LCDScreen.LCDStates.GetReady);
        }


        private static void PlayerBisReady(uint uint1, uint unit2, DateTime date)
        {
            oLCDScreenB.AddMessage(LCDScreen.LCDStates.GetReady);
        }

        private static void PlayersAreReady(uint uint1, uint unit2, DateTime date)
        {
            oLCDScreenA.AddMessage(LCDScreen.LCDStates.StartingGame);
            oLCDScreenB.AddMessage(LCDScreen.LCDStates.StartingGame);
        }

		private static void FileToProcess(string filename, string username, string screenname, Int64 userid,short Guess) {
			//oLEDStrips.CaseLights(true);
			PersonScanned= new Person.Person() {UserName= username, UserID= userid, ScreenName=screenname, Guess=Guess};
			oLCDScreenA.AddMessage(LCDScreen.LCDStates.PersonDetected, PersonScanned);
		}


        private static void ButtonADown(uint uint1, uint uint2, DateTime date )
        {
            if (StartGameController.Enabled)
            {
                StartGameController.PlayerAReady = true;
            }

        }

        private static void ButtonBDown(uint uint1, uint uint2, DateTime date)
        {
            if (StartGameController.Enabled)
            {
                StartGameController.PlayerBReady = true;
            }

        }

		private static void SettingsSetFromEthernet(string MaxGuessValue, string MaxQtyValue)
		{
			//Reset the settings from the webserver request
			oSettings.SetValue("standard", @"maxGuessValue", MaxGuessValue);
			oSettings.SetValue("standard", @"qtyMaxNumber", MaxQtyValue);
			oLCDScreenA.MaxChooseValue = oSettings.GetValue("standard", @"maxGuessValue");
			oPersistedStorage.MaxGuessValue = int.Parse(MaxGuessValue);
			oPersistedStorage.qtyMaxNumber = byte.Parse(MaxQtyValue);
			oSettings.Save("iniSettings");

		   
		}

		private static void StickerPrintedEvent(uint data1, uint data2, DateTime time)
		{
			//Tell user to take sticker
			oLCDScreenA.AddMessage(LCDScreen.LCDStates.TakeSticker);
		}

        
		private static bool isSecretNumber(byte guess)
		{ 
			foreach (byte item in oPersistedStorage.SecretNumber)
			{
				if (item == guess)
				{ return true;}
			}
			return false;
		}

		

		// turns number in to string 1= 01 used by displays
		private static string ConvertByteToString(byte Number)
		{
			string NumberAsString = Number.ToString();
			if (NumberAsString.Length == 1)
			{
				//pad single character numbers
				NumberAsString = "0" + NumberAsString;
			}
			return NumberAsString;
		}

        private static void TagDetecteda(object sender, RFID.RFIDEventArgs e)
        {
            Debug.Print("Tag Detected a " + System.DateTime.Now.ToString());
        }

        private static void TagDetectedb(object sender, RFID.RFIDEventArgs e)
        {
            Debug.Print("Tag Detected b " + System.DateTime.Now.ToString());
        }
	}
}
