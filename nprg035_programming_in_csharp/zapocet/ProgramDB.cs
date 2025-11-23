using System;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace zapocetDB
{
    abstract class ErrorException : Exception
    {
        public const string ErrorMessage = "Error Message";
    }

    class TableExistsException : ErrorException
    {
        public new const string ErrorMessage = "Table Already Exists";
    }

    class NoColumnException : ErrorException
    {
        public new const string ErrorMessage = "No Such Column";
    }

    class NoTableExcpetion : ErrorException
    {
        public new const string ErrorMessage = "No Such Table";
    }
    
    class WrongNumberOfColumnsException : ErrorException
    {
        public new const string ErrorMessage = "Wrong Number Of Columns";
    }

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


    class Row 
    {
        private List<string> _row;
        public int Length => _row.Count;
        public Row(List<string> row) {  _row = row; }

        public void InsertValue(string value)
        {
            _row.Add(value);
        }

        public void UpdateValue(int columnNumber, string value)
        {
            _row[columnNumber] = value;
        }

        public void PrintRowColumns(List<int> columns)
        {
            foreach (var column in columns)
            {
                Console.Write(_row[column].ToString());
                Console.Write(' ');
            }
            Console.WriteLine();
            
        }

        private bool ConditionIsTrue(int first, int second, string condition)
        {
            switch (condition)
            {
                case "<": return first < second;
                case ">": return first > second;
                case "=": return first == second;
                default: return false;
            }
        }

        public void UpdateRowValues(int columnI, int columnJ, string newVal, string symbol, string conditionVal)
        {
            int columnVal = int.Parse(_row[columnJ]);
            int condVal = int.Parse(conditionVal);

            if (ConditionIsTrue(columnVal, condVal, symbol))
            {
                _row[columnI] = newVal;
            }
        }

        public string GetColumnValues()
        {
            string values = "";
            foreach(string val in _row)
            {
                values += ' ' + val;
            }
            return values;
        }
    }

    class Header
    {
        private List<string> _header;
        public int Length => _header.Count;
        public Header(List<string> header) { _header = header; }
        public int GetColumnIndex(string columnName)
        {
            return _header.IndexOf(columnName);
        }

        public string GetColumnNames()
        {
            string columns = "";
            foreach(string columnName in _header)
            {
                columns += ' ' + columnName;
            }
            return columns;
        }

        public void PrintColumnNames(List<int> columns)
        {
            for (int i = 0; i < columns.Count; i++) 
            {
                Console.Write(_header[columns[i]]);
                if (i != columns.Count - 1) Console.Write(',');
            }
            Console.WriteLine();
        }
    }

    class Table
    {
        private Header _header;
        private List<Row> _rows = new List<Row>();
        public Table(List<string> header)
        {
            _header = new Header(header);
        }

        public void PrintHeader()
        {
            Console.WriteLine(_header.GetColumnNames());
        }
        public void PrintColumns(List<int> columnsId)
        {
            _header.PrintColumnNames(columnsId);
            foreach (var row in _rows)
            {
                row.PrintRowColumns(columnsId);
            }
        }
        public int GetColumnIndex(string columnName) => _header.GetColumnIndex(columnName);

        public void InsertNewRow(List<string> vals)
        {
            if (vals.Count != _header.Length) throw new WrongNumberOfColumnsException();
            var row = new Row(vals);
            _rows.Add(row);
        } 

        public void UpdateValues(int columnI, int columnJ, string newVal, string symbol, string conditionVal)
        {
            foreach(var row in _rows)
            {
                row.UpdateRowValues(columnI, columnJ, newVal, symbol, conditionVal);
            }
        }

        public void SaveTable(string tableName, StreamWriter OutPutFile)
        {
            string createTable = "CREATE_TABLE " + tableName + _header.GetColumnNames();
            OutPutFile.WriteLine(createTable);
            foreach(var row in _rows)
            {
                string insertInto = "INSERT_INTO " + tableName + row.GetColumnValues();
                OutPutFile.WriteLine(insertInto);
            }
        }

    }

    class DataBase 
    {
        private Dictionary<string, Table> _tablesByName = new Dictionary<string, Table>();

        private bool TableAlreadyExists(string tableName)
        {
            return _tablesByName.ContainsKey(tableName);
        }
        private void CreateTable(string[] tokens)
        {
            if (TableAlreadyExists(tokens[1])) { Console.WriteLine("Invalid table name \'" + tokens[1] + "\'"); throw new TableExistsException(); }

            string tableName = tokens[1];
            var header = tokens.Skip(2).ToList();
            _tablesByName[tableName] = new Table(header);
        }

        private void SelectFrom(string[] tokens)
        {
            if (!TableAlreadyExists(tokens[1])) { Console.WriteLine("Invalid table name \'" + tokens[1] + "\'"); throw new NoTableExcpetion(); }
            var table = _tablesByName[tokens[1]];
            var columns = new List<int>();
            for (int i = 2; i < tokens.Length; i++)
            {
                int columnIndex = table.GetColumnIndex(tokens[i]);
                if (columnIndex == -1) { Console.WriteLine("Invalid column name \'" + tokens[i] + "\'"); throw new NoColumnException(); }
                columns.Add(columnIndex);
            }
            table.PrintColumns(columns);
        }
        private void InsertInto(string[] tokens)
        {
            if (!TableAlreadyExists(tokens[1])) { Console.WriteLine("Invalid table name \'" + tokens[1] + "\'"); throw new NoTableExcpetion(); }
            var table = _tablesByName[tokens[1]];
            table.InsertNewRow(tokens.Skip(2).ToList());
        }

        private void Update(string[] tokens)
        {
            if (!TableAlreadyExists(tokens[1])) { Console.WriteLine("Invalid table name \'" + tokens[1] + "\'"); throw new NoTableExcpetion(); }
            var table = _tablesByName[tokens[1]];
            int columnI = table.GetColumnIndex(tokens[2]);
            int columnJ = table.GetColumnIndex(tokens[4]);
            if (columnI == -1) 
            {
                Console.WriteLine("Invalid column name \'" + tokens[2] + "\'");
                throw new NoColumnException(); 
            }
            if (columnJ == -1)
            {
                Console.WriteLine("Invalid column name \'" + tokens[4] + "\'");
                throw new NoColumnException();

            }


            string newValue = tokens[3];
            string symbol = tokens[5];
            string conditionValue = tokens[6];

            table.UpdateValues(columnI, columnJ, newValue, symbol, conditionValue);

        }

        private void LoadFile(string[] tokens)
        {
            ProgramInputOutputState state = new ProgramInputOutputState();
            if (!state.InitializeFromCommandLineArgs(new string[] {tokens[1]}))
            {
                return;
            }
            Parser parser = new Parser(state.Reader, this);
            parser.ReadLines();
        }

        private void SaveFile(string[] tokens) 
        { 
            StreamWriter outFile = new StreamWriter(tokens[1]);
            foreach(var table in _tablesByName)
            {
                table.Value.SaveTable(table.Key, outFile);
            }
            outFile.Close();
        }
        
        public void ProcessRequest(string[] tokens)
        {
            if (tokens.Length == 0) { return; }
            switch (tokens[0])
            {
                case "CREATE_TABLE":
                    CreateTable(tokens);
                    break;
                case "SELECT_FROM":
                    SelectFrom(tokens);
                    break;
                case "INSERT_INTO":
                    InsertInto(tokens);
                    break;
                case "UPDATE":
                    Update(tokens);
                    break;
                case "LOAD":
                    LoadFile(tokens);
                    break;
                case "SAVE":
                    SaveFile(tokens);
                    break;
                default: break;

            }
        }
    
    }

    class Parser
    {
        private StreamReader _reader;
        private DataBase _db;
        
        public Parser(StreamReader reader, DataBase db)
        {
            _reader = reader;
            _db = db;
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
                    _db.ProcessRequest(GetTokens(line));
                }
                catch (ErrorException)
                {
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
            DataBase db = new DataBase();
            Parser parser = new Parser(state.Reader, db);
            parser.ReadLines();
            

        }
    }
}
