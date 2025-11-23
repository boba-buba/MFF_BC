using System;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;
using System.IO.Enumeration;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Reflection.Metadata.Ecma335;


class ErrorException : Exception
{
    public const string ErrorMessage = "Error";
}
namespace zapocet24_01_19
{
    public class ProgramInputOutputState : IDisposable
    {
        public const string ArgumentErrorMessage = "Argument Error";
        public const string FileErrorMessage = "File Error";

        public StreamReader? Reader { get; private set; }
        public StreamWriter? Writer { get; private set; }
        public string FileName { get; private set; }

        public bool InitializeFromCommandLineArgs(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Program.exe file{.c||.cpp}");
                return false;
            }
            FileName = args[0];
            string[] fileName = args[0].Split('.');
            string[] formats = { "c", "cpp" };
            if (!formats.Contains(fileName[fileName.Length-1])) 
            {
                Console.WriteLine("Cant process the file");
                return false; 
            }
            string outputName = "";
            for (int i = 0; i < fileName.Length - 1; i++)
            {
                outputName += fileName[i]+ '.';
            }
            outputName += 'E';

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
            Writer = new StreamWriter(outputName);
            Console.WriteLine(outputName);
            return true;
        }
        public void Dispose()
        {
            Reader?.Dispose();
            Writer?.Dispose();
        }
    }

    class Preprocessor
    {       
        private StreamWriter _output;
        private string _inputFileName;
        private int _currentLine = 0;
        private int _ifCounter = 0;
        private bool ifElseOrEndifBlock = false;
        private bool elseEndifBlock = false;
        private bool rightBlockToRead = false;

        private Dictionary<string, string> _values = new Dictionary<string, string>();
        public Preprocessor(StreamWriter output, string inputFileName)
        {
            _output = output;
            _inputFileName = inputFileName;
        }

        public Preprocessor(StreamWriter output, string inputFileName, Dictionary<string, string> values)
        {
            _output = output;
            _inputFileName = inputFileName;
            _values = values;
        }

        private void DefineValue(string name, string value)
        {
            _values[name] = value;
        }

        private void UndefSymbol(string symbol)
        {
            if (_values.ContainsKey(symbol)) { _values.Remove(symbol); }
            Console.Write(_inputFileName);
        }

        private bool checkIDF(string name)
        {
            foreach (char c in name) 
            {
                if (!(Char.IsLetterOrDigit(c) || c == '_'))
                {
                    return false;
                }
            }
            return true;
        }

        private bool SymbolDefined(string name)
        {
            return _values.ContainsKey(name);
        }
        private void ProcessDefine(string[] tokens)
        {
            if (!checkIDF(tokens[1])) 
            {
                throw new ErrorException();
            }
            if (tokens.Length == 3) 
            {
                DefineValue(tokens[1], tokens[2]);
            }
            else if (tokens.Length == 2) 
            {
                DefineValue(tokens[1], "1");
            }
        }
        private void ProcessInclude(string[] tokens)
        {
            if (tokens.Length != 2) { throw new ErrorException(); }
            ProgramInputOutputState state = new ProgramInputOutputState();
            if (!state.InitializeFromCommandLineArgs(new string[]{ tokens[1] }))
            {
                throw new ErrorException();
            }
            Preprocessor pp = new Preprocessor(_output, state.FileName, _values);
            Parser parser = new Parser(state.Reader, pp);
            parser.ReadLines();
            state.Dispose();

        }
        private void ProcessUndef(string[] tokens)
        {
            if (tokens.Length != 2)
            {
                throw new ErrorException();
            }
            UndefSymbol(tokens[1]);
        }
        private void ProcessIfdef(string[] tokens)
        {
            _ifCounter++;
            if (tokens.Length != 2) 
            {
                throw new ErrorException();
            }
            bool defined = SymbolDefined(tokens[1]);
            if (defined) 
            {
                ifElseOrEndifBlock = true;
                rightBlockToRead = true;
            }
            else { elseEndifBlock = true; }
        }
        private void ProcessElse(string[] tokens)
        {
            if (ifElseOrEndifBlock) { rightBlockToRead = false; }
            else { rightBlockToRead = true; }
        }
        private void ProcessEndIf(string[] tokens)
        {
            _ifCounter--;
            rightBlockToRead = false;
            ifElseOrEndifBlock = false;
            elseEndifBlock = false;
        }

        private void ProcessCodeLine(string[] tokens)
        {
            if (!rightBlockToRead) { return; }
            if (tokens.Length < 2) { throw new ErrorException(); }
            switch (tokens[1])
            {
                case "__LINE__":
                    WriteDown(new string[]{ tokens[0], _currentLine.ToString()});
                    break;
                case "__FILE__":
                    WriteDown(new string[] { tokens[0], _inputFileName.ToString() });
                    break;
                default: 
                    if (SymbolDefined(tokens[1])) 
                    {
                        WriteDown(new string[] { tokens[0], _values[tokens[1]]});
                    }
                    else
                    {
                        WriteDown(new string[] { tokens[0], tokens[1] });
                    }
                    break;
            }
        }

        private void WriteDown(string[] tokens)
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                _output.Write(tokens[i]);
                if (i != tokens.Length - 1) { _output.Write(' '); }
            }
            _output.WriteLine();
        }
        public void ProcessLine(string[] tokens)
            {
                _currentLine++;
                switch (tokens[0])
                {
                    case "#define":
                        ProcessDefine(tokens);
                        break;
                    case "#include":
                        ProcessInclude(tokens);
                        break;
                    case "#undef":
                        ProcessUndef(tokens);
                        break;
                    case "#ifdef":
                        ProcessIfdef(tokens);
                        break;
                    case "#else":
                        ProcessElse(tokens);
                        break;
                    case "#endif":
                        ProcessEndIf(tokens);
                        break;
                    default:
                        ProcessCodeLine(tokens);
                        break;
                }
            }

        public int GetIfCounter() => _ifCounter;
    }

    class Parser
    {
        private StreamReader _input;
        Preprocessor _processor;
        char[] whiteChars = new char[] { ' ', '\t' };
        public Parser(StreamReader input, Preprocessor processor)
        {
            _input = input;
            _processor = processor;
        }
        private string[] GetTokens(string line)
        {
            string[] tokens = line.Split(whiteChars, StringSplitOptions.RemoveEmptyEntries);
            return tokens;
        }

        public void ReadLines()
        {
            try
            {
                string line = _input.ReadLine();
                while (line != null)
                {
                    _processor.ProcessLine(GetTokens(line));
                    line = _input.ReadLine();
                    if (line == "") line = _input.ReadLine();
                }
            } catch  (ErrorException) {
                Console.WriteLine(ErrorException.ErrorMessage);
            }
            if (_processor.GetIfCounter() != 0)
            {
                Console.WriteLine(ErrorException.ErrorMessage);
            }
            
        }

    }
    internal class Program2
    {
        static void Main3(string[] args)
        {
            var state = new ProgramInputOutputState();
            if (!state.InitializeFromCommandLineArgs(args))
            {
                return;
            }
            Preprocessor pp = new Preprocessor(state.Writer, state.FileName);
            Parser parser = new Parser(state.Reader, pp);
            parser.ReadLines();

            state.Dispose();
        }
    }
}
