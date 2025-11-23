using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;



namespace Huffman
{
    public class ProgramInputOutputState : IDisposable
    {
        public const string ArgumentErrorMessage = "Argument Error";
        public const string FileErrorMessage = "File Error";

        public FileStream? Reader { get; private set; }
        public FileStream? Writer { get; private set; }

        public bool InitializeFromCommandLineArgs(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine(ArgumentErrorMessage);
                return false;
            }

            try
            {
                Reader = File.OpenRead(args[0]);
            }
            catch (IOException)
            {
                Console.WriteLine(FileErrorMessage);
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine(FileErrorMessage);
                return false;
            }
            catch (ArgumentException)
            {
                Console.WriteLine(FileErrorMessage);
                return false;
            }
            Writer = new FileStream(args[0] + ".huff", FileMode.Create, FileAccess.Write);
            return true;
        }

        public void Dispose()
        {
            Reader?.Dispose();
            Writer?.Dispose();
        }
    }


    class FileParser
    {
        public int[] byteArray { get; private set; } = new int[256];
        FileStream reader;
        byte[] buffer = new byte[4096];
        public FileParser(FileStream reader)
        {
            this.reader = reader;
        }

        void IncrementByteCount(int b)
        {
            byteArray[b]++;
        }

        public void ProcessData()
        {
            int b = reader.ReadByte();
            while (b != -1)
            {
                IncrementByteCount(b);
                b = reader.ReadByte();
            }
        }
    }

    class Node
    {
        public int characterValue;
        public int characterWeight;
        public Node? Right;
        public Node? Left;
    }
    class Tree
    {
        int[] byteArray;
        List<Node> treeRoots = new List<Node>();
        int maxCharValue = 256;

        public Tree(int[] byteArray)
        {
            this.byteArray = byteArray;
        }

        Node? FindTheLightestNode()
        {
            Node LightestNode = new Node()
            {
                characterValue = 256,
                characterWeight = int.MaxValue,
            };

            for (int i = 0; i < treeRoots.Count; i++)
            {
                if (treeRoots[i].characterValue == maxCharValue) //inner nodes
                {
                    if (treeRoots[i].characterWeight < LightestNode.characterWeight)
                    {
                        LightestNode = treeRoots[i];
                    }
                }
                else if (treeRoots[i].characterWeight <= LightestNode.characterWeight) //leaf
                {
                    if (treeRoots[i].characterWeight == LightestNode.characterWeight)
                    {
                        if (treeRoots[i].characterValue < LightestNode.characterValue)
                        {
                            LightestNode = treeRoots[i];
                        }
                    }
                    else
                    {
                        LightestNode = treeRoots[i];
                    }
                }

            }
            return LightestNode;
        }


        void BuildForest()
        {
            for (int i = 0; i < byteArray.Length; i++)
            {
                if (byteArray[i] == 0) continue;
                treeRoots.Add(new Node()
                {
                    characterValue = i,
                    characterWeight = byteArray[i],
                    Left = null,
                    Right = null,
                });
            }
        }

        void DeleteTheLightest(Node node)
        {
            treeRoots.Remove(node);
        }

        void BuildTreeFromForest()
        {
            while (treeRoots.Count > 1)
            {
                Node leftNode = FindTheLightestNode();
                DeleteTheLightest(leftNode);
                Node rightNode = FindTheLightestNode();
                DeleteTheLightest(rightNode);
                Node newRoot = new Node()
                {
                    characterValue = maxCharValue,
                    characterWeight = leftNode.characterWeight + rightNode.characterWeight,
                    Left = leftNode,
                    Right = rightNode,

                };
                treeRoots.Add(newRoot);
            }
            if (treeRoots.Count == 1)
            {
                return;
            }
        }

        void PrintTreePreOrder(Node root, bool space)
        {
            if (root == null) return;
            if (space)
                Console.Write(" ");

            if (root.Left == null && root.Right == null)
                Console.Write("*" + root.characterValue + ":" + root.characterWeight);
            else
                Console.Write(root.characterWeight);
            PrintTreePreOrder(root.Left, true);
            PrintTreePreOrder(root.Right, true);

        }
        public Node GetRoot()
        {
            return treeRoots[0];
        }
        public void Report()
        {
            BuildForest();
            BuildTreeFromForest();
            //PrintTreePreOrder(treeRoots[0], false);
        }

    }

    class HuffmanCompressor
    {
        FileStream reader;
        FileStream writer;
        Node root;
        byte[] buffer = new byte[4096];

        byte[] header = { 0x7B, 0x68, 0x75, 0x7C, 0x6D, 0x7D, 0x66, 0x66 };
        byte[] footer = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        byte[] template = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        string[] symbolsEncoding = new string[256];

        public HuffmanCompressor(FileStream reader, FileStream writer, Node root) 
        {
            this.reader = reader;
            this.writer = writer;
            this.root = root;
        }
        void ProcessData()
        {
            byte byteToWrite = 0x00;
            int count = 0;
            int b = reader.ReadByte();

            while (b != -1)
            {
                string encoding = symbolsEncoding[b];
                foreach (char c in encoding)
                {
                    byteToWrite >>= 1;
                    if (c == '1')
                    {
                        byteToWrite += 0x80;
                    }

                    count++;
                    if (count == 8)
                    {
                        writer.WriteByte(byteToWrite);
                        count = 0;
                        byteToWrite = 0x00;
                    }
                }
                b = reader.ReadByte();
            }
            if (count > 0) 
            {
                byteToWrite >>= (8 - count);
                writer.WriteByte(byteToWrite);
            }
        }
        public void ProcessFile()
        {
            writer.Write(header);
            ProcessTreeInPrefix(root, "");
            writer.Write(footer);
            ProcessData();
        }

        void ProcessLeafNode(out byte[] nodeInBytes, Node node)
        {
            nodeInBytes = template;
            nodeInBytes[7] = Convert.ToByte(node.characterValue);

            int weight = node.characterWeight;
            int byte0 = 1 + ((weight & 0x7f) << 1); 
            nodeInBytes[0] = Convert.ToByte(byte0);
            weight >>= 7;
            for (int i = 1; i < 7; i++) 
            {
                nodeInBytes[i] = Convert.ToByte(weight & 0xFF);
                weight >>= 8;
            }
        }

        void ProcessInnerNode(out byte[] nodeInBytes, Node node)
        {
            nodeInBytes = template;
            nodeInBytes[7] = 0x00;

            int weight = node.characterWeight;
            int byte0 = ((weight & 0x7f) << 1);
            nodeInBytes[0] = Convert.ToByte(byte0);
            weight >>= 7;
            for (int i = 1; i < 7; i++)
            {
                nodeInBytes[i] = Convert.ToByte(weight & 0xFF);
                weight >>= 8;
            }
        }

        void ProcessTreeInPrefix(Node root, string encode)
        {
            byte[] symbolInBytes;
            if (root == null) return;
            

            if (root.Left == null && root.Right == null)
            {
                ProcessLeafNode(out symbolInBytes, root);
                writer.Write(symbolInBytes);
                symbolsEncoding[root.characterValue] = encode;
            }
            else
            {
                ProcessInnerNode(out symbolInBytes, root);
                writer.Write(symbolInBytes);

            }
            ProcessTreeInPrefix(root.Left, encode + "0");
            ProcessTreeInPrefix(root.Right, encode + "1");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var state = new ProgramInputOutputState();
            if (!state.InitializeFromCommandLineArgs(args))
            {
                return;
            }
            FileParser fileParser = new FileParser(state.Reader);
            fileParser.ProcessData();
            Tree tree = new Tree(fileParser.byteArray);
            tree.Report();
            HuffmanCompressor compressor = new HuffmanCompressor(new FileStream(args[0], FileMode.Open, FileAccess.Read), state.Writer, tree.GetRoot());
            compressor.ProcessFile();
            state.Dispose();
        }
    }
}


