using System;
using Microsoft.SPOT;
using System.Collections;
using System.IO;

namespace NeonMika.Webserver.FileReader
{
    class SDFileReader
    {

        public static ArrayList GetSecretNumbers(string CurrentSecretNumberFilename)
        {
            // Try to get the secret number from file, randomly generate one and write to file if it does not 
            // yet exist
            ArrayList Result= new ArrayList();
            if (File.Exists(CurrentSecretNumberFilename))
            {
                using (var filestream = new FileStream(CurrentSecretNumberFilename, FileMode.Open, FileAccess.Read, FileShare.Read, 8))
                {
                    using (var reader = new StreamReader(filestream))
                    {
                        String line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!(line == null) && !(line.Length == 0))
                            {
                                Result.Add(byte.Parse(line));
                                if (reader.EndOfStream) break;
                            }
                        }
                        //this.SecretNumber = byte.Parse(reader.ReadToEnd());
                        reader.Close();
                    }
                }
            }
            return Result;
          
        }
    }
}
