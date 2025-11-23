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

namespace zapoce
{
    class ErrorException : Exception
    {
        public const string ErrorMessage = "Syntax Error";
    }
    public class ProgramInputOutputState : IDisposable
    {
        public const string ArgumentErrorMessage = "Argument Error";
        public const string FileErrorMessage = "File Error";

        public StreamReader? Reader { get; private set; }

        public bool InitializeFromCommandLineArgs(string[] args)
        {
            if (args.Length != 1)
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
            return true;
        }
        public void Dispose()
        {
            Reader?.Dispose();
        }
    }



    class Parser
    {
        private StreamReader _reader;
        public Parser(StreamReader reader)
        {
            _reader = reader;
        }
        private string[] GetTokens(string line)
        {
            char[] whitespaces = { ' ', '\t' };
            string[] tokens = line.Split(whitespaces, StringSplitOptions.RemoveEmptyEntries);
            return tokens;
        }

        public void ReadLines()
        {

            string line = _reader.ReadLine();
            while (line != null)
            {
                try
                {
                }
                catch (ErrorException)
                {
                    Console.WriteLine(ErrorException.ErrorMessage);
                }
                line = _reader.ReadLine();
            }

        }
    }
    internal class Program2
    {
        static void Main(string[] args)
        {

            ProgramInputOutputState state = new ProgramInputOutputState();
            if (!state.InitializeFromCommandLineArgs(args))
            {
                return;
            }
            Parser parser = new Parser(state.Reader);
            parser.ReadLines();

            

        }
    }
}
