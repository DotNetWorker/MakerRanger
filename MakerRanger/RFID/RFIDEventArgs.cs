using System;
using Microsoft.SPOT;

namespace MakerRanger.RFID
{
    public class RFIDEventArgs:EventArgs
    {
        public String TagIdentityText { get; set; }
        public DateTime TagReadTime{ get; set; }
        public short TagIndex { get; set; }

        public RFIDEventArgs(String TagIdentityText,short TagIndex , DateTime Date)
        {
            this.TagIdentityText = TagIdentityText;
            this.TagReadTime = Date;
            this.TagIndex = TagIndex;
            
        }
    }
}
