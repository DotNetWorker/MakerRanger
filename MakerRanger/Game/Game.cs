using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Collections;
using System.IO;

namespace MakerRanger.Game
{
    class Game
    {
        // if playing with users from social media we get the user info from here
        public Person.Person PersonA;
        public Person.Person PersonB;

        //Round list keeps the list of items and the order we are seeking them
        public RoundList RoundListA = new RoundList();
        public RoundList RoundListB = new RoundList();
        // Round Times holds the times to find each animal for stats print out later
        public Queue RoundTimesA = new Queue();
        public Queue RoundTimesB = new Queue();


        //Events
        public event NativeEventHandler OnPlayersAreReady;
        public event NativeEventHandler OnPlayerReady;
        public event NativeEventHandler OnPlayerAdded;

        public event NativeEventHandler OnNextAnimalPush;

        public event NativeEventHandler OnEndOfRound;
        public event NativeEventHandler OnEndOfGame;

        public event NativeEventHandler OnNextAnimal;



        //Overall game is in progress - no config allowed
        public bool InProgressA { get; set; }
        public bool InProgressB { get; set; }
        public bool IsSinglePlayermode { get; set; }
        public bool IsShowingAnimalsOnDisplay { get; set; }

        //Awaiting push button to scan animal
        public bool AwaitingNextPushA { get; set; }
        public bool AwaitingNextPushB { get; set; }

        public DateTime GameStartedTime { get; set; }
        public DateTime GameFinishedTimeA { get; set; }
        public DateTime GameFinishedTimeB { get; set; }

        public int NumberOfRounds { get; set; }

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
                    if ((value & PlayerBReady) | (value & this.IsSinglePlayermode))
                    {
                        PlayersAreReady();
                    }
                    else
                    {
                        NativeEventHandler PlayerReady = OnPlayerReady;
                        if (PlayerReady != null)
                        {
                            PlayerReady((uint)0, (uint)0, DateTime.Now);
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
            this.NumberOfRounds = 2;
            this.IsShowingAnimalsOnDisplay = true;
            // class initializer
        }

        public void ButtonAPressed()
        {
            if (this.Enabled)
            {
                if (this.InProgressA)
                {
                    if (this.AwaitingNextPushA)
                    {
                        //raise event to say do scanning sequence for A
                        this.AwaitingNextPushA = false;
                        NativeEventHandler NextAnimalPush = OnNextAnimalPush;
                        if (NextAnimalPush != null)
                        {
                            NextAnimalPush((uint)0, (uint)1, DateTime.Now);
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
                    if (this.AwaitingNextPushB)
                    {
                        //raise event to say do scanning sequence for B
                        this.AwaitingNextPushB = false;
                        NativeEventHandler ScanAnimal = OnNextAnimalPush;
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

            // if players loaded and played, remove them else set the current players 
            // to have played
            if (!(this.PersonA==null)&&(this.PersonA.HasPlayed))
            {
                this.PersonA = null;
            }
            else
            {
                if (!(this.PersonA == null))
                {
this.PersonA.HasPlayed = true;
                }
                
            }

            if (!(this.PersonB==null)&&(this.PersonB.HasPlayed))
            {
                this.PersonB = null;
            }
            else
            {
                if (!(this.PersonB==null))
                {
                this.PersonB.HasPlayed = true;
                }
            }

            GameStartedTime = DateTime.Now;
            RoundListA.NewList(this.NumberOfRounds, 15);
            if (!(this.IsSinglePlayermode))
            {
                RoundListB.NewList(this.NumberOfRounds, 15);
                if (!(InProgressB)) { this.InProgressB = true; }

            }
            if (!(InProgressA)) { this.InProgressA = true; }
            //Start Time
            RoundTimesA.Enqueue(DateTime.Now);
            if (!(this.IsSinglePlayermode))
            {
                //Start Time
                RoundTimesB.Enqueue(DateTime.Now);
            }

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
                            GameFinishedTimeA = DateTime.Now;
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
                            GameFinishedTimeA = DateTime.Now;
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
                            GameFinishedTimeB = DateTime.Now;
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
                            GameFinishedTimeB = DateTime.Now;
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
            SaveRoundToFile(this.RoundListA, this.RoundTimesA);
            if (!(this.IsSinglePlayermode))
            {
                SaveRoundToFile(this.RoundListB, this.RoundTimesB);
            }
        }

        private void SaveRoundToFile(RoundList ThisRoundList, Queue RoundTimes)
        {
            if (!(Directory.Exists(GameLogSDFolder)))
            {
                Directory.CreateDirectory(GameLogSDFolder);
            }

            DateTime StartTime = (DateTime)RoundTimes.Dequeue();
            //Write the log of each player round to file
            using (var filestream = new FileStream(GameLogSDFolder + ThisRoundList.ID.ToString(), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 8))
            {
                using (var streamWriter = new StreamWriter(filestream))
                {
                    for (int i = 0; i < ThisRoundList.Count - 1; i++)
                    {
                        //Get time since started as seconds
                        TimeSpan ElapsedTime = (StartTime - (DateTime)RoundTimes.Dequeue());
                        streamWriter.WriteLine(ThisRoundList[i].ToString() + "\t" + ((ElapsedTime.Minutes * 60) + ElapsedTime.Seconds).ToString());
                    }
                    streamWriter.Close();
                }
            }

        }


        public void AddPlayer(Person.Person PlayerToAdd)
        {
            if (this.Enabled)
            {
                if (!(this.InProgressB) & !(this.InProgressA))
                {
                    if ((this.PersonA == null) || (this.PersonA.HasPlayed))
                    {
                        this.PersonA = PlayerToAdd;
                        NativeEventHandler PlayerAdded = OnPlayerAdded;
                        if (PlayerAdded != null)
                        {
                            PlayerAdded((uint)0, (uint)0, DateTime.Now);
                        }

                    }
                    else if (((this.PersonB == null) || (this.PersonB.HasPlayed)) & (!(this.PersonA == null) && this.PersonA.UserID != PlayerToAdd.UserID))
                    {
                        if (!(this.IsSinglePlayermode))
                        {
                            this.PersonB = PlayerToAdd;
                            NativeEventHandler PlayerAdded = OnPlayerAdded;
                            if (PlayerAdded != null)
                            {
                                PlayerAdded((uint)1, (uint)0, DateTime.Now);
                            }
                        }

                    }
                }
            }
        }

        //Raise event to start scanning sequence
        private void OnScanAnimalA()
        {
            NativeEventHandler ScanAnimal = OnNextAnimalPush;
            if (ScanAnimal != null)
            {
                //By convention well pass 0 for A and 1 for B
                ScanAnimal(0, 0, DateTime.Now);
            }

        }

        //Raise event to start scanning sequence
        private void OnScanAnimalB()
        {
            NativeEventHandler ScanAnimal = OnNextAnimalPush;
            if (ScanAnimal != null)
            {
                //By convention well pass 0 for A and 1 for B
                ScanAnimal(1, 0, DateTime.Now);
            }
        }
    }
}
