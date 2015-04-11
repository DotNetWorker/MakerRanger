using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using MicroLiquidCrystal;
using System.Collections;
using MakerRanger.LCD;

namespace MakerRanger
{
    // This class takes care of running the LCD screen
    //  it runs on a separate thread up dating the screen when it gets notified of "states" from the  
    //  main thread
    class LCDScreen
    {

        // create the transfer provider
        private Shifter74Hc595LcdTransferProvider shifter;

        // create the LCD interface
        private Lcd lcd;


        // private static  lcd;
        //Other threads push messages into the q to be displayed
        private System.Collections.Queue MessageQueue;
        public enum LCDStates
        {
            WelcomeMessages,
            Startup,
            PressToNextAnimal,
            TakeSticker,
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

        private LCDMessage _CurrentState = new LCDMessage(LCDStates.WelcomeMessages, null);
        public LCDMessage CurrentState
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

        public LCDScreen()
        {
            throw new NotImplementedException();
        }

        public LCDScreen(ref SPI SPIInstance, Cpu.Pin SpiSelect)
        {

            // Create instance of shift register
            shifter = new Shifter74Hc595LcdTransferProvider(ref SPIInstance, SPI_Devices.SPI1, SpiSelect,
                Shifter74Hc595LcdTransferProvider.BitOrder.MSBFirst);

            // Create new LCD instance and use shift register as a transport layer
            lcd = new Lcd(shifter);

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
                if (!isMessageQueueEmpty())
                {
                    this.CurrentState = (LCDMessage)MessageQueue.Dequeue();
                }
                else
                {
                    this.CurrentState.LCDState = LCDStates.WelcomeMessages;
                }
                ShowNextMessages();
            } while (true);

        }

        private void ShowNextMessages()
        {
            if (this.CurrentState.LCDState == LCDStates.WelcomeMessages)
            {
                WelcomeMessages();
            }
            else if (this.CurrentState.LCDState == LCDStates.Startup)
            {
                StartupMessage();
            }
            else if (this.CurrentState.LCDState == LCDStates.TakeSticker)
            {
                TearOffSticker();
            }
            else if (this.CurrentState.LCDState == LCDStates.GetReady)
            {
                GetReady();
            }
            else if (this.CurrentState.LCDState == LCDStates.TestComplete)
            {
                TestComplete(this.CurrentState.MessageArgument);
            }
            else if (this.CurrentState.LCDState == LCDStates.WrongAnimalTryAgain)
            {
                WrongAnimal(this.CurrentState.MessageArgument);
            }
            else if (this.CurrentState.LCDState == LCDStates.HealthCheckOK)
            {
                HealthCheckOK();
            }
            else if (this.CurrentState.LCDState == LCDStates.FindThe)
            {
                FindThe(this.CurrentState.MessageArgument);
            }
            else if (this.CurrentState.LCDState == LCDStates.ScanningHealth)
            {
                ScanningHealth();
            }
            else if (this.CurrentState.LCDState == LCDStates.StartingGame)
            {
                StartingGame();
            }
            else if (this.CurrentState.LCDState == LCDStates.SinglePlayerMode)
            {
                SinglePlayerMode();
            }
            else if (this.CurrentState.LCDState == LCDStates.TwoPlayerMode)
            {
                TwoPlayerMode();
            }
            else if (this.CurrentState.LCDState == LCDStates.PressToNextAnimal)
            {
                PressToScan(this.CurrentState.MessageArgument);
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


        private void StartingGame()
        {
            lcd.Visible = true;
            lcd.Backlight = true;
            lcd.Clear();
            lcd.SetCursorPosition(0, 0);
            lcd.Write("Starting test");
            SnoozeDisplay(2000, false);
            lcd.SetCursorPosition(0, 0);
            lcd.Write("             ");
            lcd.SetCursorPosition(6, 0);
            lcd.Write("three");
            SnoozeDisplay(1200, false);
            lcd.SetCursorPosition(6, 0);
            lcd.Write("two  ");
            SnoozeDisplay(1200, false);
            lcd.SetCursorPosition(6, 0);
            lcd.Write("one  ");
            SnoozeDisplay(1200, false);
            lcd.SetCursorPosition(6, 0);
            lcd.Write("GO   ");
            SnoozeDisplay(1000, false);
        }


        private void TearOffSticker()
        {
            lcd.Visible = true;
            lcd.Backlight = true;
            LoadUpArrowSpecialChars();

            lcd.Clear();
            lcd.SetCursorPosition(4, 0);
            lcd.Write("Take badge");

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
            SnoozeDisplay(15, false);
        }


        // Sleep the thread but keep checking for any message in the queue to display
        private void SnoozeDisplay(int SnoozeTime, Boolean AllowBreak = true)
        {
            int index = SnoozeTime / 100;
            do
            {
                //Sleep the thread for 100ms as that is adequate to respond to changes
                System.Threading.Thread.Sleep(100);
                if (!isMessageQueueEmpty() & AllowBreak)
                {
                    // If we have a message to display stop snoozing
                    break;
                }
                index -= 1;
            } while (index > 0);
        }

        private void FlashDisplayBackLight(short NumberOfFlashes, int FlashOnTime, int FlashOffTime)
        {
            while (NumberOfFlashes > 0)
            {
                lcd.Backlight = false;
                SnoozeDisplay(FlashOffTime);
                lcd.Backlight = true;
                SnoozeDisplay(FlashOnTime);
                NumberOfFlashes -= 1;
                if (!isMessageQueueEmpty()) { break; }
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
                if (!isMessageQueueEmpty()) { break; }
            }
        }

        private void ScrollMessage(int Scrollby, short Speed)
        {
            int index = 0;
            while (index < Scrollby)
            {
                if (!isMessageQueueEmpty())
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


        private void FindThe(string TextArgs)
        {
            lcd.Backlight = true;
            lcd.Visible = true;
            lcd.Clear();
            lcd.SetCursorPosition(0, 0);
            lcd.Write("Catch a ...");
            lcd.SetCursorPosition(0, 1);
            lcd.Write(TextArgs);
            SnoozeDisplay(1000);
            DisplaySnoozeWaitForNextMessage();
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


        private void TestComplete(string MessageArgument)
        {
            lcd.Backlight = true;
            lcd.Visible = true;
            lcd.Clear();
            lcd.SetCursorPosition(1, 0);
            lcd.Write("You Passed!");
            lcd.SetCursorPosition(1, 1);
            lcd.Write("taking " + MessageArgument);
            FlashDisplayBackLight(2, 300, 100);
            SnoozeDisplay(3000, false);
            lcd.Clear();
            lcd.SetCursorPosition(0, 0);
            lcd.Write("You are now a");
            lcd.SetCursorPosition(0, 1);
            lcd.Write("--Maker Ranger--");
            SnoozeDisplay(3000, false);
            lcd.Clear();
            lcd.SetCursorPosition(0, 0);
            lcd.Write("Wait for other");
            lcd.SetCursorPosition(0, 1);
            lcd.Write("player to finish");
            DisplaySnoozeWaitForNextMessage();
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
            for (int o = 0; o < 2; o++)
            {
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
            }

            SnoozeDisplay(1000, false);
            lcd.Clear();
            lcd.SetCursorPosition(1, 0);
            lcd.Write("100% Healthy");
            lcd.SetCursorPosition(1, 1);
            SnoozeDisplay(800, false);
            lcd.Write("Return to wild");
            SnoozeDisplay(1500, false);
        }

        private void PressToScan(string TextArg)
        {
            lcd.Backlight = true;
            lcd.Visible = true;
            lcd.Clear();
            //lcd.SetCursorPosition(1, 0);
            //lcd.Write(TextArg);
            //lcd.SetCursorPosition(4, 1);
            //lcd.Write("Well Done!");
            //SnoozeDisplay(1500, false);


            ScanningHealth();


            lcd.Clear();
            lcd.SetCursorPosition(0, 0);
            lcd.Write("Press button");
            lcd.SetCursorPosition(0, 1);
            lcd.Write("for next animal");

            DisplaySnoozeWaitForNextMessage();
        }

        private void DisplaySnoozeWaitForNextMessage()
        {
            while (true)
            {
                if (!isMessageQueueEmpty()) { break; }
                System.Threading.Thread.Sleep(50);
            }
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
            DisplaySnoozeWaitForNextMessage();
        }


        private void WrongAnimal(string TextArgument)
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

            lcd.Write(new byte[] { 0x03 }, 0, 1);
            lcd.SetCursorPosition(2, 1);

            lcd.Write(new byte[] { 0x05 }, 0, 1);
            lcd.SetCursorPosition(7, 0);
            lcd.Write("Wrong");
            lcd.SetCursorPosition(7, 1);
            lcd.Write("animal!");

            SnoozeDisplay(1500, false);
            lcd.Clear();
            lcd.SetCursorPosition(0, 0);
            lcd.Write("That was a ...");
            lcd.SetCursorPosition(0, 1);
            lcd.Write(TextArgument);

            SnoozeDisplay(3000, false);

        }

        public void WelcomeMessages()
        {
            do
            {
                if (!isMessageQueueEmpty()) { break; }
                lcd.BlinkCursor = false;
                lcd.Backlight = true;
                lcd.Visible = true;
                lcd.Clear();
                lcd.SetCursorPosition(5, 0);
                lcd.Write("Maker");
                lcd.SetCursorPosition(5, 1);
                lcd.Write("Ranger");
                if (!isMessageQueueEmpty()) { break; }
                SnoozeDisplay(3500);
                lcd.Clear();

                lcd.Write("Newcastle 2015");

                lcd.SetCursorPosition(0, 1);
                lcd.Write("Maker Faire UK");
                if (!isMessageQueueEmpty()) { break; }
                SnoozeDisplay(1500);
                if (!isMessageQueueEmpty()) { break; }
                FlashDisplayBackLight(2, 300, 100);
                if (!isMessageQueueEmpty()) { break; }
                ScrollMessage(17, 200);
                if (!isMessageQueueEmpty()) { break; }

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

                //SnoozeDisplay(3000);
                //lcd.Clear();
                lcd.Home();
                lcd.SetCursorPosition(0, 0);
                lcd.Write("Become a ranger");
                lcd.SetCursorPosition(0, 1);
                lcd.Write("pass this test");


                SnoozeDisplay(3000);
                if (!isMessageQueueEmpty()) { break; }
                ScrollMessage(17, 50);
                if (!isMessageQueueEmpty()) { break; }

                lcd.Clear();
                lcd.Home();
                lcd.Write("follow ");
                lcd.SetCursorPosition(0, 1);
                lcd.Write("Jake the snake");
                if (!isMessageQueueEmpty()) { break; }
                SnoozeDisplay(3000);
                ScrollMessage(17, 50);
                if (!isMessageQueueEmpty()) { break; }
              
                lcd.Clear();
                lcd.SetCursorPosition(0, 0);
                SnoozeDisplay(500);
                if (!isMessageQueueEmpty()) { break; }
                for (int i = 0; i < 15; i++)
                {
                    System.Threading.Thread.Sleep(50);
                    lcd.SetCursorPosition(i, 0);
                    lcd.Write(new byte[] { 0xFF }, 0, 1);
                }
                for (int i = 0; i < 15; i++)
                {
                    // Health scan
                    System.Threading.Thread.Sleep(30);
                    lcd.SetCursorPosition(i, 0);
                    if (i == 2) { lcd.Write("H"); }
                    else if (i == 3) { lcd.Write("E"); }
                    else if (i == 4) { lcd.Write("A"); }
                    else if (i == 5) { lcd.Write("L"); }
                    else if (i == 6) { lcd.Write("T"); }
                    else if (i == 7) { lcd.Write("H"); }
                    else if (i == 9) { lcd.Write("S"); }
                    else if (i == 10) { lcd.Write("C"); }
                    else if (i == 11) { lcd.Write("A"); }
                    else if (i == 12) { lcd.Write("N"); }
                    else { lcd.Write(" "); }

                }
                lcd.SetCursorPosition(2, 1);
                lcd.Write("the animals...");
                if (!isMessageQueueEmpty()) { break; }

                SnoozeDisplay(3000);
                if (!isMessageQueueEmpty()) { break; }

                //--LoadInsertCardSpecialChars();
                lcd.Clear();
                SnoozeDisplay(500);
                if (!isMessageQueueEmpty()) { break; }

                lcd.SetCursorPosition(2, 0);
                lcd.Write("PRESS BUTTON");
                if (!isMessageQueueEmpty()) { break; }
                lcd.SetCursorPosition(4, 1);

                lcd.Write("TO START");


                if (!isMessageQueueEmpty()) { break; }
                SnoozeDisplay(1000);
                if (!isMessageQueueEmpty()) { break; }
                FlashDisplayText(10, 400, 300);
                if (!isMessageQueueEmpty()) { break; }
                lcd.Clear();
                SnoozeDisplay(1000);
                if (!isMessageQueueEmpty()) { break; }

            } while (isMessageQueueEmpty());

        }

        public void AddMessage(MakerRanger.LCD.LCDMessage Message)
        {
            // threading safety lock the resource
            lock (MessageQueue)
            {
                MessageQueue.Enqueue(Message);
            }
        }

        public void AddMessage(LCDScreen.LCDStates Message)
        {
            // threading safety lock the resource
            lock (MessageQueue)
            {
                MessageQueue.Enqueue(new LCD.LCDMessage(Message, null));
            }
        }


        public Boolean isMessageQueueEmpty()
        {
            if (MessageQueue.Count > 0)
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


        public void LoadInsertCardSpecialChars()
        {
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
