using System;
using Microsoft.SPOT;

namespace MakerRanger.LCD
{
    class LCDMessage
    {
        public LCDScreen.LCDStates LCDState { get; set; }
        public string MessageArgument { get; set; }

        public LCDMessage(LCDScreen.LCDStates LCDState, string LCDArgument)
        {
            this.LCDState = LCDState;
            this.MessageArgument = LCDArgument;
        }
    }
}
