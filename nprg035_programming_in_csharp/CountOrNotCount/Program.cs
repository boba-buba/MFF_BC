using System;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;
using System.Xml.Linq;

namespace CountOrNotCount
{
    /// <summary>
    /// Code was taken from lab materials of course NPRG035, MFF UK 2023.
    /// And slightky changed.
    /// </summary>
    public class ProgramInputOutputState : IDisposable
    {
        public const string ArgumentErrorMessage = "Argument Error";
        public const string FileErrorMessage = "File Error";

        public StreamReader? Reader { get; private set; }
        public StreamWriter? Writer { get; private set; }

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

            return true;
        }

        public void Dispose()
        {
            Reader?.Dispose();
            Writer?.Dispose();
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            const string FileError = "File Error";
            var state = new ProgramInputOutputState();
            if (!state.InitializeFromCommandLineArgs(args))
            {
                return;
            }
            try
            {
                string columnName = args[2];

                WordProcessor fInfo = new WordProcessor(columnName);
                FileInputOutput fIO = new FileInputOutput(state.Reader!, state.Writer!, fInfo);

                fIO.ParseFile();
                fIO.WriteReport();
                state.Dispose();
            }
            catch (IOException)
            {
                Console.WriteLine(FileError);
                
            }
            catch (UnauthorizedAccessException)

            {
                Console.WriteLine(FileError);
            }
            catch (ArgumentException)
            {
                Console.WriteLine(FileError);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            state.Dispose();
        }
    }

    public class WordProcessor
    {
        public const string ColumnError = "Non-existent Column Name";
        public const string FileFormatError = "Invalid File Format";
        public const string InvalidIntegerError = "Invalid Integer Value";

        int _columnNumber = -1;
        int _columns = 0;
        public string columnName { get; private set; }
        public long ItemsSum { get; private set; }
        public WordProcessor(string colName)
        {
            columnName = colName;
            ItemsSum = 0;
        }

        void AddToSum(int item)
        {
            ItemsSum += item;
        }

        public void ParseHeader(string[] line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == columnName && _columnNumber == -1)
                {
                    _columnNumber  = i;
                }
            }
            if (_columnNumber == -1)
            {
                throw new Exception(ColumnError);
            }
            _columns = line.Count();
        }

        public void ParseLine(string[] line)
        {
            int num = 0;
            if (line.Count() != _columns)
            {
                throw new Exception(FileFormatError);
            }
            for (int i = 0; i < line.Length; i++)
            {
                if (_columnNumber == i)
                {
                    try
                    {
                        num = Convert.ToInt32(line[i]);
                        AddToSum(num);
                    }
                    catch (Exception)
                    {
                        throw new Exception(InvalidIntegerError);
                    }
                }
            }
        }
    }

    public class FileInputOutput
    {
        StreamReader _fr;
        StreamWriter _fw;
        WordProcessor info;

        public FileInputOutput(StreamReader input, StreamWriter output, WordProcessor fi)
        {
            _fr = input;
            _fw = output;
            info = fi;
        }

        public void ParseFile()
        {    
            string file_line = _fr.ReadLine();
            if (file_line == null)
            {
                throw new Exception(WordProcessor.FileFormatError);
            }
  
            char[] whitechars = new char[] { ' ', '\t' };

            string[] line = file_line.Split(whitechars, StringSplitOptions.RemoveEmptyEntries);

            if (line.Count() == 0)
            {
                throw new Exception(WordProcessor.FileFormatError);
            }
            try
            {
                info.ParseHeader(line);

                while ((file_line = _fr.ReadLine()) != null)
                {
                    line = file_line.Split(whitechars, StringSplitOptions.RemoveEmptyEntries);          
                    info.ParseLine(line);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        void WriteChar(char ch)
        {
            _fw?.Write(ch);
        }

        void WriteString(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                WriteChar(text[i]);
            }
        }

        public void WriteReport()
        {
            WriteString(info.columnName + '\n');

            for (int i = 0; i < info.columnName.Length; i++)
            {
                WriteChar('-');
            }
            WriteChar('\n');
            if (info.ItemsSum > 1000000000000)
            {
                WriteString(1000000000000.ToString());
            }
            else if (info.ItemsSum < -1000000000000)
                WriteString(1000000000000.ToString());
            else
                WriteString(info.ItemsSum.ToString());
            WriteChar('\n');
        }
    }

    /// <summary>
    /// Created for test purposes only. To simulate Main() function
    /// </summary>
    public static class ProgramManager
    {
        public static void RunMainFunction(string[] args)
        {
            const string FileError = "File Error";
            var state = new ProgramInputOutputState();
            if (!state.InitializeFromCommandLineArgs(args))
            {
                return;
            }
            try
            {
                string columnName = args[2];

                WordProcessor fInfo = new WordProcessor(columnName);
                FileInputOutput fIO = new FileInputOutput(state.Reader!, state.Writer!, fInfo);

                fIO.ParseFile();
                fIO.WriteReport();
                state.Dispose();
            }
            catch (IOException)
            {
                Console.Write(FileError + '\n');

            }
            catch (UnauthorizedAccessException)

            {
                Console.Write(FileError + '\n');
            }
            catch (ArgumentException)
            {
                Console.Write(FileError + '\n');
            }
            catch (Exception e)
            {
                Console.Write(e.Message + '\n');
            }
            state.Dispose();
        }
    }
}