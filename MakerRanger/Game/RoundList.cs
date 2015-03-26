using System;
using Microsoft.SPOT;
using System.Collections;

namespace MakerRanger.Game
{
    class RoundList:ArrayList
    {
        public short Position { get; set; }

        public void NewList(int NumberOfItems, int TotalNumberOfItems){
            this.Clear();
            this.Position = 0;

            //load the array with 0..n
            for (int i = 0; i < TotalNumberOfItems; i++)
            {
                this.Add(i);
            }

            Random rand = new Random();
            //Random Shuffle the array
            for (int i = this.Count - 1; i > 0; i--)
            {
                int n = rand.Next(i + 1);
                object tmp = this[i];
                this[i] = this[n];
                this[n] = tmp;
            }
            
            // Trim down to desired size from the full array
            for (int i = this.Count - 1;  i > NumberOfItems; i--)
            {
                Debug.Print("Remove " + i.ToString());
                this.RemoveAt(i);
                Debug.Print(this.Count.ToString());
            }

            //Let us see what we have generated
            for (int i = 0; i < this.Count-1; i++)
            {
                Debug.Print("Round: " + this[i]);
            }
        }

        public bool isCurrentItem(int index)
        {
            if ((int)(this[this.Position])== index) {
                return true;
            }
            else { return false; }
        }

        public void Next()
        {
            Position += 1;
        }


    }
}
