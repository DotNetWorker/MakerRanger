using System;
using Microsoft.SPOT;
using System.IO;
using System.Collections;

namespace MakerRanger
{
	// Class to handle the persistence of the secret number and guesses for that number, stored off to SD card 
	// Generates new number if one is not already created
	class PersistedStorage
	{
		private const string GuessHistoryFilename = @"SD\GuessHistory.txt";
        private const string GuessHistoryLogFilename = @"SD\GuessHistoryLog.txt";
		
        private const string CurrentSecretNumberFilename = @"SD\SecretNumber.txt";
		private const string PrinterFormsFolder = @"SD\PrinterForms\";
				
		// Array list to receive the guesses 
		public  ArrayList GuessHistory = new ArrayList();

        public int MaxGuessValue;
        public byte qtyMaxNumber;
		
		public PersistedStorage(int MaxGuessValue, byte qtyMaxNumber)
		{
            this.MaxGuessValue = MaxGuessValue;
            this.qtyMaxNumber = qtyMaxNumber;
			// Initialise the secret number and get any previous guesses from SD 
            this.LoadSecretNumber();
			this.LoadPreviousGuesses();
		}
        private ArrayList m_SecretNumber= new ArrayList();
		public ArrayList SecretNumber
		{
			get { return m_SecretNumber; }
			set { m_SecretNumber = value; }
		}

		public void LoadSecretNumber()
		{
			// Try to get the secret number from file, randomly generate one and write to file if it does not 
			// yet exist

			if (File.Exists(CurrentSecretNumberFilename))
			{
				using (var filestream = new FileStream(CurrentSecretNumberFilename, FileMode.Open, FileAccess.Read,FileShare.Read,8))
				{
					using (var reader = new StreamReader(filestream))
					{
                        String line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!(line == null) && !(line.Length == 0))
                            {
                                this.SecretNumber.Add(byte.Parse(line));
                                if (reader.EndOfStream) break;
                            }
                        }
                        //this.SecretNumber = byte.Parse(reader.ReadToEnd());
						reader.Close();
					}
				}
			}
			else
			{
				GenerateNewSecretNumber();
			}
			//Debug.Print("Secret Number is: " + this.SecretNumber.ToString());
		}

		public void GenerateNewSecretNumber()
		{
			// Create a new secret number and store it on the SD card
			Random oRandGen = new Random();
			// for this project don't allow zero as a number as that would be boring, meaning no
			// holes punched in the card
           this.SecretNumber.Clear();
           for (byte i = 0; i < this.qtyMaxNumber-1; i++)
			{
                byte proposednumber;
			    do
                {
                    proposednumber=(byte)(oRandGen.Next(this.MaxGuessValue - 1) + 1);
                    // some reason this is generating too large a number
                    // bodge it and choose another
                } while (proposednumber > (byte)this.MaxGuessValue);
            this.SecretNumber.Add(proposednumber);
			}
            // As this is a new secret number save it to SD card
			SaveSecretNumberToSD();
			ClearGuessHistory();
		}


		// Save the secret number to the SD card so a reset or power cycle does not loose it
		public void SaveSecretNumberToSD()
		{
			using (var filestream = new FileStream(CurrentSecretNumberFilename, FileMode.OpenOrCreate,FileAccess.Write,FileShare.Write,8))
			{
				using (var streamWriter = new StreamWriter(filestream))
				{
					foreach (byte item in this.SecretNumber)
	                {
		                streamWriter.WriteLine(item.ToString());
	                }
					streamWriter.Close();
				}
			}
		}

		public void LoadPreviousGuesses()
		{
           // File.Delete(GuessHistoryFilename);
			// Open the file from the SD card, read it into the arraylist as bytes
			using (var filestream = new FileStream(GuessHistoryFilename, FileMode.OpenOrCreate)) //FileMode.OpenOrCreate,FileAccess.Read, FileShare.Read,8))
			{
				using (var reader = new StreamReader(filestream))
				{
					String line;

                    while ((line = reader.ReadLine()) != null)
					{
                        if (!(line == null) && !(line.Length == 0))
                        {
                        this.GuessHistory.Add(  byte.Parse(line));
						if (reader.EndOfStream) break;
                        }

                       
					}
					reader.Close();
				}
				Debug.Print("Previous Guess Count: " + this.GuessHistory.Count.ToString());
				foreach (var item in this.GuessHistory)
				{
					Debug.Print("Previous Guess: " + item.ToString());
				}
			}
		}

		public void WriteToGuessHistory(byte GuessValue)
		{
			// Check if already in the guess history, if not write to the SD Card and then the guess history array
			if (!this.GuessHistory.Contains(GuessValue))
			{
				using (var filestream = new FileStream(GuessHistoryFilename, FileMode.Append, FileAccess.Write, FileShare.Write, 8))
				{
					using (var streamWriter = new StreamWriter(filestream))
					{
						streamWriter.WriteLine(GuessValue.ToString());
						streamWriter.Close();
						this.GuessHistory.Add(GuessValue);
					}
				}
			}
		}

        public void WriteToGuessHistoryLog(byte GuessValue)
        {
                using (var filestream = new FileStream(GuessHistoryLogFilename, FileMode.Append, FileAccess.Write, FileShare.Write, 8))
                {
                    using (var streamWriter = new StreamWriter(filestream))
                    {
                        streamWriter.WriteLine(System.DateTime.Now.ToString() + " " + GuessValue.ToString());
                        streamWriter.Close();
                        //this.GuessHistory.Add(GuessValue);
                    }
                }
        }

		public void ClearGuessHistory()
		{
			if (File.Exists(GuessHistoryFilename))
			{
                File.Delete(GuessHistoryFilename);
                this.GuessHistory.Clear();
			}
		}

		public ArrayList GetLabelFormNamesFromSD() {
			//Going to be 01 - 99.txt
			ArrayList Results = new ArrayList();
			foreach (var  CurrentFileName in Directory.GetFiles(PrinterFormsFolder))
			{
				Results.Add(Path.GetFileNameWithoutExtension(CurrentFileName));
			}
			return Results;
		}

		//From SD Store get the file text for the supplied form format
		public String GetFormText(String FormName)
		{
			string Result;
			using (var filestream = new FileStream(PrinterFormsFolder  + FormName + ".txt", FileMode.Open , FileAccess.Read,FileShare.Read,8))
			{
				using (var reader = new StreamReader(filestream))
				{
					Result= reader.ReadToEnd();
					reader.Close();
				}
			}
			return Result;
		}



        public static void SendFileToSerial(SerialPortHelper response, string strFilePath)
        {
            FileStream fileToServe = null;
            int BUFFER_SIZE = 1000;
            try
            {
                fileToServe = new FileStream( strFilePath, FileMode.Open, FileAccess.Read);
                long fileLength = fileToServe.Length;
                // Once we know the file length, set the content length.
                //response.ContentLength64 = fileLength;
                // Send HTTP headers. Content lenght is ser
                Debug.Print("File length " + fileLength);
                // Now loops sending all the data.

                byte[] buf = new byte[BUFFER_SIZE];
                for (long bytesSent = 0; bytesSent < fileLength; )
                {
                    // Determines amount of data left.
                    long bytesToRead = fileLength - bytesSent;
                    bytesToRead = bytesToRead < BUFFER_SIZE ? bytesToRead : BUFFER_SIZE;
                    // Reads the data.
                    fileToServe.Read(buf, 0, (int)bytesToRead);
                    // Writes data to browser
                    response.Write(buf, 0, (int)bytesToRead);
                   

                    System.Threading.Thread.Sleep(3);
                    // Updates bytes read.
                    bytesSent += bytesToRead;
                }
                fileToServe.Close();
                response.Flush();
               
            }
            catch (Exception e)
            {
                if (fileToServe != null)
                {
                    fileToServe.Close();
                }
                throw;
            }
        }

	}
}