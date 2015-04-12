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
        static Boolean isMotorsConnected = false;
        //static BackStopMonitor oBackStopMonitor;
        //static PersistedStorage oPersistedStorage;
        //static MakerRanger.LEDStripLighting.LEDStrips oLEDStrips = new LEDStripLighting.LEDStrips();

        static SPI SPIInstance = new SPI(new SPI.Configuration(Cpu.Pin.GPIO_NONE, //latchPin,
                false, // active state
                0,     // setup time
                0,     // hold time 
                false, // clock idle state
                true,  /// clock edge
                10000,   // clock rate
                SPI.SPI_module.SPI1));

        static Settings.Settings oSettings;
        static LCDScreen oLCDScreenA;
        static LCDScreen oLCDScreenB;

        static Thread LCDThreadA;
        static Thread LCDThreadB;


        static LabelPrinting oLabelPrinting;

        static Person.Person PersonScanned;

        static MakerRanger.Sensors.Buttons oButtons = new MakerRanger.Sensors.Buttons();

        static MakerRanger.Game.Game GameController = new Game.Game();

        private static RfidReader rfidReadera;
        private static RfidReader rfidReaderb;



        private static RFID.RFIDIdentityDictionary oRFIDIdentityDic;

        private static Steppers.StepperController oStepperMotorController;
        private static LEDCube.LEDController oLEDController;

        public static void Main()
        {
            Debug.EnableGCMessages(true);

            oLCDScreenA = new LCDScreen(ref SPIInstance, Pins.GPIO_PIN_D7);
            oLCDScreenB = new LCDScreen(ref SPIInstance, Pins.GPIO_PIN_D6);

            //Start up the LCD thread first
            // Startup LCD A
            ThreadStart tsLCDA = new ThreadStart(oLCDScreenA.StartLCDThread);
            LCDThreadA = new Thread(tsLCDA);
            LCDThreadA.Start();
            oLCDScreenA.AddMessage(LCDScreen.LCDStates.Startup);

            // Startup LCD B
            ThreadStart tsLCDB = new ThreadStart(oLCDScreenB.StartLCDThread);
            LCDThreadB = new Thread(tsLCDB);
            LCDThreadB.Start();
            oLCDScreenB.AddMessage(LCDScreen.LCDStates.Startup);

            //Set up the I2C for controlling the stepper motors
            oStepperMotorController = new Steppers.StepperController(0x04);

            //Set up the I2C for controlling the cube lighting
            //oLEDController = new LEDCube.LEDController(0x05);

            //Dictionary holds all the RFID tags IDs and the human readable names from SD card
            oRFIDIdentityDic = new RFID.RFIDIdentityDictionary();
            oRFIDIdentityDic.LoadFromFile();

            //Button event handlers
            oButtons.OnButtonADown += ButtonADown;
            oButtons.OnButtonBDown += ButtonBDown;

            //Create label printing object
            oLabelPrinting = new LabelPrinting();
            //Clear the printer
            oLabelPrinting.ResetPrinter();
            //2 Seconds after reset before printer available

            //Main Game controller events
            GameController.OnPlayerReady += PlayerisReady;
            GameController.OnPlayersAreReady += PlayersAreReady;
            GameController.OnNextAnimalPush += NextAnimalPushed;
            GameController.OnNextAnimal += NextAnimal;
            GameController.OnEndOfRound += EndOfRound;
            GameController.OnEndOfGame += EndOfGame;
            GameController.Enabled = true;


            // Add event handler for sticker take message
            oLabelPrinting.StickersPrinted += new NativeEventHandler(StickersPrintedEvent);

            rfidReadera = new RfidReader(oRFIDIdentityDic, Pins.GPIO_PIN_D9, SPIInstance, Pins.GPIO_PIN_A0);
            rfidReaderb = new RfidReader(oRFIDIdentityDic, Pins.GPIO_PIN_D8, SPIInstance);
            rfidReadera.ResetReaders();

            Thread.Sleep(300);
            rfidReadera.InitReader();
            rfidReaderb.InitReader();


            rfidReadera.TagChangeDetected += new RfidReader.RFIDEventHandler(TagChangeDetecteda);
            rfidReaderb.TagChangeDetected += new RfidReader.RFIDEventHandler(TagChangeDetectedb);


            rfidReadera.enabled = true;
            rfidReaderb.enabled = true;

            //On startup move the pointer to neutral position
            if (isMotorsConnected)
            {
                oStepperMotorController.MoveToPosition(Steppers.StepperController.PlayerType.PlayerA, 19);
                oStepperMotorController.MoveToPosition(Steppers.StepperController.PlayerType.PlayerB, 19);



            }

            //Set webserver up listening
            //Server oWebServer = new Server(80, false, "192.168.1.118", "255.255.255.0", "192.168.1.254", "NETDUINOPLUS");
            //oWebServer.AfterFileRecieved += new FileProcessed(FileToProcess);
            //oWebServer.AfterSettingsValueSet += new SetSettingsValue(SettingsSetFromEthernet);

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

            //Sleep forever to keep static objects alive forever
            Thread.Sleep(Timeout.Infinite);
        }



        private static void NextAnimal(uint data1, uint data2, DateTime time)
        {
            //By convention 0=A 1=B
            if (data1 == 0)
            {
                DisplaySeekingA();
            }
            else
            {
                DisplaySeekingB();
            }
        }

        private static void NextAnimalPushed(uint data1, uint data2, DateTime time)
        {
            //by convention 0=A and 1=B
            if (data1 == 0)
            {
                GameController.NextInRound(Game.Game.PlayerType.PlayerA);

            }
            else
            {
                GameController.NextInRound(Game.Game.PlayerType.PlayerB);
            }
        }


        private static void PlayerisReady(uint uint1, uint unit2, DateTime date)
        {
            if (uint1 == 0)
            {
                oLCDScreenA.AddMessage(LCDScreen.LCDStates.GetReady);
            }
            else
            {
                oLCDScreenB.AddMessage(LCDScreen.LCDStates.GetReady);
            }
        }

        //Both players ready, start the game
        private static void PlayersAreReady(uint uint1, uint unit2, DateTime date)
        {
            oLCDScreenA.AddMessage(LCDScreen.LCDStates.StartingGame);

            if (!(GameController.IsSinglePlayermode))
            {
                oLCDScreenB.AddMessage(LCDScreen.LCDStates.StartingGame);
            }
            else
            {
                oLCDScreenB.AddMessage(LCDScreen.LCDStates.SinglePlayerModeReminder);
            }
            DisplaySeekingA();
            DisplaySeekingB();

        }



        private static void DisplaySeekingA()
        {
            string Description = oRFIDIdentityDic.GetName(GameController.RoundListA.CurentItemID());
            if (!(GameController.IsShowingAnimalsOnDisplay))
            {
                Description = string.Empty;
            }
            oLCDScreenA.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.FindThe, Description));
            if (isMotorsConnected)
            {
                oStepperMotorController.MoveToPosition(Steppers.StepperController.PlayerType.PlayerA, (short)GameController.RoundListA.CurentItemID());

            }

        }

        private static void DisplaySeekingB()
        {
            if (!(GameController.IsSinglePlayermode))
            {
                string Description = oRFIDIdentityDic.GetName(GameController.RoundListB.CurentItemID());
                if (!(GameController.IsShowingAnimalsOnDisplay))
                {
                    Description = string.Empty;
                }
                oLCDScreenB.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.FindThe, Description));
                if (isMotorsConnected)
                {
                    oStepperMotorController.MoveToPosition(Steppers.StepperController.PlayerType.PlayerB, (short)GameController.RoundListB.CurentItemID());
                }
            }
        }


        private static string PadLeft(string input, int length, string PadChar)
        {
            while (input.Length < length)
            {
                input = string.Concat(PadChar, input);

            }
            return input;

        }

        private static void EndOfGame(uint data1, uint data2, DateTime time)
        {



            //End of game by nature should show on both screens
            if (data1 == 0)
            {
                TimeSpan ElapsedTime = (GameController.GameFinishedTimeA - GameController.GameStartedTime);
                oLCDScreenA.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.TestComplete, (ElapsedTime.Minutes.ToString() + ":" + PadLeft(ElapsedTime.Seconds.ToString(), 2, "0"))));
                //print sticker for player A
                oLabelPrinting.PrintStoredForm(0, "MRanger", "A-Time " + (ElapsedTime.Minutes.ToString() + ":" + PadLeft(ElapsedTime.Seconds.ToString(), 2, "0")));
            }
            else
            {
                TimeSpan ElapsedTime = (GameController.GameFinishedTimeB - GameController.GameStartedTime);
                oLCDScreenB.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.TestComplete, (ElapsedTime.Minutes.ToString() + ":" + PadLeft(ElapsedTime.Seconds.ToString(), 2, "0"))));
                //print sticker for player B
                oLabelPrinting.PrintStoredForm(0, "MRanger", "B-Time " + (ElapsedTime.Minutes.ToString() + ":" + PadLeft(ElapsedTime.Seconds.ToString(), 2, "0")));
            }



            //move back to rest postion
            if (isMotorsConnected)
            {
                oStepperMotorController.MoveToPosition(Steppers.StepperController.PlayerType.PlayerA, (short)20);
                oStepperMotorController.MoveToPosition(Steppers.StepperController.PlayerType.PlayerB, (short)20);
            }

            // Tell them to take stickers
            //oLabelPrinting.RecallFormAndPrint(0, "makerranger", (short)2);


            //oLCDScreenA.AddMessage(LCDScreen.LCDStates.TakeSticker);
            //oLCDScreenB.AddMessage(LCDScreen.LCDStates.TakeSticker);

            //Reset game, save game data
            GameController.SaveRoundsToFile();



            GameController.InProgressA = false;
            GameController.InProgressB = false;
            //oLCDScreenA.AddMessage(LCDScreen.LCDStates.WelcomeMessages);
            //oLCDScreenB.AddMessage(LCDScreen.LCDStates.WelcomeMessages);
            //leave data in case we want a sticker reprint
        }

        private static void EndOfRound(uint data1, uint data2, DateTime time)
        {
            if (data1 == 0)
            {
                TimeSpan ElapsedTime = (GameController.GameFinishedTimeA - GameController.GameStartedTime);
                oLCDScreenA.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.TestComplete, (ElapsedTime.Minutes.ToString() + ":" + PadLeft(ElapsedTime.Seconds.ToString(), 2, "0"))));


            }
            else
            {
                TimeSpan ElapsedTime = (GameController.GameFinishedTimeB - GameController.GameStartedTime);
                oLCDScreenB.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.TestComplete, (ElapsedTime.Minutes.ToString() + ":" + PadLeft(ElapsedTime.Seconds.ToString(), 2, "0"))));

            }
        }


        private static void FileToProcess(string filename, string username, string screenname, Int64 userid, short Guess)
        {
            //oLEDStrips.CaseLights(true);
            PersonScanned = new Person.Person() { UserName = username, UserID = userid, ScreenName = screenname, Guess = Guess };
            //oLCDScreenA.AddMessage(LCDScreen.LCDStates.PersonDetected, PersonScanned);
        }


        private static void ButtonADown(uint uint1, uint uint2, DateTime date)
        {
            if (GameController.Enabled)
            {
                GameController.ButtonAPressed();
            }

        }

        private static void ButtonBDown(uint uint1, uint uint2, DateTime date)
        {
            if (GameController.Enabled)
            {
                GameController.ButtonBPressed();
            }

        }

        private static void SettingsSetFromEthernet(string MaxGuessValue, string MaxQtyValue)
        {
            //Reset the settings from the webserver request
            oSettings.SetValue("standard", @"maxGuessValue", MaxGuessValue);
            oSettings.SetValue("standard", @"qtyMaxNumber", MaxQtyValue);
            //oLCDScreenA.MaxChooseValue = oSettings.GetValue("standard", @"maxGuessValue");
            //oPersistedStorage.MaxGuessValue = int.Parse(MaxGuessValue);
            //oPersistedStorage.qtyMaxNumber = byte.Parse(MaxQtyValue);
            oSettings.Save("iniSettings");


        }

        private static void StickersPrintedEvent(uint data1, uint data2, DateTime time)
        {
            //Tell user to take sticker
            oLCDScreenA.AddMessage(LCDScreen.LCDStates.TakeSticker);
            if (!(GameController.IsSinglePlayermode))
            {
                oLCDScreenB.AddMessage(LCDScreen.LCDStates.TakeSticker);
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

        private static void TagChangeDetecteda(object sender, RFID.RFIDEventArgs e)
        {
            Debug.Print("Tag Detected a " + System.DateTime.Now.ToString());
            if (GameController.InProgressA)
            {
                if (GameController.RoundListA.isCurrentItem((int)e.TagIndex))
                {
                    if (!(GameController.AwaitingNextPushA))
                    {
                        GameController.AwaitingNextPushA = true;
                        //Correct tag in place
                        //Log time and ask to scan
                        oLCDScreenA.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.PressToNextAnimal, e.TagIdentityText));
                        GameController.RoundTimesA.Enqueue(e.TagReadTime);

                    }

                }
                else
                {
                    //Wrong tag in place
                    oLCDScreenA.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.WrongAnimalTryAgain, e.TagIdentityText));
                    DisplaySeekingA();
                }
            }
            //Feed to the admin processor
            if (e.TagIndex > (short)99) { ExecuteAdminCommand(e.TagIndex, (short)0); }

            // if oRoundListA.isCurrentItem(e.)
        }

        private static void TagChangeDetectedb(object sender, RFID.RFIDEventArgs e)
        {
            Debug.Print("Tag Detected b " + System.DateTime.Now.ToString());
            if ((GameController.InProgressB) & !(GameController.IsSinglePlayermode))
            {
                if (GameController.RoundListB.isCurrentItem((int)e.TagIndex))
                {
                    if (!(GameController.AwaitingNextPushB))
                    {
                        GameController.AwaitingNextPushB = true;
                        //Correct tag in place
                        //Log time and ask to scan

                        oLCDScreenB.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.PressToNextAnimal, e.TagIdentityText));
                        GameController.RoundTimesB.Enqueue(e.TagReadTime);
                    }
                }
                else
                {
                    //Wrong tag in place
                    oLCDScreenB.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.WrongAnimalTryAgain, e.TagIdentityText));
                    DisplaySeekingB();
                }

            }
            //Feed to the admin processor
            if (e.TagIndex > (short)99) { ExecuteAdminCommand(e.TagIndex, (short)1); }
        }


        //If RFID with a definition above 100 found, then it is treated as an admin command
        //Do whatever that Id defines
        private static void ExecuteAdminCommand(short CommandID, short reader)
        {
            switch (CommandID)
            {
                case (short)100:
                    // Reprint Labels
                    Debug.Print("Admin command " + CommandID.ToString());
                    //reprint the stickers and show display again
                    if (!(GameController.InProgressA) && !(GameController.InProgressB))
                    {
                        if (reader == (short)0)
                        {
                            EndOfGame((uint)0, (uint)0, DateTime.Now);
                        }
                        else
                        {
                            EndOfGame((uint)1, (uint)0, DateTime.Now);
                        }
                    }
                    break;
                case (short)101:
                    // Abort game
                    Debug.Print("Admin command " + CommandID.ToString());
                    GameController.InProgressA = false;
                    GameController.InProgressB = false;
                    oLCDScreenA.AddMessage(LCDScreen.LCDStates.WelcomeMessages);
                    oLCDScreenB.AddMessage(LCDScreen.LCDStates.WelcomeMessages);
                    break;
                // Single player toggle 
                case (short)102:
                    Debug.Print("Admin command " + CommandID.ToString());
                    Debug.Print("Single Player Toggle");
                    //If the game is not yet in progress allow switch player modes                    
                    if (!(GameController.InProgressA) && !(GameController.InProgressB))
                    {
                        GameController.IsSinglePlayermode = !GameController.IsSinglePlayermode;
                        if (GameController.IsSinglePlayermode)
                        {
                            oLCDScreenA.AddMessage(LCDScreen.LCDStates.SinglePlayerMode);
                            oLCDScreenB.AddMessage(LCDScreen.LCDStates.SinglePlayerMode);
                        }
                        else
                        {
                            oLCDScreenA.AddMessage(LCDScreen.LCDStates.TwoPlayerMode);
                            oLCDScreenB.AddMessage(LCDScreen.LCDStates.TwoPlayerMode);


                        }
                    }

                    break;
                case (short)103:
                    // Set Number of Rounds
                    // Rotate around a set number of rounds
                    if (!(GameController.InProgressA) && !(GameController.InProgressB))
                    {
                        switch (GameController.NumberOfRounds)
                        {
                            case 2:
                                GameController.NumberOfRounds = 5;
                                break;
                            case 5:
                                GameController.NumberOfRounds = 8;
                                break;
                            case 8:
                                GameController.NumberOfRounds = 10;
                                break;
                            case 10:
                                GameController.NumberOfRounds = 15;
                                break;
                            case 15:
                                GameController.NumberOfRounds = 2;
                                break;
                        }
                        //GameController.NumberOfRounds
                        oLCDScreenA.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.NumberOfRounds, GameController.NumberOfRounds.ToString()));
                        oLCDScreenB.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.NumberOfRounds, GameController.NumberOfRounds.ToString()));
                    }
                    break;
                // Show Animals toggle 
                case (short)104:
                    Debug.Print("Admin command " + CommandID.ToString());
                    Debug.Print("Show Animal Toggle");
                    //If the game is not yet in progress allow switch player modes                    
                    if (!(GameController.InProgressA) && !(GameController.InProgressB))
                    {
                        GameController.IsShowingAnimalsOnDisplay = !GameController.IsShowingAnimalsOnDisplay;
                        if (GameController.IsShowingAnimalsOnDisplay)
                        {
                            oLCDScreenA.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.ShowingAnimalsOnDisplay, "true"));
                            oLCDScreenB.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.ShowingAnimalsOnDisplay, "true"));

                        }
                        else
                        {
                            oLCDScreenA.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.ShowingAnimalsOnDisplay, "false"));
                            oLCDScreenB.AddMessage(new LCD.LCDMessage(LCDScreen.LCDStates.ShowingAnimalsOnDisplay, "false"));


                        }
                    }

                    break;
                default:
                    break;
            }

        }
    }
}
