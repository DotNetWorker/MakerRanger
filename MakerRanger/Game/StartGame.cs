using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace MakerRanger.Game
{
    class StartGame
    {

        public event NativeEventHandler OnPlayersAreReady;
        public event NativeEventHandler PlayerAisReady;
        public event NativeEventHandler PlayerBisReady;


        private bool _PlayerAReady;

        public bool PlayerAReady
        {
            get { return _PlayerAReady; }
            set
            {
                if (this.Enabled)
                {
                    _PlayerAReady = value;
                    if (value & PlayerBReady) { PlayersAreReady(); }
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
                if (this.Enabled)
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

        public StartGame()
        {
            // class initaliser

        }

        public void Reset()
        {
            this.PlayerAReady = false;
            this.PlayerBReady = false;
        }
        private void PlayersAreReady()
        {
            NativeEventHandler PlayersReady= OnPlayersAreReady;
            if (PlayersReady != null)
            {
                PlayersReady((uint)0, (uint)0, DateTime.Now);
            }
        }

    }
}
