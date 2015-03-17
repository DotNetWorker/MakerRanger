using System;
using System.IO.Ports;
using System.Text;
using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System.Threading;
using SecretLabs.NETMF.Hardware;
using Microsoft.SPOT.Hardware;


namespace MakerRanger
{
    public class SerialPortHelper
    {
        static SerialPort serialPort;
        
        const int bufferMax = 1024;
        static byte[] buffer = new Byte[bufferMax];

        public SerialPortHelper(string portName = "", int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            if (portName == "") { portName = SerialPorts.COM1;  }
            serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            serialPort.ReadTimeout = 10; // Set to 10ms. Default is -1?!
            //serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
            serialPort.Open();
        }

        //private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    lock (buffer)
        //    {
        //        int bytesReceived = serialPort.Read(buffer, bufferLength, bufferMax - bufferLength);
        //        if (bytesReceived > 0)
        //        {
        //            bufferLength += bytesReceived;
        //            if (bufferLength >= bufferMax)
        //                throw new ApplicationException("Buffer Overflow.  Send shorter lines, or increase lineBufferMax.");
        //        }
        //    }
        //}

        //public string ReadLine()
        //{
        //    string line = "";

        //    lock (buffer)
        //    {
        //        //-- Look for Return char in buffer --
        //        for (int i = 0; i < bufferLength; i++)
        //        {
        //            //-- Consider EITHER CR or LF as end of line, so if both were received it would register as an extra blank line. --
        //            if (buffer[i] == '\r' || buffer[i] == '\n')
        //            {
        //                buffer[i] = 0; // Turn NewLine into string terminator
        //                line = "" + new string(Encoding.UTF8.GetChars(buffer)); // The "" ensures that if we end up copying zero characters, we'd end up with blank string instead of null string.
        //                //Debug.Print("LINE: <" + line + ">");
        //                bufferLength = bufferLength - i - 1;
        //                Array.Copy(buffer, i + 1, buffer, 0, bufferLength); // Shift everything past NewLine to beginning of buffer
        //                break;
        //            }
        //        }
        //    }

        //    return line;
        //}

        public void Print(string line )
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            byte[] bytesToSend = encoder.GetBytes(line);
            
            serialPort.Write(bytesToSend, 0, bytesToSend.Length);
            serialPort.Flush();
            serialPort.DiscardInBuffer();
            
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            serialPort.Write(buffer, offset, count);
        }


        public void Print(byte[] bytesTosend)
        {
            serialPort.Write(bytesTosend, 0, bytesTosend.Length);
            serialPort.Flush();
            serialPort.DiscardInBuffer();
        }

        public void Flush()
        {
            serialPort.Flush();
        }
        public void PrintLine(string line)
        {
            Print(line + "\r\n");
        }

        //public void PrintClear()
        //{
        //    byte[] bytesToSend = new byte[2];
        //    bytesToSend[0] = 254;
        //    bytesToSend[1] = 1;
        //    serialPort.Write(bytesToSend, 0, 2);
        //    Thread.Sleep(500); 
        //}
    }
}
