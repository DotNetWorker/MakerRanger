using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Collections;
using System.IO;

namespace MakerRanger.Game
{
    class Game
    {

        //Round list keeps the list of items and the order we are seeking them
        public RoundList RoundListA = new RoundList();
        public RoundList RoundListB = new RoundList();
        // Round Times holds the times to find each animal for stats print out later
        public Queue RoundTimesA = new Queue();
        public Queue RoundTimesB = new Queue();


        //Events
        public event NativeEventHandler OnPlayersAreReady;
        public event NativeEventHandler OnPlayerReady;

        public event NativeEventHandler OnScanAnimal;

        public event NativeEventHandler OnEndOfRound;
        public event NativeEventHandler OnEndOfGame;

        public event NativeEventHandler OnNextAnimal;



        //Overall game is in progress - no config allowed
        public bool InProgressA { get; set; }
        public bool InProgressB { get; set; }
        public bool IsSinglePlayermode { get; set; }

        //Awaiting push button to scan animal
        public bool AwaitingScanA { get; set; }
        public bool AwaitingScanB { get; set; }


        public enum PlayerType : byte
        {
            PlayerA,
            PlayerB
        }

        private const string GameLogSDFolder = @"SD\Games\";

        private bool _PlayerAReady;
        public bool PlayerAReady
        {
            get { return _PlayerAReady; }
            set
            {
                if (this.Enabled)
                {
                    _PlayerAReady = value;
                    if ((value & PlayerBReady) | (this.IsSinglePlayermode)) 
                    {
                        PlayersAreReady(); 
                    }
                    else
                    {
                        NativeEventHandler PlayerReady = OnPlayerReady;
                        if (PlayerReady != null)
                        {
                            PlayerReady((uint)1, (uint)0, DateTime.Now);
                        }
                    }
                }

            }
        }

        private bool _PlayerBReady;
        public bool PlayerBReady
        {
            get { return _PlayerBReady; }
            set
            {
                if ((this.Enabled) & !(this.IsSinglePlayermode))
                {
                    _PlayerBReady = value;
                    if (value & PlayerAReady) 
                    { 
                        PlayersAreReady(); 
                    }
                    else
                    {
                        NativeEventHandler PlayerisReady = OnPlayerReady;
                        if (PlayerisReady != null)
                        {
                            PlayerisReady((uint)1, (uint)0, DateTime.Now);
                        }
                    }
                }

            }
        }

        public bool Enabled { get; set; }

        public Game()
        {
            this.IsSinglePlayermode = false;
            // class initializer
        }

        public void ButtonAPressed()
        {
            if (this.Enabled)
            {
                if (this.InProgressA)
                {
                    if (this.AwaitingScanA)
                    {
                        //raise event to say do scanning sequence for A
                        this.AwaitingScanA = false;
                        NativeEventHandler ScanAnimal = OnScanAnimal;
                        if (ScanAnimal != null)
                        {
                            ScanAnimal((uint)0, (uint)1, DateTime.Now);
                        }
                    }
                }
                else if (!(this.InProgressA) && !(this.InProgressB))
                {
                    if (!(this.PlayerAReady)) { this.PlayerAReady = true; } 
                }  // Before game
            }
        }

        public void ButtonBPressed()
        {
            if (this.Enabled)
            {
                if (this.InProgressB)
                {
                    if (this.AwaitingScanB)
                    {
                        //raise event to say do scanning sequence for B
                        this.AwaitingScanB = false;
                        NativeEventHandler ScanAnimal = OnScanAnimal;
                        if (ScanAnimal != null)
                        {
                            ScanAnimal((uint)1, (uint)1, DateTime.Now);
                        }
                    }
                }
                else if (!(this.InProgressA) && !(this.InProgressB))
                {
                    if (!(this.PlayerBReady)) { this.PlayerBReady = true; }
                }  // Before game
            }
        }

        public void DetectedAEmpty()
        {
            // if cage goes empty while wating for scan
            // set waiting for scan to false and let them go back through the detection, need to think about removing the prev time?- or make
            // scan be dated action
        }

        public void DetectedBEmpty()
        {
            // if cage goes empty while wating for scan
            // set waiting for scan to false and let them go back through the detection, need to think about removing the prev time?- or make
            // scan be dated action
        }

        public void ResetPlayersReady()
        {
            this.PlayerAReady = false;
            this.PlayerBReady = false;
        }

        private void PlayersAreReady()
        {
            InitGame();
            NativeEventHandler PlayersReady = OnPlayersAreReady;
            if (PlayersReady != null)
            {
                PlayersReady((uint)0, (uint)0, DateTime.Now);
            }
        }

        private void InitGame()
        {
            //Clear previous rounds and create new lists to catch
            RoundTimesA.Clear();
            RoundTimesB.Clear();
            RoundListA.NewList(2, 15);
            if (!(this.IsSinglePlayermode))
            {
                RoundListB.NewList(2, 15);
            }
            if (!(InProgressA)) { this.InProgressA = true; }
            if (!(InProgressB)) { this.InProgressB = true; }
            ResetPlayersReady();
        }

        public void NextInRound(PlayerType Player)
        {
            if (Player == PlayerType.PlayerA)
            {
                //Last Position already
                if (RoundListA.Count - 1 == RoundListA.Position)
                {
                    if (!(InProgressB) | IsSinglePlayermode)
                    {
                        //end of game
                        //end if the list so end of this player's game, raise event to say so
                        NativeEventHandler PlayerEndOfGame = OnEndOfGame;
                        if (PlayerEndOfGame != null)
                        {
                            //0 by convention means player B
                            PlayerEndOfGame((uint)0, (uint)0, DateTime.Now);
                        }
                        this.InProgressA = false;
                       
                    }
                    else
                    {
                        //end if the list so end of this player's game, raise event to say so
                        NativeEventHandler PlayerEndOfRound = OnEndOfRound;
                        if (PlayerEndOfRound != null)
                        {
                            //0 by convention means player A
                            PlayerEndOfRound((uint)0, (uint)0, DateTime.Now);
                        }
                        this.InProgressA = false;
                    }
                }
                else
                {
                    RoundListA.Next();
                    NativeEventHandler NextAnimal = OnNextAnimal;
                    if (NextAnimal != null)
                    {
                        NextAnimal((uint)0, (uint)RoundListA.CurentItemID(), DateTime.Now);
                    }
                }
            }
            else
            {
                if (RoundListB.Count - 1 == RoundListB.Position)
                {
                    if (!(InProgressA))
                    {
                        //end of game
                        //end if the list so end of this player's game, raise event to say so
                        NativeEventHandler PlayerEndOfGame = OnEndOfGame;
                        if (PlayerEndOfGame != null)
                        {
                            //1 by convention means player B
                            PlayerEndOfGame((uint)1, (uint)0, DateTime.Now);
                        }
                        this.InProgressB = false;
                    }
                    else
                    {
                        //end if the list so end of this player's game, raise event to say so
                        NativeEventHandler PlayerEndOfRound = OnEndOfRound;
                        if (PlayerEndOfRound != null)
                        {
                            // 1 by convention means player B
                            PlayerEndOfRound((uint)1, (uint)0, DateTime.Now);
                        }
                        this.InProgressB = false;
                    }
                }
                else
                {
                    RoundListB.Next();
                    NativeEventHandler NextAnimal = OnNextAnimal;
                    if (NextAnimal != null)
                    {

                        NextAnimal((uint)1, (uint)RoundListB.CurentItemID(), DateTime.Now);
                    }
                }
            }

        }

        public void SaveRoundsToFile()
        {
            //Use the this.ID as filename
            SaveRoundToFile(this.RoundListA);
            if (!(this.IsSinglePlayermode))
            {
              SaveRoundToFile(this.RoundListB);
            }
        }

        private void SaveRoundToFile(RoundList ThisRoundList)
        {
            if (!(Directory.Exists(GameLogSDFolder)))
            {
                Directory.CreateDirectory(GameLogSDFolder);
            }
            //Write the log of each player round to file
            using (var filestream = new FileStream(GameLogSDFolder + ThisRoundList.ID.ToString(), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 8))
			{
				using (var streamWriter = new StreamWriter(filestream))
				{
                    for (int i = 0; i < ThisRoundList.Count-1; i++)
                    {
                        streamWriter.WriteLine(ThisRoundList[i].ToString());
                    }
					streamWriter.Close();
				}
			}

        }


        //Raise event to start scanning sequence
        private void OnScanAnimalA()
        {
            NativeEventHandler ScanAnimal = OnScanAnimal;
            if (ScanAnimal != null)
            {
                //By convention well pass 0 for A and 1 for B
                ScanAnimal(0, 0, DateTime.Now);
            }

        }

        //Raise event to start scanning sequence
        private void OnScanAnimalB()
        {
            NativeEventHandler ScanAnimal = OnScanAnimal;
            if (ScanAnimal != null)
            {
                //By convention well pass 0 for A and 1 for B
                ScanAnimal(1, 0, DateTime.Now);
            }
        }
    }
}
