using System;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Reflection.Metadata;

namespace BlockAlignment
{
    /// <summary>
    /// Code was taken from lab materials of course NPRG035, MFF UK 2023.
    /// And slightly changed.
    /// </summary>
    public class ProgramInputOutputState : IDisposable
    {
        public const string ArgumentErrorMessage = "Argument Error";
        public const string FileErrorMessage = "File Error";

        public StreamReader? Reader { get; private set; }
        public StreamWriter? Writer { get; private set; }

        public int maxWidth { get; private set; }
        public bool InitializeFromCommandLineArgs(string[] args)
        {
            if (args.Length != 3)
            {
                Console.Write(ArgumentErrorMessage + "\n");
                return false;
            }

            try
            {
                Reader = new StreamReader(args[0]);
            }
            catch (IOException)
            {
                Console.Write(FileErrorMessage + "\n");
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                Console.Write(FileErrorMessage + "\n");
                return false;
            }
            catch (ArgumentException)
            {
                Console.Write(FileErrorMessage + "\n");
                return false;
            }

            try
            {
                Writer = new StreamWriter(args[1]);
            }
            catch (IOException)
            {
                Console.Write(FileErrorMessage + "\n");
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                Console.Write(FileErrorMessage + "\n");
                return false;
            }
            catch (ArgumentException)
            {
                Console.Write(FileErrorMessage + "\n");
                return false;
            }


            try
            {
                maxWidth = Convert.ToInt32(args[2]);
            }
            catch (Exception)
            {
                Console.Write(ArgumentErrorMessage + "\n");
                return false;
            }
            if (maxWidth < 1)
            {
                Console.Write(ArgumentErrorMessage + "\n");
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            Reader?.Dispose();
            Writer?.Dispose();
        }
    }
    
    public class ResultOutput
    {
        public ResultOutput(StreamWriter writer) 
        {
            this.writer = writer;
        }
        private StreamWriter writer;

        public void WriteChar(char ch)
        {
            writer?.Write(ch);
        }

        public void WriteString(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                WriteChar(text[i]);
            }
        }

    }
    public class WordProcessor
    {
        public WordProcessor(int maxWidth, ResultOutput resultOutput) 
        {
            this.maxWidth = maxWidth;
            this.resultOutput = resultOutput;
        }

        ResultOutput resultOutput;
        public int maxWidth { get; private set; }
        private List<string> LineToPrint = new List<string>();
        private int charsInLine = 0;
        bool newParagrah = false;

        int LineWordCount = 0;
        int paragraphWords = 0;
        public void IncrementCount()
        {
            LineWordCount++;
        }

        void PrintLastParagrLine()
        {
            if (newParagrah)
            {
                resultOutput.WriteChar('\n');
                newParagrah = false;
            }

            for (int i = 0;i < LineToPrint.Count() - 1;i++)
            {
                resultOutput.WriteString(LineToPrint[i] + " ");
            }
            resultOutput.WriteString(LineToPrint[LineToPrint.Count() - 1]);
            CleanLine();
        }
        
        public void ParseEOL()
        {
            if (LineWordCount == 0 && paragraphWords != 0) // empty line => end of the previous pragarph
            {
                PrintLastParagrLine();                
                paragraphWords = 0;
                newParagrah = true;
            }
            else // the end of line in the middle of the paragraph, end of non empty line
            {
                paragraphWords += LineWordCount;
                LineWordCount = 0;
            }
        }

        public void ParseEOF()
        {
            if (LineToPrint.Count > 0)
            {
                PrintLastParagrLine();
            }
            resultOutput.WriteChar('\n');
        }

        void CleanLine()
        {
            LineToPrint.Clear();
            charsInLine = 0;
        }

        public void ParseWord(string word)
        {
            IncrementCount();
            int wordsInLine = LineToPrint.Count();
            int wordLength = word.Length;
            int sum = wordLength + charsInLine + wordsInLine; // number of chars in word + number of chars in Line + min number of spaces

            if (sum <= maxWidth)
            {
                LineToPrint.Add(word);
                charsInLine += wordLength;
            } 
            else
            {
                PrintAlignedLine();
                CleanLine();
                LineToPrint.Add(word);
                charsInLine += wordLength;
            }
        }

        void PrintSpaces(int number)
        {
            for (int i = 0; i < number; i++) 
            {
                resultOutput.WriteChar(' ');
            }
        }

        void PrintAlignedLine()
        {
            if (newParagrah)
            {
                resultOutput.WriteString("\n\n");
                newParagrah = false;
            }

            int numberOfSpaces = maxWidth - charsInLine;
            if (numberOfSpaces <= 0)  // Length of word is equal or greater than maxWidth
            {
                resultOutput.WriteString(LineToPrint[0] + "\n");
            }
            else
            {
                // Places that need whitespaces are words.Count() - 1
                int placesForSpaces = LineToPrint.Count() - 1;
                if (placesForSpaces == 0)
                {
                    resultOutput.WriteString(LineToPrint[0] + '\n');
                }
                else
                {
                    int spacesForAll = numberOfSpaces / placesForSpaces;
                    int LeftSpaces = numberOfSpaces % placesForSpaces;
                    for (int i = 0; i < LineToPrint.Count - 1; i++)
                    {
                        resultOutput.WriteString(LineToPrint[i]);
                        int spacesToPrint = spacesForAll;
                        if (LeftSpaces > 0)
                        {
                            spacesToPrint += 1;
                            LeftSpaces--;
                        }

                        PrintSpaces(spacesToPrint);
                    }
                    resultOutput.WriteString(LineToPrint[LineToPrint.Count - 1] + '\n');

                }
            }
        }

    }

    public class FileParser
    {
        WordProcessor info;
        StreamReader reader;
        public FileParser(WordProcessor fi, StreamReader reader)
        {
            info = fi;
            this.reader = reader;

        }

        public void ParseFile()
        {
            char ch;
            string word = "";
            char[] whiteChars = { ' ', '\t', '\n' };
            int charInt = 0;

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
                    info.ParseWord(word);
                    word = "";
                }
                if ((ch == '\n'))
                {
                    info.ParseEOL();
                }
                charInt = reader.Read();
            }
            if (word.Length > 0) 
            {
                info.ParseWord(word);   
            }
            info.ParseEOF();
        }


    }

    public class Program
    {
        static void Main(string[] args)
        {
            var state = new ProgramInputOutputState();
            if (!state.InitializeFromCommandLineArgs(args))
            {
                return;
            }
                        
            ResultOutput resultOutput = new ResultOutput(state.Writer!);
            WordProcessor wordProcessor = new WordProcessor(state.maxWidth, resultOutput);
            FileParser fp = new FileParser(wordProcessor, state.Reader!);
            
            fp.ParseFile();
            
            state.Dispose();

        }
    }


    /// <summary>
    /// Created for test purposes only. To simulate Main() function
    /// </summary>
    public static class ProgramManager
    {
        public static void RunMainFunction(string[] args)
        {
            var state = new ProgramInputOutputState();
            if (!state.InitializeFromCommandLineArgs(args))
            {
                return;
            }

            ResultOutput resultOutput = new ResultOutput(state.Writer!);
            WordProcessor wordProcessor = new WordProcessor(state.maxWidth, resultOutput);
            FileParser fp = new FileParser(wordProcessor, state.Reader!);

            fp.ParseFile();

            state.Dispose();

        }
    }
}