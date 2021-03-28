using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    class InvalidSyntaxException : Exception
    {
        private string message = "Invalid Syntax...";

        public InvalidSyntaxException() { }
        public override string Message { get { return message; } }
        public override string ToString() { return message; } 
    }
}
