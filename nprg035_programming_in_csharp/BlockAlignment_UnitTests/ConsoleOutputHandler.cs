/*using System;

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
}*/