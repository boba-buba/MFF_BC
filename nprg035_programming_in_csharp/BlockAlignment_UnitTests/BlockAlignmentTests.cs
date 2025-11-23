/*namespace BlockAlignment_UnitTests
{
    public class BlockAlignmentTests
    {
        [Fact]
        public void ArgumentErrorLessParamaters()
        {
            // Arrange
            string[] MockCommandLine = new string[] { };
            var currentConsoleOut = Console.Out;
            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("Argument Error\n", consoleOutput.GetOuput());
            }

        }

        [Fact]
        public void ArgumentErrorThirdParameter()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "PrettyTextFromTaskIn.txt", "PrettyTextFromTaskOutActual.txt", "yu" };
            var currentConsoleOut = Console.Out;
            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("Argument Error\n", consoleOutput.GetOuput());
            }
        }

        [Fact]
        public void PrettyFromTask()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "PrettyTextFromTaskIn.txt", "PrettyTextFromTaskOutActual.txt", "17" };
            var currentConsoleOut = Console.Out;
            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("PrettyTextFromTaskOut.txt", "PrettyTextFromTaskOutActual.txt"));
            }

        }

        [Fact]
        public void SeveralParagraphs()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "SeveralParagraphsIn.txt", "SeveralParagraphsOutActual.txt", "19" };
            var currentConsoleOut = Console.Out;
            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("SeveralParagraphsOut.txt", "SeveralParagraphsOutActual.txt"));
            }
        }

        [Fact]
        public void SeveralEmptyLinesBetweenParagraphs()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "SeveralEmptyLinesBetweenParagraphsIn.txt", "SeveralEmptyLinesBetweenParagraphsOutActual.txt", "19" };
            var currentConsoleOut = Console.Out;
            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("SeveralEmptyLinesBetweenParagraphsOut.txt", "SeveralEmptyLinesBetweenParagraphsOutActual.txt"));
            }
        }

        [Fact]
        public void TooLongWordForLine()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "TooLongWordForLineIn.txt", "TooLongWordForLineOutActual.txt", "10" };
            var currentConsoleOut = Console.Out;
            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("TooLongWordForLineOut.txt", "TooLongWordForLineOutActual.txt"));
            }
        }

        [Fact]
        public void OnlyOneWord()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "OnlyOneWordIn.txt", "OnlyOneWordOutActual.txt", "10" };
            var currentConsoleOut = Console.Out;
            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("OnlyOneWordOut.txt", "OnlyOneWordOutActual.txt"));
            }
        }

        [Fact]
        public void EmptyLinesAfterLastWord()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "EmptyLinesAfterLastWordIn.txt", "EmptyLinesAfterLastWordOutActual.txt", "19" };
            var currentConsoleOut = Console.Out;
            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("EmptyLinesAfterLastWordOut.txt", "EmptyLinesAfterLastWordOutActual.txt"));
            }
        }

        [Fact]
        public void WhiteCharsAfterLastWord()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "WhiteCharsAfterLastWordIn.txt", "WhiteCharsAfterLastWordOutActual.txt", "19" };
            var currentConsoleOut = Console.Out;
            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("WhiteCharsAfterLastWordOut.txt", "WhiteCharsAfterLastWordOutActual.txt"));
            }
        }

        [Fact]
        public void EmptyLinesBeforeFirstWord()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "EmptyLinesBeforeFirstWordIn.txt", "EmptyLinesBeforeFirstWordOutActual.txt", "19" };
            var currentConsoleOut = Console.Out;
            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("EmptyLinesBeforeFirstWordOut.txt", "EmptyLinesBeforeFirstWordOutActual.txt"));
            }
        }

        [Fact]
        public void BigText()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "BigTextIn.txt", "BigTextOutActual.txt", "45" };
            var currentConsoleOut = Console.Out;
            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("BigTextOut.txt", "BigTextOutActual.txt"));
            }
        }

    }
}
*/