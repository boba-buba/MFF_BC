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

namespace zapocetPolyCalc
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



    class PolyCalculator 
    {
        private List<int> _currentPolynom = new List<int> { 0 }; 
        public PolyCalculator() { }

        private void PrintPolynom()
        {
            bool isZero = true;
            for (int i = 0; i < _currentPolynom.Count; i++) 
            {
                int koef = _currentPolynom[i];
                int power = i;
                if (koef != 0)
                {
                    isZero = false;
                    if (koef == -1 && power == 0) Console.Write("-1");
                    else if (koef == -1 && power != 0) Console.Write('-');
                    else if (koef == 1 && power == 0) Console.Write(koef);
                    else if (koef != 1) Console.Write(koef);

                    if (power != 0)
                    {
                        if (power == 1) Console.Write('x');
                        else Console.Write("x^" + power.ToString());
                    }
                    Console.Write(' ');
                }
            }
            if (isZero) { Console.Write(0); }
            Console.WriteLine();
        }
        private (int, int) ParseMember(string member)
        {
            int koef = 1;
            int power = 0;
            bool hasX = member.Contains('x');
            bool hasPower = member.Contains('^');
            if (hasX)
            {
                power = 1;
                var pair = member.Split('x', StringSplitOptions.RemoveEmptyEntries);
                if (pair.Length == 0) { koef = 1; power = 1; }
                else if (pair.Length == 1 && !hasPower) //5x
                {
                    if (pair[0] == "-") koef = -1;
                    else { if (!int.TryParse(pair[0], out koef)) { throw new ErrorException(); } }
                }
                else if (pair.Length == 1 && hasPower) //x^2
                {
                    if (!int.TryParse(pair[0].Substring(1), out power)) { throw new ErrorException(); }
                }
                else //5x^2
                {
                    if (pair[0] == "-") 
                        koef = -1;
                    else { if (!int.TryParse(pair[0], out koef)) { throw new ErrorException(); } }
                    if (!int.TryParse(pair[1].Substring(1), out power)) { throw new ErrorException(); }
                }
            }
            else //5
            {
                if (!int.TryParse(member, out koef)) { throw new ErrorException(); }
            }


            
            return (power, koef);
        }
        private void TryParsePolynom(string[] tokens, List<int> pol)
        {
            foreach (var token in tokens)
            {
                var pair = ParseMember(token);
                if (pol.Count < pair.Item1 + 1)
                {
                    int delta = pair.Item1 + 1 - pol.Count;
                    for (int i = 0; i < delta; i++)
                    {
                        pol.Add(0);
                    }
                }
                pol[pair.Item1] = pair.Item2;
            }
        }

        private void ReadPolynomToMem(string[] tokens)
        {
            var temp = new List<int> { 0 };
            TryParsePolynom(tokens, temp);
            _currentPolynom = temp;
            
        }
        private void SumPolynoms(List<int> first, List<int> second, char symbol)
        {
            for (int i = 0; i < first.Count; i++)
            {
                if (i == second.Count) second.Add(0);
                if (symbol == '-') second[i] -= first[i];
                else second[i] += first[i];
            }
        }
        private void AddPolynom(string[] tokens)
        {
            var polToAdd = new List<int>();
            
            TryParsePolynom(tokens.Skip(1).ToArray(), polToAdd);

            if (polToAdd.Count < _currentPolynom.Count)
            {
                SumPolynoms(polToAdd, _currentPolynom, '+');
            }
            else
            {
                SumPolynoms(_currentPolynom, polToAdd, '+');
                _currentPolynom = polToAdd;
            }
        }
        private void SubtractPolynom(string[] tokens)
        {
            var polToAdd = new List<int>();
            TryParsePolynom(tokens.Skip(1).ToArray(), polToAdd);
            SumPolynoms(polToAdd, _currentPolynom, '-');
        }
        private void AddZeros(List<int> pol, int count)
        {
            for (int i = 0; i < count; i++)  
                pol.Add(0);
        }
        
        private List<int> MultiplyPolynoms(List<int> first, List<int> second)
        {
            List<int> temp = new List<int>();
            AddZeros(temp, first.Count + second.Count - 1);

            for (int i = 0; i < first.Count; i++)
            {
                for (int j = 0; j < second.Count; j++)
                {
                    int power = i + j;
                    int koef = first[i] * second[j];
                    temp[power] += koef;
                }
            }
            return temp;
        }
        private void MultiplyPolynom(string[] tokens)
        {
            var polToMult = new List<int>();
            TryParsePolynom(tokens.Skip(1).ToArray(), polToMult);

            var newPol = new List<int>() {};
            AddZeros(newPol, _currentPolynom.Count + polToMult.Count - 1);

            for (int i = 0; i < polToMult.Count; i++)
            {
                for(int j = 0; j < _currentPolynom.Count; j++)
                {
                    int power = i + j;                    
                    int koef = _currentPolynom[j] * polToMult[i];
                    newPol[power] += koef;
                }
            }
            _currentPolynom = newPol;
        }
        private void EvaluatePolynom(string[] tokens)
        {
            double result = 0;
            int value;
            if (tokens.Length != 2) { throw new ErrorException(); }
            if (!int.TryParse(tokens[1], out value)) throw new ErrorException();
            for (int i = 0; i < _currentPolynom.Count; i++)
            {
                result += Math.Pow(value, i) * _currentPolynom[i];
            }
            Console.WriteLine(result);
        }
        private void DerivatePolynom()
        {
            _currentPolynom[0] = 0;
            for (int i = 1; i < _currentPolynom.Count; ++i)
            {
                _currentPolynom[i - 1] = i * _currentPolynom[i];
            }
            _currentPolynom[_currentPolynom.Count - 1] = 0;
        }

        private List<int> PolynomPower(List<int> polynom, int power)
        {
            List<int> result = new List<int> {0};
            AddZeros(result, polynom.Count-1 * power);

            if (power >= 1) { SumPolynoms(polynom, result, '+'); }

            for(int i = 1; i < power; ++i)
            {
                var temp = MultiplyPolynoms(result, polynom);
                result = temp;
            }
            return result;
        }
        private void SubstitutePolynom(string[] tokens)
        {
            var polToInsert = new List<int>();
            TryParsePolynom(tokens.Skip(1).ToArray(), polToInsert);

            List<int> result = new List<int>();
            AddZeros(result, (_currentPolynom.Count - 1) * (polToInsert.Count - 1) + 1);
            for (int i = 1; i < _currentPolynom.Count; i++)
            {
                int power = i;
                int koef = _currentPolynom[i];
                var temp = PolynomPower(polToInsert, power);
                var tempKoef = MultiplyPolynoms(temp, new List<int> { koef});
                SumPolynoms(tempKoef, result, '+');
            }
            result[0] += _currentPolynom[0];
            _currentPolynom = result;
        }
        public void ParseRequest(string[] tokens)
        {
            if (!tokens.Any()) { throw new ErrorException(); }
            switch (tokens[0])
            {
                case "+":
                    AddPolynom(tokens);
                    PrintPolynom();
                    break;
                case "-":
                    SubtractPolynom(tokens);
                    PrintPolynom();
                    break;
                case "*":
                    MultiplyPolynom(tokens);
                    PrintPolynom();
                    break;
                case "e":
                    EvaluatePolynom(tokens);
                    break;
                case "d":
                    DerivatePolynom();
                    PrintPolynom();
                    break;
                case "s":
                    SubstitutePolynom(tokens);
                    PrintPolynom();
                    break;
                default:
                    ReadPolynomToMem(tokens);
                    PrintPolynom();
                    break;
            }
        }
    }


    class Parser
    {
        private StreamReader _reader;
        private PolyCalculator _calc = new PolyCalculator();
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
                    _calc.ParseRequest(GetTokens(line));
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
        static void Main2(string[] args)
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
