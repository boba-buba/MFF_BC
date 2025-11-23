using System;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Reflection.Metadata;

namespace SeveralFilesBlockAlignment
{
    /// <summary>
    /// Code was taken from lab materials of course NPRG035, MFF UK 2023.
    /// And slightly changed.
    /// </summary>
    public class ProgramInputOutputState : IDisposable
    {
        public const string ArgumentErrorMessage = "Argument Error";
        public const string FileErrorMessage = "File Error";
        const string higlightSpacesOption = "--highlight-spaces";

        public TextWriter? Writer { get; private set; }

        public int maxWidth { get; private set; }
        public bool higlightSpaces { get; private set; } = false;
        public int firstFileIndex { get; private set; } = 0;
        public int lastFileIndex { get; private set; } = int.MaxValue;

        public bool InitializeFromCommandLineArgs(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Write(ArgumentErrorMessage + "\n");
                return false;
            }

            if (args[0] == higlightSpacesOption) 
            {
                if (args.Length < 4) 
                {
                    Console.Write(ArgumentErrorMessage + "\n");
                    return false; 
                }
                higlightSpaces = true;
                firstFileIndex = 1;
            }

            try
            {
                Writer = new StreamWriter(args[args.Length-2]);
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
                maxWidth = Convert.ToInt32(args[args.Length-1]);
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
            lastFileIndex = args.Length - 2;
            return true;
        }

        public void Dispose()
        {
            Writer?.Dispose();
        }
    }

    public class ResultOutput
    {
        public ResultOutput(TextWriter writer)
        {
            this.writer = writer;
        }
        public TextWriter writer { get; private set; }

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
        char spaceSymbol = ' ';
        string nextLineSymbol = "\n";

        public WordProcessor(int maxWidth, ResultOutput resultOutput, bool higlightSpaces)
        {
            this.maxWidth = maxWidth;
            this.resultOutput = resultOutput;
            if (higlightSpaces) 
            { 
                spaceSymbol = '.'; 
                nextLineSymbol = "<-\n";
            }
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
                resultOutput.WriteString(nextLineSymbol);
                newParagrah = false;
            }

            for (int i = 0; i < LineToPrint.Count() - 1; i++)
            {
                resultOutput.WriteString(LineToPrint[i] + spaceSymbol);
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
            resultOutput.WriteString(nextLineSymbol);
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
                resultOutput.WriteChar(spaceSymbol);
            }
        }

        void PrintAlignedLine()
        {
            if (newParagrah)
            {
                resultOutput.WriteString(nextLineSymbol+nextLineSymbol);
                newParagrah = false;
            }

            int numberOfSpaces = maxWidth - charsInLine;
            if (numberOfSpaces <= 0)  // Length of word is equal or greater than maxWidth
            {
                resultOutput.WriteString(LineToPrint[0] + nextLineSymbol);
            }
            else
            {
                // Places that need whitespaces are words.Count() - 1
                int placesForSpaces = LineToPrint.Count() - 1;
                if (placesForSpaces == 0)
                {
                    resultOutput.WriteString(LineToPrint[0] + nextLineSymbol);
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
                    resultOutput.WriteString(LineToPrint[LineToPrint.Count - 1] + nextLineSymbol);

                }
            }
        }

    }

    public class FileParser
    {
        WordProcessor info;
        int fileCounter;
        public FileParser(WordProcessor fi, int fileCounter)
        {
            info = fi;
            this.fileCounter = fileCounter;
        }

        public void ParseFile(TextReader reader)
        {
            char ch;
            string word = "";
            char[] whiteChars = { ' ', '\t', '\n' };
            int charInt = 0;
            fileCounter--;

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
            if (fileCounter == 0)
            {
                info.ParseEOF();
            }   
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
            WordProcessor wordProcessor = new WordProcessor(state.maxWidth, resultOutput, state.higlightSpaces);
            int fileCounter = state.lastFileIndex - state.firstFileIndex;
            FileParser fp = new FileParser(wordProcessor, fileCounter);

            for (int i = state.firstFileIndex; i < state.lastFileIndex; i++)
            {
                try
                {
                    StreamReader fileToParse = new StreamReader(args[i]);
                    fp.ParseFile(fileToParse);
                }
                catch (Exception) 
                {
                    StringReader empty = new StringReader("");
                    fp.ParseFile(empty);
                }
            }
            state.Dispose();

        }
    }

}

/* Tests 
 namespace SeveralFilesBlockAlignment_UnitTests
{
    public class SeveralFilesBlockAlignment_Tests
    {
        [Fact]
        public void HiglightSpacesOption_LessThanFourParameters()
        {
            // Arrange
            string[] args = { "--highlight-spaces", "ex01.out", "17" };
            var state = new ProgramInputOutputState();

            using (var consoleOutput = new ConsoleOutputManager())
            {
                // Act
                Assert.False(state.InitializeFromCommandLineArgs(args));
                
                // Assert
                Assert.Equal("Argument Error\n", consoleOutput.GetOuput());
            }
        }

        [Fact]
        public void ThreeFiles_OneParagraphEach()
        {
            // Arrange
            string[] args = { 
                "If a train station is where the train stops, what is a work station?",
                "If a train station is where the train stops, what is a work station?",
                "If a train station is where the train stops, what is a work station?"};
            string expectedOutput = """
                If     a    train
                station  is where
                the  train stops,
                what  is  a  work
                station?   If   a
                train  station is
                where  the  train
                stops,  what is a
                work  station? If
                a  train  station
                is    where   the
                train stops, what
                is     a     work
                station?
                
                """;
            
            StringWriter outputWriter = new StringWriter();

            ResultOutput resultOutput = new ResultOutput(outputWriter);
            int maxWidth = 17;
            bool higlightSpaces = false;
            WordProcessor wordProcessor = new WordProcessor(maxWidth, resultOutput, higlightSpaces);
            int fileCounter = args.Length;
            FileParser fp = new FileParser(wordProcessor, fileCounter);

            // Act
            for (int i = 0; i < fileCounter; i++)
            {
                try
                {
                    StringReader fileToParse = new StringReader(args[i]);
                    fp.ParseFile(fileToParse);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            // Assert
            Assert.Equal(expectedOutput, resultOutput.writer.ToString());

        }


        [Fact]
        public void ThreeFiles_SecondFileIsEmpty()
        {
            // Arrange
            string[] args = {
                "If a train station is where the train stops, what is a work station?",
                "",
                "If a train station is where the train stops, what is a work station?"};
            string expectedOutput = """
                If     a    train
                station  is where
                the  train stops,
                what  is  a  work
                station?   If   a
                train  station is
                where  the  train
                stops,  what is a
                work station?
                
                """;

            StringWriter outputWriter = new StringWriter();

            ResultOutput resultOutput = new ResultOutput(outputWriter);
            int maxWidth = 17;
            bool higlightSpaces = false;
            WordProcessor wordProcessor = new WordProcessor(maxWidth, resultOutput, higlightSpaces);
            int fileCounter = args.Length;
            FileParser fp = new FileParser(wordProcessor, fileCounter);

            // Act
            for (int i = 0; i < fileCounter; i++)
            {
                try
                {
                    StringReader fileToParse = new StringReader(args[i]);
                    fp.ParseFile(fileToParse);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            // Assert
            Assert.Equal(expectedOutput, resultOutput.writer.ToString());

        }

        [Fact]
        public void TwoFiles_FirstEndsWithEOL_SecondStartsWithEOL()
        {
            // Arrange
            string[] args = {
                "If a train station is where the train stops, what is a work station?\n",
                "\nIf a train station is where the train stops, what is a work station?"};
            string expectedOutput = """
                If     a    train
                station  is where
                the  train stops,
                what  is  a  work
                station?

                If     a    train
                station  is where
                the  train stops,
                what  is  a  work
                station?
                
                """;

            StringWriter outputWriter = new StringWriter();

            ResultOutput resultOutput = new ResultOutput(outputWriter);
            int maxWidth = 17;
            bool higlightSpaces = false;
            WordProcessor wordProcessor = new WordProcessor(maxWidth, resultOutput, higlightSpaces);
            int fileCounter = args.Length;
            FileParser fp = new FileParser(wordProcessor, fileCounter);

            // Act
            for (int i = 0; i < fileCounter; i++)
            {
                try
                {
                    StringReader fileToParse = new StringReader(args[i]);
                    fp.ParseFile(fileToParse);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            // Assert
            Assert.Equal(expectedOutput, resultOutput.writer.ToString());

        }

        [Fact]
        public void ThreeEmptyFiles()
        {
            // Arrange
            string[] args = {"", "", ""};
            string expectedOutput = """
                

                """;

            StringWriter outputWriter = new StringWriter();

            ResultOutput resultOutput = new ResultOutput(outputWriter);
            int maxWidth = 17;
            bool higlightSpaces = false;
            WordProcessor wordProcessor = new WordProcessor(maxWidth, resultOutput, higlightSpaces);
            int fileCounter = args.Length;
            FileParser fp = new FileParser(wordProcessor, fileCounter);

            // Act
            for (int i = 0; i < fileCounter; i++)
            {
                try
                {
                    StringReader fileToParse = new StringReader(args[i]);
                    fp.ParseFile(fileToParse);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            // Assert
            Assert.Equal(expectedOutput, resultOutput.writer.ToString());

        }

        [Fact]
        public void TwoFiles_EachHasTwoParagraphs_ResultMustHaveTreeParagraphs()
        {
            // Arrange
            string[] args = {
                "If a train station is where the\n\ntrain stops, what is a work station?",
                "If a train station is where the\n\ntrain stops, what is a work station?"};
            string expectedOutput = """
                If     a    train
                station  is where
                the

                train stops, what
                is     a     work
                station?   If   a
                train  station is
                where the

                train stops, what
                is     a     work
                station?
                
                """;

            StringWriter outputWriter = new StringWriter();

            ResultOutput resultOutput = new ResultOutput(outputWriter);
            int maxWidth = 17;
            bool higlightSpaces = false;
            WordProcessor wordProcessor = new WordProcessor(maxWidth, resultOutput, higlightSpaces);
            int fileCounter = args.Length;
            FileParser fp = new FileParser(wordProcessor, fileCounter);

            // Act
            for (int i = 0; i < fileCounter; i++)
            {
                try
                {
                    StringReader fileToParse = new StringReader(args[i]);
                    fp.ParseFile(fileToParse);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            // Assert
            Assert.Equal(expectedOutput, resultOutput.writer.ToString());

        }

        [Fact]
        public void HighlightSpaces_OneFile_OneParagraph()
        {
            // Arrange
            string[] args = {"If a train station is where the train stops, what is a work station?"};
            string expectedOutput = """
                If.....a....train<-
                station..is.where<-
                the..train.stops,<-
                what..is..a..work<-
                station?<-
                
                """;

            StringWriter outputWriter = new StringWriter();

            ResultOutput resultOutput = new ResultOutput(outputWriter);
            int maxWidth = 17;
            bool higlightSpaces = true;
            WordProcessor wordProcessor = new WordProcessor(maxWidth, resultOutput, higlightSpaces);
            int fileCounter = args.Length;
            FileParser fp = new FileParser(wordProcessor, fileCounter);

            // Act
            for (int i = 0; i < fileCounter; i++)
            {
                try
                {
                    StringReader fileToParse = new StringReader(args[i]);
                    fp.ParseFile(fileToParse);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            // Assert
            Assert.Equal(expectedOutput, resultOutput.writer.ToString());

        }

        [Fact]
        public void HighlightSpaces_ThreeFiles_EachOneParagraph_ResultMustHaveOneParagraph()
        {
            // Arrange
            string[] args = { 
                "If a train station is where the train stops, what is a work station?",
                "If a train station is where the train stops, what is a work station?",
                "If a train station is where the train stops, what is a work station?"
            };

            string expectedOutput = """
                If.....a....train<-
                station..is.where<-
                the..train.stops,<-
                what..is..a..work<-
                station?...If...a<-
                train..station.is<-
                where..the..train<-
                stops,..what.is.a<-
                work..station?.If<-
                a..train..station<-
                is....where...the<-
                train.stops,.what<-
                is.....a.....work<-
                station?<-
                
                """;

            StringWriter outputWriter = new StringWriter();

            ResultOutput resultOutput = new ResultOutput(outputWriter);
            int maxWidth = 17;
            bool higlightSpaces = true;
            WordProcessor wordProcessor = new WordProcessor(maxWidth, resultOutput, higlightSpaces);
            int fileCounter = args.Length;
            FileParser fp = new FileParser(wordProcessor, fileCounter);

            // Act
            for (int i = 0; i < fileCounter; i++)
            {
                try
                {
                    StringReader fileToParse = new StringReader(args[i]);
                    fp.ParseFile(fileToParse);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            // Assert
            Assert.Equal(expectedOutput, resultOutput.writer.ToString());

        }

        [Fact]
        public void HighlightSpaces_ThreeEmptyFiles()
        {
            // Arrange
            string[] args = { "", "", "" };

            string expectedOutput = """
                <-

                """;

            StringWriter outputWriter = new StringWriter();

            ResultOutput resultOutput = new ResultOutput(outputWriter);
            int maxWidth = 17;
            bool higlightSpaces = true;
            WordProcessor wordProcessor = new WordProcessor(maxWidth, resultOutput, higlightSpaces);
            int fileCounter = args.Length;
            FileParser fp = new FileParser(wordProcessor, fileCounter);

            // Act
            for (int i = 0; i < fileCounter; i++)
            {
                try
                {
                    StringReader fileToParse = new StringReader(args[i]);
                    fp.ParseFile(fileToParse);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            // Assert
            Assert.Equal(expectedOutput, resultOutput.writer.ToString());

        }

        [Fact]
        public void HighlightSpaces_OneFile_TwoParagraphs()
        {
            // Arrange
            string[] args = { "If a train station is where the\n\ntrain stops, what is a work station?" };

            string expectedOutput = """
                If.....a....train<-
                station..is.where<-
                the<-
                <-
                train.stops,.what<-
                is.....a.....work<-
                station?<-
                
                """;

            StringWriter outputWriter = new StringWriter();

            ResultOutput resultOutput = new ResultOutput(outputWriter);
            int maxWidth = 17;
            bool higlightSpaces = true;
            WordProcessor wordProcessor = new WordProcessor(maxWidth, resultOutput, higlightSpaces);
            int fileCounter = args.Length;
            FileParser fp = new FileParser(wordProcessor, fileCounter);

            // Act
            for (int i = 0; i < fileCounter; i++)
            {
                try
                {
                    StringReader fileToParse = new StringReader(args[i]);
                    fp.ParseFile(fileToParse);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            // Assert
            Assert.Equal(expectedOutput, resultOutput.writer.ToString());

        }

    }
}


Helping class for tests:

namespace ConsoleOutputHandler
{
    /// <summary>
    /// Created for test purposes. To get Console output into tests.
    /// </summary>
    public class ConsoleOutputManager : IDisposable
    {
        /// <summary>
        /// This code was taken from http://www.vtrifonov.com/2012/11/getting-console-output-within-unit-test.html
        /// </summary>
        private StringWriter stringWriter;
        private TextWriter originalOutput;

        public ConsoleOutputManager()
        {
            stringWriter = new StringWriter();
            originalOutput = Console.Out;
            Console.SetOut(stringWriter);
        }

        public string GetOuput()
        {
            return stringWriter.ToString();
        }

        public void Dispose()
        {
            Console.SetOut(originalOutput);
            stringWriter.Dispose();
        }
    }
}
 */
