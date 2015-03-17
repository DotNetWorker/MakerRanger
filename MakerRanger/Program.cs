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
		static BackStopMonitor oBackStopMonitor;
		static PersistedStorage oPersistedStorage;
		static MakerRanger.LEDStripLighting.LEDStrips oLEDStrips = new LEDStripLighting.LEDStrips();
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
		static LCDScreen oLCDScreen = new LCDScreen(ref SPIInstance);
		static Thread LCDThread;
		static LEDMatrix.LEDMatrixControllerEyesDisplay oLEDMatrixController = new LEDMatrix.LEDMatrixControllerEyesDisplay(ref SPIInstance);
		static Thread LEDMatrixThread;
		static LEDMatrix.LEDMatrixReaderDisplayController oLEDMatrixReaderDisplayController = new LEDMatrix.LEDMatrixReaderDisplayController(ref SPIInstance);
		static Thread LEDMatrixReaderDisplayThread;
		static byte PreviousGuess;
		static Thread LEDStripThead;
		
		static bool CaseLightsOn = false;
		static Boolean WaitingForCard = true;
		static LabelPrinting oLabelPrinting;
		static OutputPort FanOutput = new OutputPort(Pins.GPIO_PIN_A2, false);
		static FanControl.FanController oFanController = new FanControl.FanController(ref FanOutput);

		static Person.Person PersonScanned;
	  

 

		public static void Main()
		{
			Debug.EnableGCMessages(true);


			// Tie analogue ports high that are not used

			OutputPort dummy1 = new OutputPort(Pins.GPIO_PIN_A1, true);
			OutputPort dummy3 = new OutputPort(Pins.GPIO_PIN_A3, true);
			OutputPort dummy4 = new OutputPort(Pins.GPIO_PIN_A4, true);

			//for (int i = 0; i < 10; i++)
			//{
			//    Thread.Sleep(1000);
			//    FanOutput.Write(! FanOutput.Read());
			//}

			//Switch lights on
			oLEDStrips.Normal();

			//Start up the LCD thread first
			ThreadStart tsLCD = new ThreadStart(oLCDScreen.StartLCDThread);
			LCDThread = new Thread(tsLCD);
			LCDThread.Start();
			oLCDScreen.AddMessage(LCDScreen.LCDStates.Startup);

			//Create label printing object
			oLabelPrinting = new LabelPrinting();
			//Clear the printer
			oLabelPrinting.ResetPrinter();
			//2 Seconds after reset before printer available
		  
			// Add event handler for sticker take message
			oLabelPrinting.StickerPrinted += new NativeEventHandler(StickerPrintedEvent);

			Server oWebServer = new Server(80, false, "192.168.1.118", "255.255.255.0", "192.168.1.254", "NETDUINOPLUS");
			oWebServer.AfterFileRecieved += new FileProcessed(FileToProcess);
			oWebServer.AfterSettingsValueSet += new SetSettingsValue(SettingsSetFromEthernet);
		  

			//Start up the LED Matrix for eyes
			ThreadStart tsLEDMatrix = new ThreadStart(oLEDMatrixController.StartLEDMatrixThread);
			LEDMatrixThread = new Thread(tsLEDMatrix);
			LEDMatrixThread.Start();

			//Start up the LED Matrix for reader
			ThreadStart tsLEDMatrixReaderDisplay = new ThreadStart(oLEDMatrixReaderDisplayController.StartLEDMatrixThread);
			LEDMatrixReaderDisplayThread = new Thread(tsLEDMatrixReaderDisplay);
			LEDMatrixReaderDisplayThread.Start();

			int imaxGuessValue = 99;
			byte bqtyMaxNumber = 3;

		
			oSettings = new Settings.Settings();
			oSettings.Load("iniSettings",false);
			// Get settings defaults
			if (oSettings.GetValue("standard",@"maxGuessValue") == string.Empty)
			{
				 oSettings.SetValue("standard",@"maxGuessValue",imaxGuessValue);
			}
			if (oSettings.GetValue("standard", @"qtyMaxNumber") == string.Empty)
				 {
				oSettings.SetValue("standard", @"qtyMaxNumber", bqtyMaxNumber);
			}
			oSettings.Save("iniSettings");

			oLCDScreen.MaxChooseValue = oSettings.GetValue("standard", @"maxGuessValue");
		   
			// Gets any previous guesses from SD card storage and/or initialises the secret number
			oPersistedStorage = new PersistedStorage(Int32.Parse( oSettings.GetValue("standard", @"maxGuessValue")), Byte.Parse( oSettings.GetValue("standard", @"qtyMaxNumber")));
			oLEDMatrixController.PreviousGuesses = oPersistedStorage.GuessHistory;

			//Back stop monitor raises events for the card reaching the back of the reader timer Threshold voltage
			oBackStopMonitor = new BackStopMonitor(new AnalogInput(AnalogChannels.ANALOG_PIN_A0), 300, 600);

			//Wire up event handlers
			oBackStopMonitor.AnalogInterrupt += new NativeEventHandler(CardInsertedEvent);
			oBackStopMonitor.AnalogInterruptCardRemoved += new NativeEventHandler(CardRemovedEvent);

			oOpticalSensorArray.CardRead += new NativeEventHandler(CardReadEvent);
			
			//Sleep forever to keep static objects alive forever
			Thread.Sleep(Timeout.Infinite);
		}

		private static void FileToProcess(string filename, string username, string screenname, Int64 userid,short Guess) {
			oLEDStrips.CaseLights(true);
			PersonScanned= new Person.Person() {UserName= username, UserID= userid, ScreenName=screenname, Guess=Guess};
			oLCDScreen.AddMessage(LCDScreen.LCDStates.PersonDetected, PersonScanned);
		}

						
		private static void SettingsSetFromEthernet(string MaxGuessValue, string MaxQtyValue)
		{
			//Reset the settings from the webserver request
			oSettings.SetValue("standard", @"maxGuessValue", MaxGuessValue);
			oSettings.SetValue("standard", @"qtyMaxNumber", MaxQtyValue);
			oLCDScreen.MaxChooseValue = oSettings.GetValue("standard", @"maxGuessValue");
			oPersistedStorage.MaxGuessValue = int.Parse(MaxGuessValue);
			oPersistedStorage.qtyMaxNumber = byte.Parse(MaxQtyValue);
			oSettings.Save("iniSettings");

		   
		}

		private static void StickerPrintedEvent(uint data1, uint data2, DateTime time)
		{
			//Tell user to take sticker
			oLCDScreen.AddMessage(LCDScreen.LCDStates.TakeSticker);
		}


		private static void CardRemovedEvent(uint data1, uint data2, DateTime time)
		{
			if (CaseLightsOn)
			{
				oLEDStrips.CaseLights(false);
				CaseLightsOn = false;
			}
			//Restart the card reader display as we shut it down while we read the card
			oLEDMatrixReaderDisplayController.StartupDisplayDriver();
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

		// Fires when a card is detected by the detector at the back of the unit
		private static void CardInsertedEvent(uint data1, uint data2, DateTime time)
		{
			// WaitingForCard Flag is used to prevent us reacting to multiple events, card withdrawn to clear flag
			if (WaitingForCard)
			{
				WaitingForCard = false;
				Debug.Print("Card Detected");
				//reduce interference while we read by shutting down the driver chip that chucks out lots of rubbish RF wise
				oLEDMatrixReaderDisplayController.ShutdownDisplayDriver();

				// if this is the current number to guess raise an success event else failed guess event
				oLCDScreen.AddMessage(LCDScreen.LCDStates.Reading);
				oOpticalSensorArray.ReadValue();
				
			}
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

		// Event raised after the value of the card reader has been read
		private static void CardReadEvent(uint CardReaderValue1, uint CardReaderValue2, DateTime time)
		{
			oLEDStrips.CaseLights(true);
			CaseLightsOn = true;

			Byte DetectedNumber = (Byte)(10 * CardReaderValue1 + (CardReaderValue2));
			if (DetectedNumber == 165)
			{
				oLCDScreen.AddMessage(LCDScreen.LCDStates.CaseLightsOn);
				Debug.Print("Lights on detected");
				oLEDStrips.CaseLights(true);
				CaseLightsOn = true;
			
			}
			else if ((DetectedNumber == 132) & (PreviousGuess == 132))
			{
				oLCDScreen.secretNumbers =oPersistedStorage.SecretNumber;
				oLCDScreen.AddMessage(LCDScreen.LCDStates.SecretDisplay);
				Debug.Print("Friend detected twice");
			}
			else if (DetectedNumber == 132)
			{
				oLCDScreen.AddMessage(LCDScreen.LCDStates.FriendDisplay);
				Debug.Print("Friend detected");
				Debug.Print("Printer Reset");
				oLabelPrinting.ResetPrinter();
				PreviousGuess = DetectedNumber;
			}
			else if ((CardReaderValue1 > 9 | CardReaderValue2 > 9) | DetectedNumber > int.Parse(oSettings.GetValue("standard","maxGuessValue")))
			{
				// one of the values read is out side the range we accept for this application..
				oLCDScreen.AddMessage(LCDScreen.LCDStates.NumberOverFifty);
				Debug.Print("Wrong number number over " + oSettings.GetValue("standard"," maxGuessValue"));
			}
			else if (CardReaderValue1 > 9)
			{
				// one of the values read is out side the range we accept for this application..
				oLCDScreen.AddMessage(LCDScreen.LCDStates.FirstNumberOver9);
				Debug.Print("Wrong number first digit too large");
			}
			else if (CardReaderValue2 > 9)
			{
				// one of the values read is out side the range we accept for this application..
				oLCDScreen.AddMessage(LCDScreen.LCDStates.SecondNumberOver9);
				Debug.Print("Wrong number second digit too large");
			}
			else if (DetectedNumber == 0)
			{
				// one of the values read is out side the range we accept for this application..
				oLCDScreen.AddMessage(LCDScreen.LCDStates.NoNumberDetected);
				Debug.Print("Bad read");
			}
			else
			{
				oPersistedStorage.WriteToGuessHistory(DetectedNumber);
				oPersistedStorage.WriteToGuessHistoryLog(DetectedNumber);
				
				oLEDMatrixController.PreviousGuesses = oPersistedStorage.GuessHistory;
				oLEDMatrixController.LastGuessValue = DetectedNumber;

				PreviousGuess = DetectedNumber;
				oLCDScreen.LastGuessValue = DetectedNumber;
				oLCDScreen.AddMessage(LCDScreen.LCDStates.YourGuessWas);

				if (isSecretNumber(DetectedNumber))
				{
					//Guessed the secret number, print label and raise event to let other object know what to do
					Debug.Print("Correct Guess");
					LEDStripThead = new Thread(delegate() { oLEDStrips.WinnerSequence(); });
					LEDStripThead.Start();
					Thread FanWinnerThread = new Thread(delegate() { oFanController.InflateMan(); });
					FanWinnerThread.Start();
			   
					oLCDScreen.AddMessage(LCDScreen.LCDStates.Winner);
					oLEDMatrixController.CurrentDisplayMode = LEDMatrix.LEDMatrixControllerEyesDisplay.MatrixModeType.WinnerSequence;

					if (PersonScanned == null) {
						oLabelPrinting.RecallFormAndPrint(8000,"03", oPersistedStorage, ConvertByteToString(DetectedNumber), true);
					
					} else {
						oLabelPrinting.RecallFormAndPrint(3000,"04", oPersistedStorage, ConvertByteToString(DetectedNumber), true);
					}


					//Thread thread = new Thread(() => oLabelPrinting.RecallFormAndPrint("03", oPersistedStorage, ConvertByteToString(DetectedNumber), true));
					//thread.Start();

					oPersistedStorage.GenerateNewSecretNumber();
					//Wait for the fan to finish
					//FanWinnerThread.Join();
				}
				else
				{
					oLEDMatrixController.CurrentDisplayMode = LEDMatrix.LEDMatrixControllerEyesDisplay.MatrixModeType.ThisGuess;

					//Failed guess raise event to let other objects know we failed
					Debug.Print("Failed Guess");
					LEDStripThead = new Thread(delegate() { oLEDStrips.LooserSequence(); });
					LEDStripThead.Start();
					oLCDScreen.AddMessage(LCDScreen.LCDStates.Looser);
					//Thread threadJoinMethodThread = new Thread( ThreadJoinMethod);
					//threadJoinMethodThread.Start();
					if (PersonScanned == null)
					{
						oLabelPrinting.RecallFormAndPrint(9000,"03", oPersistedStorage, ConvertByteToString(DetectedNumber), false);
					}
					else {
						oLabelPrinting.RecallFormAndPrint(3000,"04", oPersistedStorage, ConvertByteToString(DetectedNumber), false);
					}
					//Thread thread = new Thread(() => oLabelPrinting.RecallFormAndPrint("03", oPersistedStorage, ConvertByteToString(DetectedNumber), false));
					//thread.Start();
					//oLEDMatrixController.CurrentDisplayMode = LEDMatrix.LEDMatrixController.MatrixModeType.LooserSequence;
				}
				//Clear person scanned

				

			}
			oLCDScreen.PersonToDisplay = null;
			PersonScanned = null;
			WaitingForCard = true;
		}
	}
}
