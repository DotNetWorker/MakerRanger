using System;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;

namespace netduino.helpers.Hardware
{
    /// <summary>
    /// MAX7219 / 7221 LED display driver
    /// http://datasheets.maxim-ic.com/en/ds/MAX7219-MAX7221.pdf
    /// </summary>
    public class Max72197221 : IDisposable
    {
        public enum RegisterAddressMap
        {
            NoOp,
            Digit0,
            Digit1,
            Digit2,
            Digit3,
            Digit4,
            Digit5,
            Digit6,
            Digit7,
            DecodeMode,
            Intensity,
            ScanLimit,
            Shutdown,
            DisplayTest = 0x0F
        }

        public enum ShutdownRegister
        {
            ShutdownMode,
            NormalOperation
        }

        /// <summary>
        /// Logic OR the values of the DecodeModeRegister together or use DecodeDigitAll to decode all digits
        /// </summary>
        [Flags]
        public enum DecodeModeRegister
        {
            NoDecodeMode,
            DecodeDigit0,
            DecodeDigit1,
            DecodeDigit2,
            DecodeDigit3,
            DecodeDigit4,
            DecodeDigit5,
            DecodeDigit6,
            DecodeDigit7,
            DecodeDigitAll = 255
        }

        //public enum CodeBFont
        //{
        //    Zero,
        //    One,
        //    Two,
        //    Three,
        //    Four,
        //    Five,
        //    Six,
        //    Seven,
        //    Eight,
        //    Nine,
        //    Dash,
        //    E,
        //    H,
        //    L,
        //    P,
        //    Blank = 0x0F
        //}

        public enum CodeBDecimalPoint
        {
            OFF,
            ON = 0x80
        }

        public enum DisplayTestRegister
        {
            NormalOperation,
            DisplayTestMode
        }

        public Boolean DualDisplay { get; set; }

        private SPI.Configuration extendedSpiConfig;

        /// <summary>
        /// Instantiates a Max7219/7221 LED driver, using the netduino's hard SPI interface by default:
        /// If multiple Max7219/7221 chips are chained together, the CS pin must be controlled by the caller instead of the netduino handling it.
        /// CLK = pin 13
        /// MOSI = pin 11
        /// CS = pin 10
        /// </summary>
        /// <param name="chipSelect">Chip Select pin.</param>
        /// <param name="spiModule">SPI module, SPI 1 is used by default.</param>
        /// <param name="speedKHz">Speed of the SPI bus in kHz. Set @ 10MHz by default (max chip speed).</param>
        public Max72197221(ref SPI SPIInstance, Cpu.Pin chipSelect, SPI.SPI_module spiModule = SPI.SPI_module.SPI1, uint speedKHz = (uint)10000, Boolean DualDisplay = false)
        {
            this.DualDisplay = DualDisplay;
            Spi = SPIInstance;
            
            this.extendedSpiConfig = new SPI.Configuration(chipSelect,   //Chip Select pin
                                                false,              //Chip Select Active State
                                                0,                  //Chip Select Setup Time
                                                0,                  //Chip Select Hold Time
                                                true,               //Clock Idle State
                                                true,               //Clock Edge
                                                speedKHz,               //Clock Rate (kHz)
                                                spiModule);//SPI Module
                
                
                //this.extendedSpiConfig = new ExtendedSpiConfiguration(
                //SPI_mod: spiModule,
                //ChipSelect_Port: chipSelect,
                //ChipSelect_ActiveState: false,
                //ChipSelect_SetupTime: 0,
                //ChipSelect_HoldTime: 0,
                //Clock_IdleState: true,
                //Clock_Edge: true,
                //Clock_RateKHz: speedKHz,
                //BitsPerTransfer: 16);
            if (Spi == null)
            {
                Spi = new SPI(this.extendedSpiConfig);
            }
            else
            {
                // Spi.Config = this.extendedSpiConfig;
            }


            DigitScanLimitSafety = true;

            SpiBuffer = new ushort[1];
            SpiDoubleBuffer = new ushort[2];
        }


        public void SetIntensity(byte value)
        {
            SetIntensity(value, value);
        }

        public void SetIntensity(byte value, byte value2)
        {
            if ((value < 0 || value > 15) & (value2 < 0 || value2 > 15))
            {
                throw new ArgumentOutOfRangeException("value");
            }
            if (this.DualDisplay)
            {
                Write((byte)RegisterAddressMap.Intensity, value, value2);

            }
            else
            {
                Write((byte)RegisterAddressMap.Intensity, value);
            }
        }

        public bool DigitScanLimitSafety { get; set; }

        public void SetDigitScanLimit(byte value)
        {
            if (value < 0 || value > 7)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            if (DigitScanLimitSafety && value < 3)
            {
                throw new ArgumentException("SetDigitScanLimitSafety value should not be set too low in order to keep within datasheet limits and protect your matrix or digits from burning out.");
            }
            if (this.DualDisplay)
            {
                Write((byte)RegisterAddressMap.ScanLimit, value, value);
            }
            else
            {
                Write((byte)RegisterAddressMap.ScanLimit, value);

            }
        }

        public void SetDecodeMode(DecodeModeRegister value)
        {
            if (value < DecodeModeRegister.NoDecodeMode || value > DecodeModeRegister.DecodeDigitAll)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            if (this.DualDisplay)
            {
                Write((byte)RegisterAddressMap.DecodeMode, (byte)value, (byte)value);
            }
            else
            {
                Write((byte)RegisterAddressMap.DecodeMode, (byte)value);
            }

        }

        public void Shutdown(ShutdownRegister value = ShutdownRegister.ShutdownMode)
        {
            if (value != ShutdownRegister.NormalOperation && value != ShutdownRegister.ShutdownMode)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            if (this.DualDisplay)
            { Write((byte)RegisterAddressMap.Shutdown, (byte)value, (byte)value); }
            else
            {
                Write((byte)RegisterAddressMap.Shutdown, (byte)value);
            }


        }

        public void SetDisplayTest(DisplayTestRegister value)
        {
            if (value != DisplayTestRegister.DisplayTestMode && value != DisplayTestRegister.NormalOperation)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            if (this.DualDisplay)
            {
                Write((byte)RegisterAddressMap.DisplayTest, (byte)value, (byte)value);
            }
            else
            {
                Write((byte)RegisterAddressMap.DisplayTest, (byte)value);
            }

        }

        /// <summary>
        /// Send an 8x8 matrix pattern to the LED driver.
        /// The LED driver requires DecodeMode = DecodeModeRegister.NoDecodeMode first.
        /// </summary>
        /// <param name="matrix">8x8 bitmap to be displayed.</param>
        public void Display(byte[] matrix)
        {
            if (matrix.Length != 8)
            {
                throw new ArgumentOutOfRangeException("matrix");
            }
            var rowNumber = (byte)RegisterAddressMap.Digit0;
            foreach (var rowData in matrix)
            {
                Write(rowNumber, rowData);

                rowNumber++;
            }
        }

        /// <summary>
        /// Send an 8x8 matrix pattern to the LED driver.
        /// The LED driver requires DecodeMode = DecodeModeRegister.NoDecodeMode first.
        /// </summary>
        /// <param name="matrix">8x8 bitmap to be displayed.</param>
        public void Display(byte[] matrix, byte[] matrix2)
        {
            if ((matrix.Length != 8) | (matrix2.Length != 8))
            {
                throw new ArgumentOutOfRangeException("matrix");
            }
            var rowNumber = (byte)RegisterAddressMap.Digit0;
            int rowIndex = 0;
            foreach (var rowData in matrix)
            {
                Write(rowNumber, rowData, matrix2[rowIndex] );
                rowNumber++;
                rowIndex++;
            }


        }


        protected void Write(byte register, byte value)
        {
            SpiBuffer[0] = register;
            SpiBuffer[0] <<= 8;
            SpiBuffer[0] |= value;
            lock (Spi)
            {
                Spi.Config = this.extendedSpiConfig;
                Spi.Write(SpiBuffer);
            }
        }

        //protected void Write(byte register, byte value)
        //{
        //    SpiBuffer[0] = register;
        //    SpiBuffer[0] <<= 8;
        //    SpiBuffer[0] |= value;
        //    SpiBuffer[1] = register;
        //    SpiBuffer[1] <<= 8;
        //    SpiBuffer[1] |= value;
        //    //SpiBuffer[0] <<= 8;
        //    //SpiBuffer[0] |= register;
        //    //SpiBuffer[0] <<= 8;
        //    //SpiBuffer[0] |= value;
        //    lock (Spi)
        //    {
        //        Spi.Config = this.extendedSpiConfig;
        //        Spi.Write(SpiBuffer);
        //    }
        //} 
        protected void Write(byte register, byte value, byte value2)
        {
            SpiDoubleBuffer[0] = register;
            SpiDoubleBuffer[0] <<= 8;
            SpiDoubleBuffer[0] |= value;
            SpiDoubleBuffer[1] = register;
            SpiDoubleBuffer[1] <<= 8;
            SpiDoubleBuffer[1] |= value2;
            //SpiBuffer[0] <<= 8;
            //SpiBuffer[0] |= register;
            //SpiBuffer[0] <<= 8;
            //SpiBuffer[0] |= value;
            lock (Spi)
            {
                Spi.Config = this.extendedSpiConfig;
                Spi.Write(SpiDoubleBuffer);
            }


        }

        public void Dispose()
        {
            Spi.Dispose();
            Spi = null;
            SpiBuffer = null;
            SpiDoubleBuffer = null;
        }

        protected SPI Spi;
        protected ushort[] SpiBuffer;
        protected ushort[] SpiDoubleBuffer;
    }
}
