using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace MakerRanger.LEDCube
{
    class LEDController
    {
        private const int DefaultClockRate = 100;
        private const int TransactionTimeout = 1000;

        private I2CDevice.Configuration i2cConfig;
        private I2CDevice i2cDevice;

        public bool Enabled { get; set; }

        public byte Address { get; private set; }

        public LEDController(byte address, int clockRateKhz, ref I2CDevice I2CDeviceInstance)
        {
            this.Address = address;
            this.i2cConfig = new I2CDevice.Configuration(this.Address, clockRateKhz);
            if (I2CDeviceInstance == null)
            {
                I2CDeviceInstance = new I2CDevice(this.i2cConfig);
                this.i2cDevice = I2CDeviceInstance;
            }
            else
            {
                this.i2cDevice = I2CDeviceInstance;

            }
        }
        public LEDController(byte address, ref I2CDevice I2CDeviceInstance)
            : this(address, DefaultClockRate, ref I2CDeviceInstance)
        {
        }

        private void Write(byte[] writeBuffer)
        {
            this.i2cDevice.Config = this.i2cConfig;
            // create a write transaction containing the bytes to be written to the device
            I2CDevice.I2CTransaction[] writeTransaction = new I2CDevice.I2CTransaction[]
        {
            I2CDevice.CreateWriteTransaction(writeBuffer)
        };

            // write the data to the device
            int written = this.i2cDevice.Execute(writeTransaction, TransactionTimeout);

            while (written < writeBuffer.Length)
            {
                byte[] newBuffer = new byte[writeBuffer.Length - written];
                Array.Copy(writeBuffer, written, newBuffer, 0, newBuffer.Length);

                writeTransaction = new I2CDevice.I2CTransaction[]
            {
                I2CDevice.CreateWriteTransaction(newBuffer)
            };

                written += this.i2cDevice.Execute(writeTransaction, TransactionTimeout);
            }

            // make sure the data was sent
            if (written != writeBuffer.Length)
            {
                throw new Exception("Could not write to device.");
            }
        }
        private void Read(byte[] readBuffer)
        {
            this.i2cDevice.Config = this.i2cConfig;
            // create a read transaction
            I2CDevice.I2CTransaction[] readTransaction = new I2CDevice.I2CTransaction[]
        {
            I2CDevice.CreateReadTransaction(readBuffer)
        };

            // read data from the device
            int read = this.i2cDevice.Execute(readTransaction, TransactionTimeout);

            // make sure the data was read
            if (read != readBuffer.Length)
            {
                throw new Exception("Could not read from device.");
            }
        }

        protected void WriteToRegister(byte register, byte value)
        {
            this.Write(new byte[] { register, value });
        }
        protected void WriteToRegister(byte register, byte[] values)
        {
            // create a single buffer, so register and values can be send in a single transaction
            byte[] writeBuffer = new byte[values.Length + 1];
            writeBuffer[0] = register;
            Array.Copy(values, 0, writeBuffer, 1, values.Length);
            this.Write(writeBuffer);
        }
        protected void ReadFromRegister(byte register, byte[] readBuffer)
        {
            this.Write(new byte[] { register });
            this.Read(readBuffer);
        }


        public enum PlayerType
        {
            PlayerA,
            PlayerB
        }

        public void ScanningAnimation(PlayerType Player)
        {
            //Format is command followed by 5 byte params
            Byte[] CommandToSend = new byte[6];
            if (Player == PlayerType.PlayerA)
            {
                CommandToSend[0] = 1;
                Debug.Print("LED CUBE SCANNING: " );

            }
            else
            {
                CommandToSend[0] = 2;
                Debug.Print("LED CUBE SCANNING: ");
            }
                                 
            this.Write(CommandToSend);
        }




    }
}
