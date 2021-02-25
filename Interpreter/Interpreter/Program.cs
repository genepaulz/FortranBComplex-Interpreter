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
            string time = "Time start " + DateTime.Now.ToString("h:mm:ss tt");
            Console.WriteLine(time);
            bool readFile = true;
            while (!readFile)
            {
                if (interpreter.HasFinished) { break; }
                string output = interpreter.Interpret(Console.ReadLine());
                Console.WriteLine(output);
            }

            if (readFile)
            {
                //OPEN FILE TEST
                StreamReader s1 = new StreamReader(@"C:\Users\Zafra\Desktop\test1.txt");
                //StreamReader s2 = new StreamReader(@"C:\Users\Zafra\Desktop\PROG_test.txt");
                string liner1 = s1.ReadLine();
                //string liner2 = s2.ReadLine();

                //Thread.Sleep(2000);
                while (readFile)
                {
                    if (interpreter.HasFinished | liner1 == null) break;
                    Console.WriteLine(liner1);
                    string output = interpreter.Interpret(liner1);
                    //string liner1 = interpreter.Interpret(liner2);

                    //Console.WriteLine(liner2);
                    Console.WriteLine(output);
                    liner1 = s1.ReadLine();
                }
                s1.Close();
                //s2.Close();
            }

            time = "Time ended " + DateTime.Now.ToString("h:mm:ss tt");
            Console.WriteLine(time);
            Console.ReadLine();
        }


    }
}
