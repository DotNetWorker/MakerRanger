using System;
using Microsoft.SPOT;
using System.IO;
using System.Collections;


namespace MakerRanger.RFID
{
    class RFIDIdentityDictionary : System.Collections.Hashtable
    {
        public const string DefaultFilename = @"SD\RFIDConfig.txt";

        public void LoadFromFile(string filename)
        {
            // Open the file from the SD card, read it into the Hashtable 
            using (var filestream = new FileStream(filename, FileMode.OpenOrCreate)) //FileMode.OpenOrCreate,FileAccess.Read, FileShare.Read,8))
            {
                using (var reader = new StreamReader(filestream))
                {
                    String line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!(line == null) && !(line.Length == 0))
                        {
                            //Line tab delimited Hex tag name tab then Human Readable Name preceeded by two digit order id
                            //9F93A9    01Pig
                            //22E03D    02Hamster
                            String[] columnArray = line.Split('\t');
                            if (columnArray.Length == 2) { this.Add(columnArray[0], columnArray[1]); };
                            if (reader.EndOfStream) break;
                        }
                    }
                    reader.Close();
                }
                Debug.Print("Loaded: " + this.Count.ToString() + "RFID Definitions");
                foreach (DictionaryEntry item in this)
                {
                    Debug.Print("RFID: " + item.Key.ToString() + " " + item.Value.ToString());
                }
            }
        }

        public void LoadFromFile()
        {
            LoadFromFile(DefaultFilename);
        }

        public string GetName(string TagID)
        {
            foreach (DictionaryEntry item in this)
            {
                if (item.Key.ToString() == TagID)
                {
                    return item.Value.ToString();
                }
            }
            return null;
        }
    }
}
