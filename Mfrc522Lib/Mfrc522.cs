using System.Threading;
using Mfrc522Lib.Constants;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace Mfrc522Lib
{
    public class Mfrc522
    {
        private OutputPort _resetPowerDown, _ss;
        private SPI _spi;
        private SPI.Configuration SPIConfig;
      
        public Mfrc522(SPI SPIInstance, Cpu.Pin ssPin, Cpu.Pin resetPowerDownPin)
            :this(SPIInstance, ssPin)
        {
            _resetPowerDown = new OutputPort(resetPowerDownPin, true);
        }

        public Mfrc522(SPI SPIInstance, Cpu.Pin ssPin)
        {
            _ss = new OutputPort(ssPin, true);
            this.SPIConfig = new SPI.Configuration(
               Cpu.Pin.GPIO_NONE, //latchPin,
               false, // active state
               0,     // setup time
               0,     // hold time 
               false, // clock idle state
               true,  // clock edge
               10000,   // clock rate
               SPI.SPI_module.SPI1);
            _spi = SPIInstance;
        }

        //public void Init(SPI SPIInstance, Cpu.Pin ssPin)
        //{
        //    _ss = new OutputPort(ssPin, true);
        //    this.SPIConfig = new SPI.Configuration(
        //       Cpu.Pin.GPIO_NONE, //latchPin,
        //       false, // active state
        //       0,     // setup time
        //       0,     // hold time 
        //       false, // clock idle state
        //       true,  // clock edge
        //       10000,   // clock rate
        //       SPI.SPI_module.SPI1);
        //    _spi = SPIInstance;

        //    //Reset();
        //}

        public void Reset()
        {
            _resetPowerDown.Write(false);
            Thread.Sleep(50);
            _resetPowerDown.Write(true);
            Thread.Sleep(50);
        }

        public void Init()
        {
            // Force 100% ASK modulation
            WriteRegister(Registers.TxAsk, 0x40);

            // Set CRC to 0x6363
            WriteRegister(Registers.Mode, 0x3D);

            //Set Max antenna gain
            //PCD_WriteRegister(PCD_Register.RFCfgReg, (0x07 << 4)); //Set the antenna gain to max for best pick up
            SetRegisterBits(Registers.ConfigReg, (0x07 << 4));//Set the antenna gain to max for best pick up

            // Enable antenna
            SetRegisterBits(Registers.TxControl, 0x03);
        }

        public bool IsTagPresent()
        {
            // Enable short frames
            WriteRegister(Registers.BitFraming, 0x07);

            // Transceive the Request command to the tag
            Transceive(false, PiccCommands.Request);

            // Disable short frames
            WriteRegister(Registers.BitFraming, 0x00);

            // Check if we found a card
            return GetFifoLevel() == 2 && ReadFromFifoShort() == PiccResponses.AnswerToRequest;
        }

        public Uid ReadUid()
        {
            // Run the anti-collision loop on the card
            Transceive(false, PiccCommands.Anticollision_1, PiccCommands.Anticollision_2);

            // Return tag UID from FIFO
            return new Uid(ReadFromFifo(5));
        }

        public void HaltTag()
        {
            // Transceive the Halt command to the tag
            Transceive(false, PiccCommands.Halt_1, PiccCommands.Halt_2);
        }

        public bool SelectTag(Uid uid)
        {
            // Send Select command to tag
            var data = new byte[7];
            data[0] = PiccCommands.Select_1;
            data[1] = PiccCommands.Select_2;
            uid.FullUid.CopyTo(data, 2);

            Transceive(true, data);

            return GetFifoLevel() == 1 && ReadFromFifo() == PiccResponses.SelectAcknowledge;
        }

        public byte[] ReadBlock(byte blockNumber, Uid uid, byte[] keyA = null, byte[] keyB = null)
        {
            if (keyA != null)
                MifareAuthenticate(PiccCommands.AuthenticateKeyA, blockNumber, uid, keyA);
            else if (keyB != null)
                MifareAuthenticate(PiccCommands.AuthenticateKeyB, blockNumber, uid, keyB);
            else
                return null;

            // Read block
            Transceive(true, PiccCommands.Read, blockNumber);

            return ReadFromFifo(16);
        }

        public bool WriteBlock(byte blockNumber, Uid uid, byte[] data, byte[] keyA = null, byte[] keyB = null)
        {
            if (keyA != null)
                MifareAuthenticate(PiccCommands.AuthenticateKeyA, blockNumber, uid, keyA);
            else if (keyB != null)
                MifareAuthenticate(PiccCommands.AuthenticateKeyB, blockNumber, uid, keyB);
            else
                return false;

            // Write block
            Transceive(true, PiccCommands.Write, blockNumber);

            if (ReadFromFifo() != PiccResponses.Acknowledge)
                return false;

            // Make sure we write only 16 bytes
            var buffer = new byte[16];
            data.CopyTo(buffer, 0);

            Transceive(true, buffer);

            return ReadFromFifo() == PiccResponses.Acknowledge;
        }


        protected void MifareAuthenticate(byte command, byte blockNumber, Uid uid, byte[] key)
        {
            // Put reader in Idle mode
            WriteRegister(Registers.Command, PcdCommands.Idle);

            // Clear the FIFO
            SetRegisterBits(Registers.FifoLevel, 0x80);

            // Create Authentication packet
            var data = new byte[12];
            data[0] = command;
            data[1] = (byte)(blockNumber & 0xFF);
            key.CopyTo(data, 2);
            uid.Bytes.CopyTo(data, 8);

            WriteToFifo(data);

            // Put reader in MfAuthent mode
            WriteRegister(Registers.Command, PcdCommands.MifareAuthenticate);

            // Wait for (a generous) 25 ms
            Thread.Sleep(25);
        }

        protected void Transceive(bool enableCrc, params byte[] data)
        {
            if (enableCrc)
            {
                // Enable CRC
                SetRegisterBits(Registers.TxMode, 0x80);
                SetRegisterBits(Registers.RxMode, 0x80);
            }

            // Put reader in Idle mode
            WriteRegister(Registers.Command, PcdCommands.Idle);

            // Clear the FIFO
            SetRegisterBits(Registers.FifoLevel, 0x80);

            // Write the data to the FIFO
            WriteToFifo(data);

            // Put reader in Transceive mode and start sending
            WriteRegister(Registers.Command, PcdCommands.Transceive);
            SetRegisterBits(Registers.BitFraming, 0x80);

            // Wait for (a generous) 25 ms
            Thread.Sleep(25);

            // Stop sending
            ClearRegisterBits(Registers.BitFraming, 0x80);

            if (enableCrc)
            {
                // Disable CRC
                ClearRegisterBits(Registers.TxMode, 0x80);
                ClearRegisterBits(Registers.RxMode, 0x80);
            }
        }


        protected byte[] ReadFromFifo(int length)
        {
            var buffer = new byte[length];

            for (int i = 0; i < length; i++)
                buffer[i] = ReadRegister(Registers.FifoData);

            return buffer;
        }

        protected byte ReadFromFifo()
        {
            return ReadFromFifo(1)[0];
        }

        protected void WriteToFifo(params byte[] values)
        {
            foreach (var b in values)
                WriteRegister(Registers.FifoData, b);
        }

        protected int GetFifoLevel()
        {
            return ReadRegister(Registers.FifoLevel);
        }


        protected byte ReadRegister(byte register)
        {
            register <<= 1;
            register |= 0x80;

            var writeBuffer = new byte[] { register, 0x00 };

            return TransferSpi(writeBuffer)[1];
        }

        protected ushort ReadFromFifoShort()
        {
            var low = ReadRegister(Registers.FifoData);
            var high = (ushort)(ReadRegister(Registers.FifoData) << 8);

            return (ushort)(high | low);
        }

        protected void WriteRegister(byte register, byte value)
        {
            register <<= 1;

            var writeBuffer = new byte[] { register, value };

            TransferSpi(writeBuffer);
        }

        protected void SetRegisterBits(byte register, byte bits)
        {
            var currentValue = ReadRegister(register);
            WriteRegister(register, (byte)(currentValue | bits));
        }

        protected void ClearRegisterBits(byte register, byte bits)
        {
            var currentValue = ReadRegister(register);
            WriteRegister(register, (byte)(currentValue & ~bits));
        }


        private byte[] TransferSpi(byte[] writeBuffer)
        {
            var readBuffer = new byte[writeBuffer.Length];
            //only one spi device at a time may be on the bus close it down to one thread at a time
            lock (_spi)
            {
                _spi.Config = this.SPIConfig;
                _ss.Write(false);
                _spi.WriteRead(writeBuffer, readBuffer);
                _ss.Write(true);
            }
            return readBuffer;
        }
    }
}
