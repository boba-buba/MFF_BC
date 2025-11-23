using System;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Data;

namespace zapocetHistoryStats
{   
    public class ProgramInputOutputState : IDisposable
    {
        public const string ArgumentErrorMessage = "Argument Error";
        public const string FileErrorMessage = "Could not find file \'";

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
                Console.Write(FileErrorMessage + args[0] + "\'\n");
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                Console.Write(FileErrorMessage + args[0] + "\'\n");
                return false;
            }
            catch (ArgumentException)
            {
                Console.Write(FileErrorMessage + args[0] + "\'\n");
                return false;
            }
            return true;
        }
        public void Dispose()
        {
            Reader?.Dispose();
        }
    }

    class UserStat
    {
        public string Name { get; private set; }
        public int OverallMessagesNumber => ReceivedMessagesNumber + SentMessagesNumber;
        public UserStat(string name)
        {
            Name = name;
        }
        public int ReceivedMessagesNumber { get; private set; }
        public int SentMessagesNumber { get; private set; }
        public int WordsNumber { get; private set; }

        private Dictionary<string, int> _words = new Dictionary<string, int>();
        private int[] _histogram = new int[24];
        private Dictionary<DateTime, int> _weekHistogram = new Dictionary<DateTime, int>();
        public int[] GetHistogram() => _histogram;
        public Dictionary<DateTime, int> GetWeekHist() => _weekHistogram;
        public void AddToWeekHistogram(DateTime date)
        {
            bool inserted = false;

            foreach (var item in _weekHistogram.OrderBy(item => item.Key))
            {
                TimeSpan delta = date - item.Key;

                if (delta.Days < 7)
                {
                    inserted = true;
                    _weekHistogram[item.Key]++;
                    return;
                }
            }
            if (!inserted || _weekHistogram.Count == 0)
            {
                _weekHistogram[date] = 1;
            }
        }
        public void AddWord(string word)
        {
            if (_words.ContainsKey(word))
            {
                _words[word]++;
            }
            else
            {
                _words.Add(word, 1);
            }
        }
        public void AddToHistogram(int hour, int value) => _histogram[hour] += value;
        public void AddReceivedMessages() => ReceivedMessagesNumber++;
        public void AddSentMessages() => SentMessagesNumber++;
        public void AddWordsNumber(int number) => WordsNumber+= number;
        public string GetNMostPopularWords(int n)
        {
            string result = "";
            int counter = 0;
            foreach(var word in _words.OrderByDescending(word => word.Value))
            {

                    
                    result+= word.Key;
                    counter++;
                    if (counter != n) result += ", ";
                    else break;
                    
                
            }
            return result;
        }
        public void TryAddNewWeek(DateTime date)
        {
            bool alreadyHere = false;
            foreach (var item in _weekHistogram)
            {
                TimeSpan delta = date - item.Key;

                if (delta.Days < 7)
                {
                    alreadyHere = true;
                    break;
                }
            }
            if (!alreadyHere) { _weekHistogram[date] = 0; }

        }

    }
    class DataStorage
    {
        public Dictionary<string, UserStat> users = new Dictionary<string, UserStat>{ { "TOTAL", new UserStat("TOTAL")} };
        private string[] GetTokens(string line)
        {
            char[] whitespaces = { ' ', '\t', ',' };
            string[] tokens = line.Split(whitespaces, StringSplitOptions.RemoveEmptyEntries);
            return tokens;
        }
        private void ProcessNameDate(UserStat user, string nameDate)
        {
            var tokens = GetTokens(nameDate);
            if (tokens[0] == "Me") 
            { user.AddReceivedMessages();}
            else { user.AddSentMessages(); }

            DateTime dateTime = DateTime.Parse(tokens[2]);
            user.AddToHistogram(dateTime.Hour, 1);
            foreach(var u in users) 
            {
                u.Value.TryAddNewWeek(DateTime.Parse(tokens[1]));
            }
            user.AddToWeekHistogram(DateTime.Parse(tokens[1]));
        }
        private void ProcessText(UserStat user, string text)
        {
            var whitespaces = new char[] { ' ', '.', ',', '!', '?', ';' };
            string[] words = text.Split(whitespaces, StringSplitOptions.RemoveEmptyEntries);
            user.AddWordsNumber(words.Length);

            foreach (string word in words)
            {
                user.AddWord(word.ToLower());
            }

        }
        public void ProcessUser(StreamReader reader)
        {
            bool alreadyExists = false;
            string line = reader.ReadLine();
            UserStat User;
            if (users.ContainsKey(line))
            {
                User = users[line];
            }
            else User = new UserStat(line);

            line = reader.ReadLine();
            while (line != "---" && reader.Peek() > -1)
            {
                ProcessNameDate(User, line);
                ProcessNameDate(users["TOTAL"], line);
                line = reader.ReadLine();
                ProcessText(User, line);
                ProcessText(users["TOTAL"], line);
                line = reader.ReadLine();
            }
            if (!alreadyExists) users[User.Name] = User;
        }
    }

    class PrintingResults
    {
        StreamWriter _writer = new StreamWriter("stats.html");
        DataStorage _storage;
        public PrintingResults(DataStorage ds)
        {
            _storage = ds;
        }
        private void PrintHistogram(UserStat user)
        {
            var hist = user.GetHistogram();
            int maximum = hist.Max();


            _writer.WriteLine("<div class=\"barContainer\">");
            foreach(var val in hist)
            {
                var histValue = val * 60 / maximum;
                _writer.WriteLine("\t<div class=\"hoursContainer\"><div class=\"hoursBar\" style=\"height:" + histValue.ToString() + "px;\"></div></div>");
                _writer.WriteLine();
            }
            _writer.WriteLine("</div>");
        }
        private void PrintWeekHistogram(UserStat user)
        {
            var hist = user.GetWeekHist();
            int maximum = hist.OrderByDescending(x => x.Value).First().Value;


            _writer.WriteLine("<td><div class=\"barContainer\">");
            foreach (var val in hist.OrderBy(val => val.Key))
            {
                var histValue = val.Value * 60 / maximum;
                _writer.WriteLine("\t<div class=\"timelineContainer\"><div class=\"graphBar\" style=\"height:" + histValue.ToString() + "px;\"></div></div>");
            }
            _writer.WriteLine("</div></td>");

        }
        private void PrintUser(UserStat user, int rank)
        {
            _writer.WriteLine("<tr>");
            _writer.WriteLine("<td>" + rank.ToString() + "</td>");
            _writer.WriteLine("<td>" + user.Name + "</td>");
            _writer.WriteLine("<td>" + user.OverallMessagesNumber + "</td>");
            double a = user.ReceivedMessagesNumber;
            double b = user.OverallMessagesNumber;
            double visualizationRecieved = Math.Round((a/b) * 100);
            _writer.WriteLine("<td>" + "<div class=\"outDiv\"><div class=\"inDiv\" style=\"width:"+ a/b + "%;\">"+visualizationRecieved+"%</div></div>" + "</td>");
            _writer.WriteLine("<td>" + user.WordsNumber + "</td>");
            string words = user.GetNMostPopularWords(5);
            _writer.WriteLine("<td>" + words + "</td>");

            PrintHistogram(user);
            PrintWeekHistogram(user);
            

            _writer.WriteLine("</tr>");
        }
        

        public void PrintingUsersResults()
        {
            int currentRank = _storage.users.Count;
            foreach (var user in _storage.users.OrderByDescending(user => user.Value.OverallMessagesNumber))
            {
                if (user.Key == "TOTAL") continue;
                PrintUser(user.Value, currentRank);
                currentRank--;
                _writer.WriteLine();
            }
            PrintUser(_storage.users["TOTAL"], 0);
            _writer.Close();
        }
    }
    class Parser
    {
        private StreamReader _reader;
        private DataStorage _da;
        
        public Parser(StreamReader reader, DataStorage da)
        {
            _reader = reader;
            _da = da;
        }

        public void ReadLines()
        {
            while (_reader.Peek() > -1)
            {
                _da.ProcessUser(_reader);
            }
            Console.Write("all");
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
            DataStorage da = new DataStorage();
            Parser parser = new Parser(state.Reader, da);
            parser.ReadLines();
            PrintingResults pr = new PrintingResults(da);
            pr.PrintingUsersResults();
            

        }
    }
}
