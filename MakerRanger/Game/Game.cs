using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Collections;

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
        public event NativeEventHandler PlayerAisReady;
        public event NativeEventHandler PlayerBisReady;
        public event NativeEventHandler OnScanAnimal;

        //Overall game is in progress - no config allowed
        public bool InProgress { get; set; }
        public bool IsSinglePlayermode { get; set; }
        
        //Awaiting push button to scan animal
        public bool AwaitingScanA { get; set; }
        public bool AwaitingScanB { get; set; }


        private bool _PlayerAReady;
        public bool PlayerAReady
        {
            get { return _PlayerAReady; }
            set
            {
                if (this.Enabled)
                {
                    _PlayerAReady = value;
                    if ((value & PlayerBReady)|(this.IsSinglePlayermode)){ PlayersAreReady(); }
                    else
                    {
                        NativeEventHandler OnPlayersAisReady = PlayerAisReady;
                        if (OnPlayersAisReady != null)
                        {
                            OnPlayersAisReady((uint)0, (uint)0, DateTime.Now);
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
                if ((this.Enabled)& !(this.IsSinglePlayermode))
                {
                    _PlayerBReady = value;
                    if (value & PlayerAReady) { PlayersAreReady(); }
                    else
                    {
                        NativeEventHandler OnPlayersBisReady = PlayerBisReady;
                        if (OnPlayersBisReady != null)
                        {
                            OnPlayersBisReady((uint)0, (uint)0, DateTime.Now);
                        }
                    }
                }

            }
        }

        public bool Enabled { get; set; }

        public Game()
        {
            // class initializer

        }

        public void ButtonAPressed()
        {
            if (this.Enabled)
            {
                if (this.InProgress)
                {
                    if (this.AwaitingScanA)
                    {
                        //raise event to say do scanning sequence for A
                        this.AwaitingScanA = false;
                    }
                }
                else { this.PlayerAReady = true; }  // Before game
            }

        }

        public void ButtonBPressed()
        {
            if (this.Enabled)
            {
                if (this.InProgress)
                {
                    if (this.AwaitingScanB)
                    {
                        //raise event to say do scanning sequence for B
                        this.AwaitingScanB = false;
                    }
                }
                else {this.PlayerBReady = true; }  // Before game
            }

        }

        public void DetectedAEmpty(){
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
            NativeEventHandler PlayersReady = OnPlayersAreReady;
            InitGame();
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
            RoundListA.NewList(8,15);
            if (!(this.IsSinglePlayermode))
            {
                RoundListB.NewList(8,15);
            }
            if (!(InProgress)) { this.InProgress = true; }
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
