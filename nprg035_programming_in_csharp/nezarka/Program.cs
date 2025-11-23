using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Numerics;
using NezarkaBookstore;
using System.Reflection.Emit;

namespace NezarkaBookstore
{
    internal class Program
    {
        static void Main(string[] args)
        {
            StreamReader reader = new StreamReader(Console.OpenStandardInput());

            ModelStore modelStore = ModelStore.LoadFrom(reader);
            
            if (modelStore == null ) 
            {
                Console.WriteLine("Data error.");
                return;
            }
            Controller nezartkaController = new Controller(reader, modelStore);
            nezartkaController.ParseRequests();
        }
    }
}