using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            Interpreter interpreter = new Interpreter();
            string time = DateTime.Now.ToString("h:mm:ss tt");
            Console.WriteLine(time);

            while (true)
            {
                if (interpreter.HasFinished) { break; }
                string output = interpreter.Interpret(Console.ReadLine());
                Console.WriteLine(output);
            }

            time = DateTime.Now.ToString("h:mm:ss tt");
            Console.WriteLine(time);
            Console.ReadLine();
        }

        
    }
}
