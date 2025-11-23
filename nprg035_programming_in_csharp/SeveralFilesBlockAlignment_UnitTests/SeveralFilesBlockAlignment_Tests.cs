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