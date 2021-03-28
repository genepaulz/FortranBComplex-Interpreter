using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            Interpreter interpreter = new Interpreter();
            bool readFile = true;

            if (readFile)
            {
                string[] lines = File.ReadAllLines(@"C:\Users\seank\Desktop\test.txt");

                interpreter.PreRead(lines);
            }


            Console.WriteLine("Done!");
            Console.ReadLine();
        }


    }
}
