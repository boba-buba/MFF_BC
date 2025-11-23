using CountOrNotCount;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System.IO;
using System.Text;


namespace CountOrNotCount_UnitTests
{
    public class CountOrNotCountTests
    {
        [Fact]
        public void ZeroArguments()
        {
            // Arrange
            string[] MockCommandLine = new string[] {  };
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
        public void NonExistentFile()
        {
            // Arrange
            string[] MockCommandLine = new string[3] {"Hamlet.txt", "Out.txt", "cena" };
            var currentConsoleOut = Console.Out;

            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("File Error\n", consoleOutput.GetOuput());
            }

        }

        [Fact]
        public void PrettyTest()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "ZeleninaShortIn.txt", "ZeleninaShortOutActual.txt", "cena" };
            var currentConsoleOut = Console.Out;

            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("ZeleninaShortOut.txt", "ZeleninaShortOutActual.txt"));
            }
            
        }

        [Fact]
        public void OnlyHeader()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "OnlyHeaderIn.txt", "OnlyHeaderOutActual.txt", "cena" };
            var currentConsoleOut = Console.Out;

            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("OnlyHeaderOut.txt", "OnlyHeaderOutActual.txt"));
            }

        }
        
        [Fact]
        public void NonExistentColumn()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "NonExistentColumnIn.txt", "NonExistentColumnOutActual.txt", "uspora" };
            var currentConsoleOut = Console.Out;

            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("Non-existent Column Name\n", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("NonExistentColumnOut.txt", "NonExistentColumnOutActual.txt"));
            }
        }

        [Fact]
        public void NotEqualNumberOfItemsInARow()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "NotEqualNumberOfItemsInARowIn.txt", "NotEqualNumberOfItemsInARowOutActual.txt", "grape" };
            var currentConsoleOut = Console.Out;

            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("Invalid File Format\n", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("NotEqualNumberOfItemsInARowOut.txt", "NotEqualNumberOfItemsInARowOutActual.txt"));
            }
        }

        [Fact]
        public void EmptyWhiteLineInTable()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "EmptyWhiteLineInTableIn.txt", "EmptyWhiteLineInTableOutActual.txt", "grape" };
            var currentConsoleOut = Console.Out;

            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("Invalid File Format\n", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("EmptyWhiteLineInTableOut.txt", "EmptyWhiteLineInTableOutActual.txt"));
            }
        }

        [Fact]
        public void InvalidInteger()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "InvalidIntegerIn.txt", "InvalidIntegerOutActual.txt", "grape" };
            var currentConsoleOut = Console.Out;

            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("Invalid Integer Value\n", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("InvalidIntegerOut.txt", "InvalidIntegerOutActual.txt"));
            }

        }

        [Fact]
        public void InvalidIntegerOverflow()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "InvalidIntegerOverflowIn.txt", "InvalidIntegerOverflowOutActual.txt", "grape" };
            var currentConsoleOut = Console.Out;

            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("Invalid Integer Value\n", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("InvalidIntegerOverflowOut.txt", "InvalidIntegerOverflowOutActual.txt"));
            }

        }

        [Fact]
        public void SeveralErrorsMustReturnFirst()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "SeveralErrorsMustReturnFirstIn.txt", "SeveralErrorsMustReturnFirstOutActual.txt", "grape" };
            var currentConsoleOut = Console.Out;

            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("Invalid File Format\n", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("SeveralErrorsMustReturnFirstOut.txt", "SeveralErrorsMustReturnFirstOutActual.txt"));
            }
        }

        [Fact]
        public void TabSpaceSeparated()
        {
            // Arrange
            string[] MockCommandLine = new string[3] { "TabSpaceSeparatedIn.txt", "TabSpaceSeparatedOutActual.txt", "grape" };
            var currentConsoleOut = Console.Out;

            // Act  
            using (var consoleOutput = new ConsoleOutputManager())
            {
                ProgramManager.RunMainFunction(MockCommandLine);

                // Assert
                Assert.Equal("", consoleOutput.GetOuput());
                Assert.True(FileAssert.FileCompare("TabSpaceSeparatedOut.txt", "TabSpaceSeparatedOutActual.txt"));
            }
        }
    }
}