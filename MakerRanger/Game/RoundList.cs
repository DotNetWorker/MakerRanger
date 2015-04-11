using System;
using Microsoft.SPOT;
using System.Collections;

namespace MakerRanger.Game
{
    class RoundList:ArrayList
    {
        public short Position { get; set; }
        //ID guid holds unique reference for this round - used to save to file later
        public Guid ID { get; private set; }
        
        public void NewList(int NumberOfItems, int TotalNumberOfItems){
            this.Clear();
            this.Position = 0;
            this.ID = Guid.NewGuid();

            //load the array with 0..n
            for (byte i = 0; i < TotalNumberOfItems; i++)
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
            for (int i = this.Count - 1;  i > NumberOfItems-1; i--)
            {
                this.RemoveAt(i);
            }

            Debug.Print("-- Items to find list --");
            //Let us see what we have generated
            for (int i = 0; i <= this.Count-1; i++)
            {
                Debug.Print("Round: " + this[i].ToString());
            }
        }

        public bool isCurrentItem(int index)
        {
            if ((byte)(this[this.Position])== index) 
            {
                Debug.Print("Matches next in list");
                return true;
            }
            else 
            {
                Debug.Print("Does NOT match next in list");
                return false; 
            }
        }

        public void Next()
        {
           if (this.Position< this.Count-1)
           {
               this.Position += 1; 
           }
        }

        public byte CurentItemID()
        {
            return (byte)this[this.Position];
        }

       
    }
}
