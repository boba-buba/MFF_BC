using System;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;

namespace zapocet18_01_24
{
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
    class RegisterMachine
    {
        public const string ErrorMessage = "Errenous halt!";
        private Dictionary<string, int> _instructionNameParams = new Dictionary<string, int>{ { "halt", 0 }, { "dec", 3 }, { "inc", 2 } };
        private List<int> _registers = new List<int>();

        public void LoadRegisters(List<int> registers)
        {
            _registers = registers;
        }

        private bool CheckNumberParams(string[] tokens)
        {
            if (tokens.Length - 1 != _instructionNameParams[tokens[0]])
                return false;
            return true;
        }
        private int RunIncInstruction(string[] tokens)
        {
            int regNumber;
            if (!int.TryParse(tokens[1], out regNumber)) {
                throw new Exception(ErrorMessage);
            }
            _registers[regNumber]++;
            int nextInstr;
            if (!int.TryParse(tokens[2], out nextInstr))
            {
                throw new Exception(ErrorMessage);
            }
            return nextInstr;
        }

        private int RunDecInstruction(string[] tokens)
        {
            int regNumber;
            if (!int.TryParse(tokens[1], out regNumber))
            {
                throw new Exception(ErrorMessage);
            }
            string nextInstrString = tokens[3];
            if (_registers[regNumber] > 0)
            {
                _registers[regNumber]--;
                nextInstrString = tokens[2];

            }
            int nextInstr;
            if (!int.TryParse(nextInstrString, out nextInstr))
            {
                throw new Exception(ErrorMessage);
            }
            return nextInstr;

        }

        public int ParseLine(string[] tokens)
        {
            if (!CheckNumberParams(tokens))
            {
                throw new Exception(ErrorMessage);
            }
            switch (tokens[0]) {
                case "halt":
                    return 0;
                case "inc":
                    return RunIncInstruction(tokens);
                case "dec":
                    return RunDecInstruction(tokens);
                default: 
                    throw new Exception(ErrorMessage);
            }
        }

        public void PrintRegisters()
        {
            foreach (int i in _registers) 
            { 
                Console.WriteLine(i);

            }
        }
    }
    class Processor
    {

        public const string ErrorMessage = "Errenous halt!";

        private StreamReader _reader; 
        private List<string> _instructions = new List<string> {"zero instr"};
        private RegisterMachine _regMachine;
        private int _currentLine = 1;

        public Processor(StreamReader reader)
        {
            _reader = reader;
            _regMachine = new RegisterMachine();
        }

        public string[] GetTokens(string line)
        {
            string[] tokens = line.Split(' ');
            return tokens;
        }

        private void LoadRegisters(string line) 
        {
            List<int> regs = new List<int>();
            var tokens = GetTokens(line);
            foreach (string token in tokens)
            {
                int result;
                if (!int.TryParse(token, out result))
                {
                    throw new Exception(ErrorMessage);
                }

                regs.Add(result);
            }
            _regMachine.LoadRegisters(regs);
        }
        private void LoadInstructions()
        {
            var line = _reader.ReadLine();
            
            while (line != null)
            {
                _instructions.Add(line);
                line = _reader.ReadLine();
            }
        }

        private bool LoadProgram()
        {
            try
            {
                var firstLine = _reader.ReadLine();
                if (firstLine != null) { LoadRegisters(firstLine); }
                else return false;
                LoadInstructions();
                
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        private void RunProgram()
        {
            while(_currentLine != 0)
            {
                Console.WriteLine("Current instr: " + _currentLine);
                _regMachine.PrintRegisters();
                Console.WriteLine();


                _currentLine = _regMachine.ParseLine(GetTokens(_instructions[_currentLine]));
                if (_currentLine > _instructions.Count - 1 || _currentLine < 0)
                {
                    Console.WriteLine(ErrorMessage);
                    return;
                }
            }
            _regMachine.PrintRegisters();
        }

        public void ExecuteProgram()
        {
            if (!LoadProgram()) return;
            RunProgram();
        }


    }
    internal class Program2
    {
        static void Main2(string[] args)
        {

            ProgramInputOutputState state = new ProgramInputOutputState();
            if (!state.InitializeFromCommandLineArgs(args))
            {
                return;
            }

            Processor processor = new Processor(state.Reader);
            processor.ExecuteProgram();

        }
    }
}
