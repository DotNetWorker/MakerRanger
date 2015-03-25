using System;
using Microsoft.SPOT;

namespace MakerRanger.RFID
{
    public class RFIDEventArgs:EventArgs
    {
        public String TagIdentityText { get; set; }
        public DateTime ReadTime{ get; set; }

        public RFIDEventArgs(String TagIdentityText, DateTime Date)
        {
            this.TagIdentityText = TagIdentityText;
            this.ReadTime = Date;
        }
    }
}
