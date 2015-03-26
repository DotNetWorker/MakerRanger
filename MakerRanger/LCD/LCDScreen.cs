using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using MicroLiquidCrystal;
using System.Collections;

namespace MakerRanger
{
	// This class takes care of running the LCD screen
	//  it runs on a separate thread up dating the screen when it gets notified of "states" from the  
	//  main thread
	class LCDScreen
	{
	   
		// create the transfer provider
			private  Shifter74Hc595LcdTransferProvider shifter; 
		   
			// create the LCD interface
			private  Lcd lcd;

		   
	   // private static  lcd;
		//Other threads push messages into the q to be displayed
		private  System.Collections.Queue MessageQueue;
		public enum LCDStates
		{
			WelcomeMessages,
			Reading,
			Winner,
			Looser,
			YourGuessWas,
			Startup,
			NumberOverFifty,
			FirstNumberOver9,
			SecondNumberOver9,
			TakeSticker,
			CaseLightsOn,
			FriendDisplay,
			NoNumberDetected,
			SecretDisplay,
			PersonDetected,
            GetReady,
            ScanningHealth,
            FindThe,
            HealthCheckOK,
            WrongAnimalTryAgain,
            TestComplete,
            StartingGame,
            SinglePlayerMode,
            TwoPlayerMode
            }

		private LCDStates _CurrentState = LCDStates.WelcomeMessages;
		public LCDStates CurrentState
		{
			get { return _CurrentState; }
			set
			{
				if (value != _CurrentState)
				{
					_CurrentState = value;
				}
			}
		}

		private byte _LastGuessValue;
	  
		public byte LastGuessValue
		{
			get { return _LastGuessValue; }
			set
			{
				 _LastGuessValue = value;
			}
		}

		private ArrayList _secretNumbers;
		public ArrayList secretNumbers
		{
			get { return _secretNumbers; }
			set
			{
				_secretNumbers = value;
			}
		}
		public string MaxChooseValue { get; set; }

		public Person.Person PersonToDisplay  { get; set; }

		public LCDScreen()
		{
		   throw new NotImplementedException();
		}

		public LCDScreen(ref SPI SPIInstance, Cpu.Pin SpiSelect)
		{
			// TODO: Complete member initialization
			this.MaxChooseValue = MaxChooseValue;
			// Create instance of shift register
            shifter = new Shifter74Hc595LcdTransferProvider(ref SPIInstance, SPI_Devices.SPI1, SpiSelect,
				Shifter74Hc595LcdTransferProvider.BitOrder.MSBFirst);

			// Create new LCD instance and use shift register as a transport layer
			lcd =  new Lcd(shifter);
			
			LoadInsertCardSpecialChars();
			//LoadThumbsDownSpecialChars();

			// set up the LCD's number of columns and rows: 
			lcd.Begin(16, 2);
			
			MessageQueue = new System.Collections.Queue();
		}

		public void StartLCDThread()
		{
			//Loop forever showing what ever messages
			do
			{
				if (!MessageQueueEmpty())
				{
					this.CurrentState = (LCDStates)MessageQueue.Dequeue();
				}
				else
				{
					this.CurrentState = LCDStates.WelcomeMessages;
				}
				  ShowNextMessages(); 
			} while (true);
			
		}

		private void ShowNextMessages(){
		if (this.CurrentState == LCDStates.WelcomeMessages){
			WelcomeMessages();
		}
		else if (this.CurrentState == LCDStates.Reading)
		{
			ReadingCardMessage();
			
		}
		else if (this.CurrentState == LCDStates.Winner )
		{
			WinnerMessage();
		}
		else if (this.CurrentState == LCDStates.Looser)
		{
			LooserMessage();
		}
		else if (this.CurrentState == LCDStates.YourGuessWas)
		{
			YourGuessWas();
		}
		else if (this.CurrentState == LCDStates.Startup)
		{
			StartupMessage();
		}
		else if (this.CurrentState == LCDStates.NumberOverFifty)
		{
			NumberOverFifty();
		}
		else if (this.CurrentState == LCDStates.FirstNumberOver9)
		{
			FirstNumberOver9();
		}
		else if (this.CurrentState == LCDStates.SecondNumberOver9)
		{
			SecondNumberOver9();
		}
		else if (this.CurrentState == LCDStates.TakeSticker)
		{
			TearOffSticker();
		}
		else if (this.CurrentState == LCDStates.CaseLightsOn)
		{
		   PutCaseLightsOn();
		}
		else if (this.CurrentState == LCDStates.FriendDisplay)
		{
		   FriendDisplay();
		}
		else if (this.CurrentState == LCDStates.NoNumberDetected)
		{
			NoNumberDisplay();
		}
		else if (this.CurrentState == LCDStates.SecretDisplay)
		{
			SecretDisplay();
		}
		else if (this.CurrentState == LCDStates.PersonDetected)
		{
			DisplayKnownUser();
		}
        else if (this.CurrentState == LCDStates.GetReady)
		{
            GetReady();
		}
        else if (this.CurrentState == LCDStates.TestComplete)
		{
            TestComplete();
		}
            else if (this.CurrentState == LCDStates.WrongAnimalTryAgain)
		{
            WrongAnimalTryAgain();
		}
            else if (this.CurrentState == LCDStates.HealthCheckOK)
		{
            HealthCheckOK();
		}
            else if (this.CurrentState == LCDStates.FindThe)
		{
            FindThe();
		}
            else if (this.CurrentState == LCDStates.ScanningHealth)
		{
            ScanningHealth();
		}
            else if (this.CurrentState == LCDStates.StartingGame)
        {
            StartingGame();
        }
        else if (this.CurrentState == LCDStates.SinglePlayerMode)
        {
            SinglePlayerMode();
        }
        else if (this.CurrentState == LCDStates.TwoPlayerMode)
        {
            TwoPlayerMode();
        }
		else
		{
			SnoozeDisplay(100);
		}

          
            
            

		}

        private void TwoPlayerMode()
        {
            lcd.Visible = true;
            lcd.Backlight = true;
            lcd.Clear();
            lcd.SetCursorPosition(2, 0);
            lcd.Write("Two Player");
            lcd.SetCursorPosition(5, 1);
            lcd.Write("Mode");
            SnoozeDisplay(1000, true);
        }

        private void SinglePlayerMode()
        {
            lcd.Visible = true;
            lcd.Backlight = true;
            lcd.Clear();
            lcd.SetCursorPosition(2, 0);
            lcd.Write("Single Player");
            lcd.SetCursorPosition(5, 1);
            lcd.Write("Mode");
            SnoozeDisplay(1000, true);
        }
		private void DisplayKnownUser()
		{
		  if (this.PersonToDisplay!=null){
			lcd.Visible = true;
			lcd.Backlight = true;
			lcd.Clear();
			lcd.SetCursorPosition(0, 0);
			lcd.Write((this.PersonToDisplay.ScreenName + @"                                        ").Substring(0, 16));
			lcd.SetCursorPosition(3, 1);
			lcd.Write("Welcome!");
			if (this.PersonToDisplay.ScreenName.Length > 16)
			{
				//Long names scroll just the name
				SnoozeDisplay(1000, true);
				for (int i = 0; i < this.PersonToDisplay.ScreenName.Length-16; i++)
				{
				   lcd.SetCursorPosition(0, 0);
				   lcd.Write((this.PersonToDisplay.ScreenName + @"                                        ").Substring(i, 16));
				   SnoozeDisplay(600, true);
				}
			}
			else { SnoozeDisplay(1500, true);}
			 
			lcd.SetCursorPosition(0, 0);
			lcd.Write((this.PersonToDisplay.UserName + @"                                        ").Substring(0, 16));
			SnoozeDisplay(1500, true);
			lcd.SetCursorPosition(0, 0);
			lcd.Write((this.PersonToDisplay.ScreenName + @"                                        ").Substring(0, 16));
			SnoozeDisplay(1500, true);
		   
			LoadInsertCardSpecialChars();

			for (int i2 = 0; i2 < 8; i2++)
			{
				lcd.SetCursorPosition(0, 1);
				lcd.Write("  SUBMIT GUESS  ");
				SnoozeDisplay(800, true);
				if (!MessageQueueEmpty()) { break; }
				lcd.SetCursorPosition(0, 1);
				lcd.Write(">");
				
				lcd.Write(new byte[] { 0x03 }, 0, 1);

				lcd.Write(new byte[] { 0x00 }, 0, 1);
				for (int i = 0; i < 10; i++)
				{
					lcd.Write(new byte[] { 0x02 }, 0, 1);
				}
				lcd.Write(new byte[] { 0x01 }, 0, 1);
				lcd.Write(new byte[] { 0x05 }, 0, 1);

				lcd.Write("<");
				SnoozeDisplay(800, true);
			}
		  }
		}

		private void PutCaseLightsOn()
		{
			lcd.Visible = true;
			lcd.Backlight = true;
			lcd.Clear();
			lcd.SetCursorPosition(2, 0);
			lcd.Write("Case lights");
			lcd.SetCursorPosition(3, 1);
			lcd.Write("**** ON ****");
			SnoozeDisplay(1000, true);
		}

		private void SecretDisplay()
		{
			lcd.Visible = true;
			lcd.Backlight = true;
			lcd.Clear();
			lcd.SetCursorPosition(1, 0);
			string SecretListString = "";
				foreach (byte item in this.secretNumbers)
				{
					SecretListString += item.ToString() + " ";
				}
			lcd.Write("# " + SecretListString);
			lcd.SetCursorPosition(3, 1);
			lcd.Write("Sh!");
			SnoozeDisplay(1000, true);
		}

		private void FriendDisplay()
		{
			lcd.Visible = true;
			lcd.Backlight = true;
			lcd.Clear();
			lcd.SetCursorPosition(0, 0);
			lcd.Write("Paperbits Admin");
			lcd.SetCursorPosition(3, 1);
			lcd.Write("Welcome!");
			SnoozeDisplay(5000, true);
		}

        private void StartingGame()
        {
            lcd.Visible = true;
            lcd.Backlight = true;
            lcd.Clear();
            lcd.SetCursorPosition(0, 0);
            lcd.Write("Game Started");
            lcd.SetCursorPosition(0, 1);
            lcd.Write("watch Jake Snake ");
            SnoozeDisplay(2000, true);
            lcd.Write("              ");
            SnoozeDisplay(700, true);
            lcd.SetCursorPosition(0, 1);
            lcd.Write("get the animal   ");
            SnoozeDisplay(5000, true);
        }


		private void TearOffSticker()
		{
			lcd.Visible = true;
			lcd.Backlight = true;
			LoadUpArrowSpecialChars();

			lcd.Clear();
			lcd.SetCursorPosition(4, 0);
			lcd.Write("Take sticker");
			
			lcd.SetCursorPosition(4, 1);
			lcd.Write("tear upward");
			//arrow
			lcd.SetCursorPosition(0, 0);
			lcd.Write(new byte[] { 0x01 }, 0, 1);
			lcd.SetCursorPosition(1, 0);
			lcd.Write(new byte[] { 0x00 }, 0, 1);
			lcd.SetCursorPosition(2, 0);
			lcd.Write(new byte[] { 0x02 }, 0, 1);
			lcd.SetCursorPosition(1, 1);
			lcd.Write(new byte[] { 0x03 }, 0, 1);
			lcd.SetCursorPosition(0, 1);
			lcd.Write(new byte[] { 0xFF }, 0, 1);
			lcd.SetCursorPosition(2, 1);
			lcd.Write(new byte[] { 0xFF }, 0, 1);

			FlashDisplayBackLight(10, 600, 50);
			SnoozeDisplay(1000, true);
		}

		private void NoNumberDisplay()
		{
			lcd.Visible = true;
			lcd.Backlight = true;
			lcd.Clear();
			lcd.SetCursorPosition(4, 0);
			lcd.Write("Bad read");
			lcd.SetCursorPosition(1, 1);
			lcd.Write("try again");
			SnoozeDisplay(1000, true);
		}

		// Sleep the thread but keep checking for any message in the queue to display
		private void SnoozeDisplay(int SnoozeTime, Boolean AllowBreak=true )
		{
			int index = SnoozeTime / 100;
			do
			{
				//Sleep the thread for 100ms as that is adequate to respond to changes
				System.Threading.Thread.Sleep(100);
				if (! MessageQueueEmpty() & AllowBreak)
				{
					// If we have a message to display stop snoozing
					break;
				}
			index -= 1;
			} while (index >0);
		}

		private void FlashDisplayBackLight(short NumberOfFlashes, int FlashOnTime, int FlashOffTime)
		{
			while (NumberOfFlashes > 0 )
			{
				lcd.Backlight = false;
				SnoozeDisplay(FlashOffTime);
				lcd.Backlight = true;
				SnoozeDisplay(FlashOnTime);
				NumberOfFlashes -= 1;
				if (!MessageQueueEmpty()) { break; }
			}
		}
		private void FlashDisplayText(short NumberOfFlashes, int FlashOnTime, int FlashOffTime)
		{
			while (NumberOfFlashes > 0)
			{
				lcd.Visible = false;
				
				SnoozeDisplay(FlashOffTime);
				lcd.Visible = true;
				SnoozeDisplay(FlashOnTime);
				NumberOfFlashes -= 1;
				if (!MessageQueueEmpty()) { break; }
			}
		}
	   
		private void ScrollMessage(int Scrollby,short Speed )
		{
			int index = 0;
			while (index < Scrollby)
			{
				if (!MessageQueueEmpty())
				{
					break;
				}
				lcd.ScrollDisplayRight();
				index += 1;
				SnoozeDisplay(Speed);
			}
		}
		private void StartupMessage()
		{
			lcd.BlinkCursor = true;
			// Turn display on, turn back light on, hide small cursor, show big blinking cursor
            lcd.Visible = true;
            lcd.Backlight = true;
            lcd.Clear();
            lcd.SetCursorPosition(1, 0);
            lcd.Write("Starting up...");
            lcd.SetCursorPosition(0, 1);
            for (int i = 0; i < 15; i++)
            {
                System.Threading.Thread.Sleep(50);
                lcd.SetCursorPosition(i, 1);
                //lcd.Write("#");
                lcd.Write(new byte[] { 0xFF }, 0, 1);

            }
            for (int i = 0; i < 15; i++)
            {
                System.Threading.Thread.Sleep(30);
                lcd.SetCursorPosition(i, 1);
                lcd.Write(" ");
            }
			SnoozeDisplay(1500);
		 }


		private void ReadingCardMessage()
		{
			lcd.Visible = true;
			lcd.Backlight = true;
			lcd.Clear();
			lcd.SetCursorPosition(1,0);
			lcd.Write("Reading card...");
			lcd.SetCursorPosition(0, 1);
			//lcd.Write("################");
			for (int i = 0; i < 15; i++)
			{
				System.Threading.Thread.Sleep(50);
				lcd.SetCursorPosition(i, 1);
				//lcd.Write("#");
				lcd.Write(new byte[] {0xFF},0,1);
			   
			}
			for (int i = 0; i < 15; i++)
			{
				System.Threading.Thread.Sleep(30);
				lcd.SetCursorPosition(i,1);
				lcd.Write(" ");
			}
			SnoozeDisplay(9000, true);
		}

		private void YourGuessWas()
		{
			lcd.Backlight = true;
			lcd.Visible = true;
			lcd.Clear();
			lcd.SetCursorPosition(1,0);
			lcd.Write("Your guess: " + this.LastGuessValue.ToString());
			SnoozeDisplay(3000, false);
			if (this.LastGuessValue == 42)
			{
				lcd.Clear();
				lcd.SetCursorPosition(1, 0);
				lcd.Write("Life,");
				SnoozeDisplay(500, false);
				lcd.SetCursorPosition(1, 1);
				lcd.Write("the Universe");
				SnoozeDisplay(750, false);
				lcd.Clear();
				lcd.SetCursorPosition(1, 1);
				lcd.Write("and everything...");
				SnoozeDisplay(750, false);

			}
		}

		private void FirstNumberOver9()
		{
			lcd.Backlight = true;
			lcd.Visible = true;
			lcd.Clear();
			lcd.SetCursorPosition(1, 0);
			lcd.Write("9+ invalid dig1");
			lcd.SetCursorPosition(1, 1);
			lcd.Write("please check");
			SnoozeDisplay(1000, false);
		}
       private void FindThe()
		{
			lcd.Backlight = true;
			lcd.Visible = true;
			lcd.Clear();
			lcd.SetCursorPosition(1, 0);
			lcd.Write("Find the");
			lcd.SetCursorPosition(1, 1);
			lcd.Write("x animal");
			SnoozeDisplay(1000, false);
		}
          private void HealthCheckOK()
		{
			lcd.Backlight = true;
			lcd.Visible = true;
			lcd.Clear();
			lcd.SetCursorPosition(1, 0);
			lcd.Write("Health check OK");
			lcd.SetCursorPosition(1, 1);
			lcd.Write("put animal to wild");
			SnoozeDisplay(1000, false);
              //happy face?
		}   
        private void WrongAnimalTryAgain()
		{
			lcd.Backlight = true;
			lcd.Visible = true;
			lcd.Clear();
			lcd.SetCursorPosition(1, 0);
			lcd.Write("Wrong animal");
			lcd.SetCursorPosition(1, 1);
			lcd.Write("try again");
			SnoozeDisplay(1000, false);
            //sad face?
		}   
        
        private void TestComplete()
		{
			lcd.Backlight = true;
			lcd.Visible = true;
			lcd.Clear();
			lcd.SetCursorPosition(1, 0);
			lcd.Write("Test");
			lcd.SetCursorPosition(1, 1);
			lcd.Write("Complete!");
			SnoozeDisplay(1000, false);
		}      
           
        private void ScanningHealth()
		{
            lcd.Visible = true;
            lcd.Backlight = true;
            lcd.Clear();
            lcd.SetCursorPosition(1, 0);
            lcd.Write("Health scanning...");
            lcd.SetCursorPosition(0, 1);
            //lcd.Write("################");
            for (int i = 0; i < 15; i++)
            {
                System.Threading.Thread.Sleep(50);
                lcd.SetCursorPosition(i, 1);
                //lcd.Write("#");
                lcd.Write(new byte[] { 0xFF }, 0, 1);

            }
            for (int i = 0; i < 15; i++)
            {
                System.Threading.Thread.Sleep(30);
                lcd.SetCursorPosition(i, 1);
                lcd.Write(" ");
            }
            SnoozeDisplay(1000, false);
            lcd.Clear();
            lcd.SetCursorPosition(1, 0);
            lcd.Write("Healthy animal, ");
            lcd.SetCursorPosition(1, 1);
            lcd.Write("return to wild");
            SnoozeDisplay(3000, false);
		}

        private void PressToScan()
        {
            lcd.Backlight = true;
            lcd.Visible = true;
            lcd.Clear();
            lcd.SetCursorPosition(1, 0);
            lcd.Write("Press button");
            lcd.SetCursorPosition(1, 1);
            lcd.Write("to health scan");
            SnoozeDisplay(1000, false);
        }

		private void SecondNumberOver9()
		{
			lcd.Backlight = true;
			lcd.Visible = true;
			lcd.Clear();
			lcd.SetCursorPosition(1, 0);
			lcd.Write("9+ invalid dig2");
			lcd.SetCursorPosition(1, 1);
			lcd.Write("please check");
			SnoozeDisplay(1000, false);
		}

		private void NumberOverFifty()
		{
			lcd.Backlight = true;
			lcd.Visible = true;
			lcd.Clear();
			lcd.SetCursorPosition(1, 0);
			lcd.Write("Guess must be");
			lcd.SetCursorPosition(1, 1);
			lcd.Write("less than fifty");
			SnoozeDisplay(3000, false);
		}

        private void GetReady()
        {
            lcd.Backlight = true;
            lcd.Visible = true;
            lcd.Clear();
            lcd.SetCursorPosition(0, 0);
            lcd.Write("Waiting for next");
            lcd.SetCursorPosition(0, 1);
            lcd.Write("player to start");
            SnoozeDisplay(300000, true);
        }

		private void WinnerMessage()
		{
			lcd.Backlight = true;
			lcd.Visible = true;
			lcd.Clear();
			
			lcd.SetCursorPosition(0,1);
			lcd.Write(" *** WINNER! ***");
			SnoozeDisplay(20000, false);
		}

		private void LooserMessage()
		{
			LoadThumbsDownSpecialChars();
			lcd.Backlight = true;
			lcd.Visible = true;
			lcd.Clear();

		   
			lcd.SetCursorPosition(0, 0);
			lcd.Write(new byte[] { 0x00 }, 0, 1);
			lcd.SetCursorPosition(1, 0);

			lcd.Write(new byte[] { 0x02 }, 0, 1);
			lcd.SetCursorPosition(2, 0);

			lcd.Write(new byte[] { 0x04 }, 0, 1);
			lcd.SetCursorPosition(0, 1);

			lcd.Write(new byte[] { 0x01 }, 0, 1);
			lcd.SetCursorPosition(1, 1);

			lcd.Write(new byte[] { 0x03 },  0, 1);
			lcd.SetCursorPosition(2, 1);

			lcd.Write(new byte[] { 0x05 },  0, 1);

			lcd.SetCursorPosition(4,0);
			lcd.Write("unlucky");
			lcd.SetCursorPosition(5,1);
			lcd.Write("this time!");
			SnoozeDisplay(6000);
		}

		public void WelcomeMessages()
		{
			do
			{
				if (!MessageQueueEmpty()) { break; }
				lcd.BlinkCursor = false;
				lcd.Backlight = true;
				lcd.Visible = true;
				lcd.Clear();
				lcd.SetCursorPosition(5,0);
				lcd.Write("Maker");
				lcd.SetCursorPosition(5,1);
				lcd.Write("Ranger");
				if (!MessageQueueEmpty()) { break; }
				SnoozeDisplay(3500);
				lcd.Clear();

				lcd.Write("Newcastle 2015");
				
				lcd.SetCursorPosition(0,1);
				lcd.Write("Maker Faire UK");
				if (!MessageQueueEmpty()) { break; }
				SnoozeDisplay(1500);
				if (!MessageQueueEmpty()) { break; }
				FlashDisplayBackLight(2, 300, 100);
				if (!MessageQueueEmpty()) { break; }
				ScrollMessage(17, 200);
				if (!MessageQueueEmpty()) { break; }

				lcd.Clear();
                //lcd.Home();
                //lcd.SetCursorPosition(0, 0);
                //lcd.Write("25-26 April 2015");
                //SnoozeDisplay(1000);
                //for (int i = 0; i < 15; i++)
                //{
                //    System.Threading.Thread.Sleep(50);
                //    lcd.SetCursorPosition(i, 1);
                //    lcd.Write(new byte[] { 0xFF }, 0, 1);
                //}
                //for (int i = 0; i < 15; i++)
                //{
                //    System.Threading.Thread.Sleep(30);
                //    lcd.SetCursorPosition(i, 1);
                //    if (i == 3) { lcd.Write("L"); }
                //    else if (i == 4) { lcd.Write("i"); }
                //    else if (i == 5) { lcd.Write("f"); }
                //    else if (i == 6) { lcd.Write("e"); }
                //    else if (i == 8) { lcd.Write("C"); }
                //    else if (i == 9) { lcd.Write("e"); }
                //    else if (i == 10) { lcd.Write("n"); }
                //    else if (i == 11) { lcd.Write("t"); }
                //    else if (i == 12) { lcd.Write("r"); }
                //    else if (i == 13) { lcd.Write("e"); }
                //    else { lcd.Write(" "); }

                //}
                
                //SnoozeDisplay(3000);
                //lcd.Clear();
                //lcd.Home();
                //lcd.SetCursorPosition(0, 0);
                //lcd.Write("Qualified ranger,");
                //lcd.SetCursorPosition(0, 1);
                //lcd.Write("by passing test");

                SnoozeDisplay(3000);
                lcd.Clear();
                lcd.Home();
                lcd.SetCursorPosition(0, 0);
                lcd.Write("to be a ranger");
                lcd.SetCursorPosition(0, 1);
                lcd.Write("pass this test");


				SnoozeDisplay(3000);
				if (!MessageQueueEmpty()) { break; }
				ScrollMessage(17, 50);
				if (!MessageQueueEmpty()) { break; }

				lcd.Clear();
				lcd.Home();
				lcd.Write("Jake snake shows");
				lcd.SetCursorPosition(0,1);
                lcd.Write("next to scan");
				if (!MessageQueueEmpty()) { break; }
				SnoozeDisplay(3000);
				if (!MessageQueueEmpty()) { break; }
				lcd.Clear();
				lcd.SetCursorPosition(0,0);
				lcd.Write("Get ready to...");
				if (!MessageQueueEmpty()) { break; }
				lcd.SetCursorPosition(0,1);
				SnoozeDisplay(500);
				if (!MessageQueueEmpty()) { break; }
                for (int i = 0; i < 15; i++)
                {
                    System.Threading.Thread.Sleep(50);
                    lcd.SetCursorPosition(i, 1);
                    lcd.Write(new byte[] { 0xFF }, 0, 1);
                }
                for (int i = 0; i < 15; i++)
                {
                    System.Threading.Thread.Sleep(30);
                    lcd.SetCursorPosition(i, 1);
                    if (i == 5)       {lcd.Write("S"); }
                    else if (i == 6) { lcd.Write("C"); }
                    else if (i == 7) { lcd.Write("A"); }
                    else if (i == 8) { lcd.Write("N"); }
                    else {  lcd.Write(" "); }
                    
                }
               
				SnoozeDisplay(3000);
				if (!MessageQueueEmpty()) { break; }

				//--LoadInsertCardSpecialChars();
				lcd.Clear();
				SnoozeDisplay(500);
				if (!MessageQueueEmpty()) { break; }
			 
				lcd.SetCursorPosition(2,0);
				lcd.Write("PRESS BUTTON");
				if (!MessageQueueEmpty()) { break; }
				lcd.SetCursorPosition(4,1);
				
				lcd.Write("TO START");
								
				
				if (!MessageQueueEmpty()) { break; }
				SnoozeDisplay(1000);
				if (!MessageQueueEmpty()) { break; }
				FlashDisplayText(10, 400, 300);
				if (!MessageQueueEmpty()) { break; }
				lcd.Clear();
				SnoozeDisplay(1000);
				if (!MessageQueueEmpty()) { break; }
			   
			} while (MessageQueueEmpty());

		}

		public void AddMessage(LCDStates Message)
		{
			// threading safety lock the resource
			lock (MessageQueue)
			{
				MessageQueue.Enqueue(Message);
			}
		}

		public void AddMessage(LCDStates Message, Person.Person PersonToShow)
		{
			// threading safety lock the resource
			lock (MessageQueue)
			{
				this.PersonToDisplay = PersonToShow;
			}
			AddMessage(Message);
		}


		public Boolean MessageQueueEmpty()
		{
			if (MessageQueue.Count >0)
			{
				return false;
			}
			else
			{
				return true; 
			}
		}


		public void LoadUpArrowSpecialChars()
		{
			byte[] buffer = new byte[] {   0x1F,0x1B,0x11,0x00,0x00,0x00,0x00,0x00, 
										   0x1F,0x1F,0x1F,0x1F,0x1E,0x1C,0x18,0x10, 
										   0x1F,0x1F,0x1F,0x1F,0x0F,0x07,0x03,0x01,
										   0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F
										   
										   
									   };

			// Load custom characters to display CGRAM
			for (int i = 0; i < 4; i++)
			{
				lcd.CreateChar(i, buffer, i * 8);
			}


		}


		public void LoadThumbsDownSpecialChars()
		{
			byte[] buffer = new byte[] {   0x01,0x0A,0x0B,0x04,0x0B,0x04,0x0B,0x04, 
										   0x0B,0x00,0x00,0x00,0x00,0x00,0x00,0x00, 
										   0x1E,0x01,0x00,0x00,0x00,0x00,0x00,0x00,
										   0x00,0x1E,0x08,0x11,0x12,0x12,0x0C,0x00,
										   0x00,0x10,0x0E,0x0A,0x0A,0x0A,0x0A,0x0A,
										   0x06,0x08,0x10,0x00,0x00,0x00,0x00,0x00 
										   
									   };

			// Load custom characters to display CGRAM
			for (int i = 0; i < 6; i++)
			{
				lcd.CreateChar(i, buffer, i * 8);
			}


		}
	   

	public void LoadInsertCardSpecialChars(){
		byte[] buffer = new byte[] {        0x00, 0x00, 0x03, 0x64, 0x64, 0x03, 0x00, 0x00, 
											0x00, 0x00, 0x18, 0x04, 0x04, 0x18, 0x00, 0x00, 
											0x00, 0x00, 0x1F, 0x00, 0x00, 0x1F, 0x00, 0x00,
											0x00, 0x04, 0x02, 0x01, 0x02, 0x04, 0x00, 0x00,
											0x00, 0x00, 0x02, 0x01, 0x02, 0x00, 0x00, 0x00,
											0x00, 0x04, 0x08, 0x10, 0x08, 0x04, 0x00, 0x00, 
											0x00, 0x00, 0x08, 0x10, 0x08, 0x00, 0x00, 0x00
									   };

		// Load custom characters to display CGRAM
		for (int i = 0; i < 7; i++)
		{
			lcd.CreateChar(i, buffer, i * 8);
		}
	   
	
	}

	}
}
