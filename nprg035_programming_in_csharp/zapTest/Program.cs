using System;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;
using System.Numerics;
using System.Net.Mail;
using System.Timers;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace zapTest
{
    class ErrorException : Exception
    {
        public const string ErrorMessage = "File Error";
    }
    
    class InputFileProcessor
    {
        List<InputFile> files;
        string _fileName;
        string _outputDelim;
        int _currentLine = 0;
        int counterReadFiles = 0;
        StreamWriter _writer;
        public InputFileProcessor(List<InputFile> files, string outputFile, string outputDelim) 
        {
            this.files = files; _fileName = outputFile; _outputDelim = outputDelim;
        }
        private void ProcessLine(string line, InputFile f)
        {
            var tokens = line.Split(f._delimeter);
            if (f._fieldList == null || f._fieldList.Count == 0)
            {
                for (int i = 0; i < tokens.Length; i++)
                {
                    if (i != 0) Console.Write(_outputDelim);
                    Console.Write(tokens[i]);
                }
            }
            else
            {
                for (int i = 0; i < f._fieldList.Count; i++)
                {
                    if (f._fieldList[i] > tokens.Length) continue;
                    if (i != 0)
                        Console.Write(_outputDelim);
                    int index = f._fieldList[i] - 1;
                    Console.Write(tokens[index]);
                }
            }
        }

        private void LineFromAllFiles()
        {
            int counter = 0;
            foreach(var f in files)
            {
                counter++;

                if (f.stillCanRead && f.reader.Peek() > -1)
                {
                    ProcessLine(f.reader.ReadLine(), f);
                    if (counter != files.Count) Console.Write(_outputDelim);
                }
                else
                {
                    if (f.stillCanRead) 
                    { 
                        counterReadFiles++; 
                        f.stillCanRead = false;
                        f.reader.Close();
                    }
                }
            }
        }



 
        public void ProcessFiles()
        {
            if (_fileName != "")
            {
                _writer = new StreamWriter(_fileName);
                Console.SetOut(_writer);
            }
            
            while (counterReadFiles != files.Count)
            {
                if (_currentLine != 0) Console.WriteLine();
                
                LineFromAllFiles();
                _currentLine++;
            }  
            if (_writer != null) _writer.Dispose();
        }
    }

    class InputFile
    {
        public List<int> _fieldList = new List<int>();
        public string _delimeter = "";
        public string _inputFile = "";
        public bool stillCanRead = true;
        public StreamReader reader;

        public InputFile(List<int> fields, string delim, string inputFile) 
        {
            _fieldList = fields;
            _delimeter = delim;
            _inputFile = inputFile;
        }
    }

    class Parser
    {
        public List<InputFile> _files = new List<InputFile>();
        public Dictionary<string, InputFile>  NameFiles= new Dictionary<string, InputFile>{};
        private string[] _tokens;
        public Parser(string[] tokens){ _tokens = tokens; }

        List<int> _fieldList;
        private string _delimeter = "\t";
        public string _outputDelim = "\t";
        public string _outPutFile = "";
        private string _inputFile = "";
        
        private void ParseRange(string range, List<int> list)
        {
            var fields = range.Split('-');
            if (fields.Length != 2) { throw new ErrorException(); }
            int first = int.Parse(fields[0]);
            int second = int.Parse(fields[1]);
            if (first >= second)
            {
                for (int i = first; i >= second; i--) 
                    list.Add(i);
            }
            else for (int i = first; i <= second; i++) 
                    list.Add(i);
        }
        private void ParseFieldList(string list)
        {
            List<int> fields = new List<int>();
            var fieldsSeparated = list.Split(',');
            foreach (var field in fieldsSeparated)
            {
                int fieldNumber;
                if (!int.TryParse(field, out fieldNumber))
                {
                    ParseRange(field, fields);
                }
                else fields.Add(fieldNumber);
            }
            _fieldList = fields;
        }
        private void ParseDelimeter(string delimeter)
        {
           _delimeter = delimeter;
        }
        private void ParseOutputDelimeter(string outputDelimeter)
        {
            if (outputDelimeter[0] == '\'')
            {
                _outputDelim = outputDelimeter.Substring(1, outputDelimeter.Length-2);
            }
            else 
            _outputDelim = outputDelimeter;
        }
        private void ParseOutFile(string outFile)
        {
            _outPutFile = outFile;
        }
        private void TryParseInputFile(string input)
        {
            _inputFile = input;
        }
        private void AddInputFile()
        {
            try
            {
                InputFile f = new InputFile(_fieldList, _delimeter, _inputFile);
                f.reader = new StreamReader(f._inputFile);
                _files.Add(f);

            }
            catch (IOException)
            {
                Console.WriteLine("File Error");
                return;
            }
        }
        private void CleanAll()
        {
            _fieldList = new List<int>{ };
            _inputFile = "";
            _delimeter = "\t";
        }
        public void ParseRequest()
        {
            int i = 0;
            while (i < _tokens.Length) 
            {
                switch (_tokens[i])
                {
                    case "-f":
                        i++;
                        ParseFieldList(_tokens[i]);
                        break;
                    case "-d":
                        i++;
                        ParseDelimeter(_tokens[i]);
                        break;
                    case "--od":
                        i++;
                        ParseOutputDelimeter(_tokens[i]);
                        break;
                    case "--out":
                    case ">":
                        i++;
                        ParseOutFile(_tokens[i]);
                        break;
                    default:
                        TryParseInputFile(_tokens[i]);
                        AddInputFile();
                        CleanAll();
                        break;
                }
                i++;
            }

        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Parser parser = new Parser(args);
            parser.ParseRequest();
            InputFileProcessor pr = new InputFileProcessor(parser._files, parser._outPutFile, parser._outputDelim);
            pr.ProcessFiles();

        }
    }
}
