using System;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;

namespace WordFrequency
{
    class FileInfo
    {
        public FileInfo() {  }

        Dictionary<string, int> words = new Dictionary<string, int>(); 
        public void PrintWords() 
        {
            foreach (KeyValuePair<string, int> pair in words.OrderBy(pair => pair.Key))
            {
                Console.WriteLine("{0}: {1}", pair.Key, pair.Value);
            }
        }

        public void AddToDict(string word) 
        {
            if (words.ContainsKey(word))
            {
                words[word]++;
            }
            else
            {
                words.Add(word, 1);
            }
        }
    }

    class FileParser
    {
        public FileParser(FileInfo fi) { info = fi; }
        FileInfo info;
        public void ParseFile(string fileName)
        {
            char ch;
            string word = "";
            char[] whiteChars = { ' ', '\n', '\t', '\r' };
            int charInt = 0;

            StreamReader reader = new StreamReader(fileName);
            charInt = reader.Read();

            while (charInt != -1)
            {
                ch = (char)charInt;
                if (!(whiteChars.Contains(ch)))
                {
                    word += ch; 
                }
                else if ((whiteChars.Contains(ch)) && (word.Length > 0))
                {
                    info.AddToDict(word);
                    word = "";
                }
                charInt = reader.Read();
            }
            if (word.Length > 0) { info.AddToDict(word); }

            reader.Close();
            reader.Dispose();
        }
    }



    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Argument Error");
                return;
            }

            try
            {
                string f = args[0];
                FileInfo fi = new FileInfo();
                FileParser fp = new FileParser(fi);
                fp.ParseFile(f);
                fi.PrintWords();
            }
            catch (IOException)
            {
                Console.WriteLine("File Error");
            }

        }
    }
}