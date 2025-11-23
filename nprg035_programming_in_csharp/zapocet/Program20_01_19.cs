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

namespace zapocet20_01_19
{
    class ErrorException : Exception
    {
        public const string ErrorMessage = "ErrorMessage";
    }
    public class ProgramInputOutputState : IDisposable
    {
        public const string ArgumentErrorMessage = "Argument Error";
        public const string FileErrorMessage = "File Error";

        public StreamReader? Reader { get; private set; }
        //public StreamWriter? Writer { get; private set; }

        public bool InitializeFromCommandLineArgs(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: Program.exe file...");
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

    class Combination
    {
        private static readonly char[] colour = new char[] { 'c', 'd', 'h', 's' };
        private static readonly char[] PossibleNumber = new char[] { '2', '3', '4', '5', '6', '7', '8', '9', 'X', 'J', 'Q', 'K', 'A' };
        public enum PossibleCombinations { HighCard, OnePair, ThreeOfAKind, Straight, Flush, FourOfAKind, StraightFlush };

        public Dictionary<PossibleCombinations, List<string>> combs = new Dictionary<PossibleCombinations, List<string>>();
        public Dictionary<char, List<string>> numbers = new Dictionary<char, List<string>> { };
        private bool CheckValidColour(string card) => colour.Contains(card[0]);

        private bool CheckValidNumber(string card) => PossibleNumber.Contains(card[1]);
        private bool NSameNUmbers(string[] cards, int param, PossibleCombinations comb)
        {
            bool combinationExists = false;
            foreach (KeyValuePair<char, List<string>> entry in numbers) {
                if (entry.Value.Count >= param)
                {
                    combs[comb] = entry.Value;
                    combinationExists = true;
                }
            }
            return combinationExists;
        }
        private bool OnePair(string[] cards)
        {
            return NSameNUmbers(cards, 2, PossibleCombinations.OnePair);
        }
        private bool ThreeOfAKind(string[] cards)
        {
            return NSameNUmbers(cards, 3, PossibleCombinations.ThreeOfAKind);
        }
        private bool FourOfAKind(string[] cards)
        {
            return NSameNUmbers(cards, 4, PossibleCombinations.FourOfAKind);
        }

        private bool CheckRange(int index)
        {
            for (int i = index; i < index + 5; i++)
            {
                int currIndex = i % (PossibleNumber.Length);
                if (!(numbers.ContainsKey(PossibleNumber[currIndex]) && numbers[PossibleNumber[currIndex]].Count == 1))
                    return false;
            }
            return true;
        }
        private bool Straight(string[] cards)
        {
            bool CombinationsExists = false;
            for (int i = 0; i < PossibleNumber.Length; i++)
            {
                if (numbers.ContainsKey(PossibleNumber[i]) && numbers[PossibleNumber[i]].Count == 1)
                {
                    if (CheckRange(i)) 
                    { 
                        CombinationsExists = true;
                        break;
                    }
                }
            }
            if (CombinationsExists)
                combs[PossibleCombinations.Straight] = new List<string>(cards);
            return CombinationsExists;
        }

        private bool Flush(string[] cards)
        {
            if (CheckIfSameColour(cards))
            {
                combs[PossibleCombinations.Flush] = new List<string>(cards);
                return true;
            }
            return false;
        }


        private bool CheckIfSameColour(string[] cards)
        {
            char colour = cards[0][0];
            foreach (string card in cards)
            {
                if (card[0] != colour) return false;
            }
            return true;
        }
        private void FindSameNumbers(string[] cards)
        {
            foreach (string card in cards)
            {
                if (!numbers.ContainsKey(card[1]))
                {
                    numbers[card[1]] = new List<string>();
                }
                numbers[card[1]].Add(card);
            }
        }

        public KeyValuePair<PossibleCombinations, List<string>> 
            FindBestCombination(string[] cards)
        {
            FindSameNumbers(cards);

            
            if (OnePair(cards)) 
            {
                if (ThreeOfAKind(cards))
                {
                    FourOfAKind(cards);
                }
            }
            bool straight = Straight(cards);
            bool flush = Flush(cards);
            if (straight && flush)
            {
                combs[PossibleCombinations.StraightFlush] = new List<string>(cards);
            }

            var result = new KeyValuePair<PossibleCombinations, List<string>>(combs.Keys.Max(), combs[combs.Keys.Max()]);
            
            ClearMess();
            return result;
        }
        
        public int GetHighestCard(List<string> cards)
        {
            int HighestCard = -1;
            foreach (string card in cards)
            {
                int index = Array.IndexOf(PossibleNumber, card[1]);
                HighestCard = int.Max(index, HighestCard);
            }
            return HighestCard;
        }

        private void ClearMess()
        {
            combs.Clear();
            numbers.Clear();
        }
    }

    class RoundProcessor {

        private int _numberGamers = 0;
        private int _currentPlayer = 0;
        Combination _GamerComb = new Combination();
        private List<KeyValuePair<Combination.PossibleCombinations, List<string>>> _gamers = new List<KeyValuePair<Combination.PossibleCombinations, List<string>>>();
 
        private void SetNumberGamers(string num)
        {
            int result;
            if (!int.TryParse(num, out result))
            {
                throw new ErrorException();
            }
            _numberGamers = result;
            _currentPlayer = 0;
            _gamers.Clear();
        }
        public void ProcessLine(string[] tokens)
        {
            switch (tokens.Length) 
            {
                case 0:
                    AnalyzeResults();
                    break;
                case 1:
                    SetNumberGamers(tokens[0]);
                    break;
                default:
                    ProcessGamerCards(tokens);
                    break;
            }
        }

        private List<int> HighestCard(List<int> winners)
        {
            int HighestCardPerRound = -1;
            List<int> finalWinners = new List<int>();
            for (int i = 0; i < winners.Count; i++)
            {
                int highestCardPerGamer = _GamerComb.GetHighestCard(_gamers[winners[i]].Value);
                if (highestCardPerGamer >= HighestCardPerRound)
                {
                    HighestCardPerRound = highestCardPerGamer;
                    finalWinners.Add(i);
                }
            }
            return finalWinners;
        }
        public void AnalyzeResults() //must do
        {
            int MaxRank = -1;
            List<int> Winners = new List<int>();
            for (int i = 0; i < _numberGamers; i++)
            {
                if ((int)_gamers[i].Key >= MaxRank)
                {
                    Winners.Add(i);
                    MaxRank = (int)_gamers[i].Key;
                }
            }

            if (Winners.Count > 1) 
            {
                Winners = HighestCard(Winners);
            }
            PrintRoundResults(Winners);
        }

        private void ProcessGamerCards(string[] tokens)
        {
            _currentPlayer++;
            var result = _GamerComb.FindBestCombination(tokens);
            _gamers.Add(result);
        }

        private void PrintWinner()
        {
            Console.Write(" (WINNER)");
        }

        private void PrintRoundResults(List<int> winners)
        {

            for (int i = 0;i < _gamers.Count;i++) 
            {
                Console.Write("Player: " + (i + 1) + " " + _gamers[i].Key.ToString());

                foreach (var card in _gamers[i].Value)
                {
                    Console.Write(" " + card);
                }
                if (winners.Contains(i)) { PrintWinner(); }
                Console.WriteLine();

            }

        }

    }


    class Parser
    {
        private StreamReader _input;
        private RoundProcessor _processor = new RoundProcessor();
        char[] whiteChars = new char[] { ' ', '\t' };
        public Parser(StreamReader input)
        {
            _input = input;
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
                }
                _processor.AnalyzeResults();
            }
            catch (ErrorException)
            {
                Console.WriteLine(ErrorException.ErrorMessage);
            }
            

        }

    }
    internal class Program5
    {
        static void Main2(string[] args)
        {
            var state = new ProgramInputOutputState();
            if (!state.InitializeFromCommandLineArgs(args))
            {
                Console.Read();
                return;
            }
            Parser parser = new Parser(state.Reader);
            parser.ReadLines();

        }
    }
}
