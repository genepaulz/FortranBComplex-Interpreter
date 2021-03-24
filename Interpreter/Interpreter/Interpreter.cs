using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Interpreter
{
    class Interpreter
    {
        Dictionary<string, dynamic> variableList;
        Dictionary<string, dynamic> dataTypes;
        Dictionary<string, string> patterns;

        List<string> reserved;

        bool hasStarted = false;
        bool hasFinished = false;

        public Interpreter()
        {
            this.variableList = new Dictionary<string, dynamic>();
            this.dataTypes = new Dictionary<string, dynamic>(4);

            this.dataTypes.Add("INT", 0);
            this.dataTypes.Add("FLOAT", 0.0f);
            this.dataTypes.Add("BOOL", false);
            this.dataTypes.Add("CHAR", ' ');

            var patterns = new[] {
                new {ID = "Declaration" , Pattern = @"^\s*VAR\s+([a-zA-Z_][a-zA-Z0-9_]*|[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*(([a-zA-Z][a-zA-Z0-9]*|-?\d+|-?\d+.\d+))(\s*[+-/*]\s*(([a-zA-Z][a-zA-Z0-9]*|-?\d+|-?\d+.\d+)))*|[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*(\'\w\'|""(TRUE|FALSE)""))(\s*,\s*([a-zA-Z_][a-zA-Z0-9_]*|[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*(([a-zA-Z][a-zA-Z0-9]*|-?\d+|-?\d+.\d+))(\s*[+-/*]\s*(([a-zA-Z][a-zA-Z0-9]*|-?\d+|-?\d+.\d+)))*|[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*(\'\w\'|""(TRUE|FALSE)"")))*\s+AS\s+(INT|FLOAT|BOOL|CHAR)\s*$"},
                new {ID = "Assignment", Pattern = @"^([a-zA-Z][a-zA-Z0-9]*)\s*=\s*.*"},
                new {ID = "Input", Pattern = @"^INPUT\s*:\s*([a-zA-Z_]\w*)(\s*\,\s*[a-zA-Z_]\w*)*$"},
                new {ID = "Output", Pattern = @"^OUTPUT\s*:.*$"},
                new {ID = "Start", Pattern = @"^START$"},
                new {ID = "Stop", Pattern = @"^STOP$"},
                new {ID = "Comment", Pattern = @"^\*.*"},
                new {ID = "Unary", Pattern = @"^(([a-zA-Z_][a-zA-Z0-9_]*(\+\+|\-\-))|((\+\+|\-\-|!)[a-zA-Z_][a-zA-Z0-9_]*)|(~([a-zA-Z_][a-zA-Z0-9_]*|\d+)))"},
                new {ID = "IF", Pattern = @"^\s*(IF)\s*\([\w\W]+\s*\)\s*$"}
            };

            this.patterns = patterns.ToDictionary(n => n.ID, n => n.Pattern);

            var reserved = new[]
            {
                "VAR",
                "AS",
                "INT",
                "FLOAT",
                "BOOL",
                "CHAR",
                "START",
                "STOP"
            };

            this.reserved = new List<string>(reserved);
        }
        public Dictionary<string, object> Variables
        {
            get { return variableList; }
            set { variableList = value; }
        }

        public Dictionary<string, string> Patterns
        {
            get { return patterns; }
        }
        public bool HasStarted
        {
            get { return hasStarted; }
        }
        public bool HasFinished
        {
            get { return hasFinished; }
        }

        public string Interpret(string line)
        {
            string output = "";

            try
            {
                foreach (KeyValuePair<string, string> pattern in patterns)
                {
                    Match match = Regex.Match(line, pattern.Value);

                    if (match.Success)
                    {
                        switch (pattern.Key)
                        {
                            case "Declaration": output = Declaration(line); break;
                            case "Assignment": output = Assignment(line); break;
                            case "Input": output = Input(line); break;
                            case "Output": output = Output(line); break;
                            case "Comment": output = Comment(line);break;
                            case "Unary": output = Unary(line); break;
                            case "IF": output = IF(line); break;
                            case "Start":
                                if (!hasStarted)
                                    hasStarted = true;
                                else
                                    throw new Exception();
                                break;

                            case "Stop":
                                if (hasStarted)
                                    hasFinished = true;
                                else
                                    throw new Exception();
                                break;
                        }

                        return output;
                    }
                }

                throw new Exception();

            }
            catch (Exception e)
            {
                return output = e.Message;
            }
        }


        public string Declaration(string line)
        {
            string output = "";

            try
            {
                if (!hasStarted)
                {
                    string trimmer_p = @"(?<=\bVAR\s+).*(?=AS\s*(INT|FLOAT|BOOL|CHAR))";
                    string variables_p = @"^[a-zA-Z_][a-zA-z0-9_]*";
                    string variablesType_p = @"\b(INT|FLOAT|BOOL|CHAR)\b";

                    string trimmed = Regex.Match(line, trimmer_p).Value;
                    string[] sets = Regex.Split(trimmed, ",");

                    string variableType = Regex.Match(line, variablesType_p).Value;

                    foreach (string set in sets)
                    {
                        string temp = set.Trim();

                        string variableName = Regex.Match(temp, variables_p).Value;

                        if (isVariable(variableName))
                        {
                            if (!variableList.ContainsKey(variableName))
                            {
                                variableList.Add(variableName, dataTypes[variableType]);

                                output = Assignment(temp, 1,variableType, variableName);
                            }
                            else
                            {
                                variableList.Remove(variableName);
                                throw new Exception(); // VARIABLE ALREADY DECLARED
                            }
                        } else
                        {
                            throw new Exception(); // INVALID VAR NAME
                        }
                        
                    }
                }
                else
                {
                    throw new Exception(); // START NA PROGRAM
                }
            }
            catch (Exception e)
            {
                output = e.Message;
            }


            return output;
        }

        public string Assignment(string line,int del = 0,string variableType = "", string variableName = "")
        {
            string output = "";

            string variable_p = @"^[a-zA-Z_][a-zA-z0-9_]*";

            string[] sets = Regex.Split(line, ",");

            try
            {
                if (variableName == "" && variableType == "")
                {
                    variableName = Regex.Match(line, variable_p).Value;

                    if (isVariableReal(variableName))
                    {
                        if (isInt(variableList[variableName])) variableType = "INT";
                        else if (isFloat(variableList[variableName])) variableType = "FLOAT";
                        else if (isBool(variableList[variableName])) variableType = "BOOL";
                        else variableType = "CHAR";
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                var v = dataTypes[variableType];
                
                foreach (string set in sets)
                {

                    string[] trimmed = Regex.Split(set, "=");

                    if(trimmed.Length == 2)
                    {
                        string variable = trimmed[0].Trim();
                        string expression = trimmed[1].Trim();

                        if (variableType == "INT" | variableType == "FLOAT")
                        {
                            string[] post = Postfix.ToPostFix(expression);

                            if (post != null)
                            {
                                v = Postfix.QuickMath(post, this);
                            }

                        }
                        if (variableType == "BOOL")
                        {
                            if (expression == "\"TRUE\"") v = true;
                            else if (expression == "\"FALSE\"") v = false;
                            else v = IsTrue(expression);
                            
                            if(v == null) throw new FormatException();
                        }
                        if (variableType == "CHAR")
                        {
                            if (expression.Length == 3) v = expression[1];
                            else throw new FormatException();
                        }
                    }
                }

                variableList[variableName] = v;

                return output;
            }
            catch (Exception e)
            {
                if(del == 1)
                variableList.Remove(variableList.Keys.Last());

                return output = e.Message;
            }
        }
        public string Input(string line)
        {
            string output = "";
            try
            {
                if (hasStarted)
                {
                    string[] variables = Regex.Matches(line, @"[^(INPUT:\,\s+)]\w*")
                        .Cast<Match>()
                        .Select(m => m.Value)   
                        .ToArray();

                    foreach (string variable in variables)
                    {
                        if (isVariable(variable))
                        {
                            if (isVariableReal(variable))
                            {
                                var v = variableList[variable];

                                var input = Console.ReadLine();

                                if (isInt(v))
                                {
                                    v = Int32.Parse(input);
                                }
                                else if (isFloat(v))
                                {
                                    v = Single.Parse(input);
                                }
                                else if (isBool(v))
                                {

                                    if (input == "TRUE")
                                        v = true;
                                    else if (input == "FALSE")
                                        v = false;
                                    else
                                        throw new InvalidCastException();
                                }
                                else if (isChar(v))
                                {
                                    if (Regex.Match(input, @"^\w$").Success)
                                    {
                                        v = input;
                                    }
                                    else
                                    {
                                        throw new FormatException();
                                    }
                                }
                                variableList[variable] = v;
                                Console.Write("" + variableList[variable]);
                            }
                            else
                            {
                                throw new NullReferenceException();
                            }
                        }
                        else
                        {
                            throw new FormatException();
                        }

                    }

                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception e)
            {
                output = e.Message;
            }

            return output;
        }

        public string Output(string line)
        {
            string output = "";
            string outputString = "";
            //string bug = "\nIDebug\n";
            try
            {
                if (hasStarted)
                {
                    string input = Regex.Match(line, @"[^OUTPUT\s*:].*").Value;
                    string head = "";
                    bool processed = false;
                    bool isSubstring = false;
                    bool fromSubstring = false;
                    int count = 0;

                    while (!processed)
                    {

                        for (int i = count; i < input.Length; count++, i++)
                        {
                            if (isSubstring)
                            {
                                if (input[i] == '\"')
                                {
                                    isSubstring = false;
                                    fromSubstring = true;
                                    break;
                                }
                                else
                                {
                                    head += input[i];
                                }
                            }
                            else if (input[i] == '\"')
                            {
                                isSubstring = true;
                                fromSubstring = false;
                                continue;
                            }
                            else if (input[i] != ' ')
                                head += input[i];
                            else break;

                        }

                        if (isVariableReal(head))
                        {
                            outputString += "" + variableList[head] + "";
                        }
                        else if (head == "&")
                        {
                            count++;
                            head = "";
                            continue;
                        }
                        else if (fromSubstring)
                        {
                            if (head.Contains("#"))
                            {
                                if (head.Contains("["))
                                {
                                    if ((head.IndexOf("]", 2) - 2) != head.IndexOf("[", 0))
                                    {
                                        throw new Exception("Invalid Escape Character!");
                                    }
                                    else if (head.IndexOf("]") == 1)
                                    {
                                        outputString += head[head.IndexOf("[") + 1];
                                    }
                                    else
                                    {
                                        bool opened = false;
                                        bool closed = false;
                                        for (int i = 0; i < head.Length; i++)
                                        {
                                            if (head[i] == '[' && opened == false)
                                            {
                                                opened = true;
                                                continue;
                                            }

                                            if (head[i] == ']' && closed == false)
                                            {
                                                closed = true;
                                                continue;
                                            }

                                            else if (head[i] == '[' && opened)
                                            {
                                                outputString += head[i];
                                            }

                                            else if (head[i] == ']' && closed)
                                            {
                                                outputString += head[i];
                                            }
                                            else
                                            {
                                                outputString += head[i];
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    outputString += "\n";
                                }
                            }
                            else if (head.Contains("["))
                            {
                                if ((head.IndexOf("]", 2) - 2) != head.IndexOf("[", 0))
                                {
                                    throw new Exception("Invalid Escape Character!");
                                }
                                else if (head.IndexOf("]") == 1)
                                {
                                    outputString += head[head.IndexOf("[") + 1];
                                }
                                else
                                {
                                    bool opened = false;
                                    bool closed = false;
                                    for (int i = 0; i < head.Length; i++)
                                    {
                                        if (head[i] == '[' && opened == false)
                                        {
                                            opened = true;
                                            continue;
                                        }

                                        if (head[i] == ']' && closed == false)
                                        {
                                            closed = true;
                                            continue;
                                        }

                                        else if (head[i] == '[' && opened)
                                        {
                                            outputString += head[i];
                                        }

                                        else if (head[i] == ']' && closed)
                                        {
                                            outputString += head[i];
                                        }
                                        else
                                        {
                                            outputString += head[i];
                                        }
                                    }
                                }
                            }
                            else if (head != null)
                            {
                                outputString += head;
                            }
                        }
                        else
                        {
                            throw new NullReferenceException();
                        }

                        head = "";
                        if (count == input.Length)
                        {
                            processed = true;
                        }
                        count++;
                    }
                    Console.WriteLine(outputString);
                    //Console.WriteLine(bug);
                    outputString = null;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception e)
            {
                output = e.Message;
            }
            return output;
        }

        public string Comment(string line)
        {
            string output = "";

            try
            {
                if (hasStarted)
                {
                    if (Regex.Match(line, @"^\*.*").Success)
                    {
                        clearConsoleLine();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("" + line);
                        Console.ResetColor();
                    }
                }
                else { throw new Exception(); }
            }
            catch (Exception e)
            {

                output = e.Message;
            }

            return output;
        }

        public string Unary(string line)
        {
            string output = "";

            string variable_p = @"[a-zA-Z_][a-zA-Z0-9_]*";
            string action_p = @"(\+\+|--)";

            try
            {
                if (hasStarted)
                {
                    var variable = Regex.Match(line, variable_p).Value;
                    var action = Regex.Match(line, action_p).Value;


                    if (isVariable(variable))
                    {
                        if (isVariableReal(variable))
                        {
                            
                            switch (action)
                            {
                                case "++":++variableList[variable]; break;
                                case "--":--variableList[variable];break;
                            }                          
                            
                        }
                        else
                        {
                            throw new NullReferenceException();
                        }
                    }
                    else
                    {
                        throw new FormatException();
                    }

                }
                else
                {
                    throw new Exception();
                }

            }
            catch (Exception e)
            {
                output = e.Message;
            }

            return output;
        }
        public string IF(string line)
        {
            string output = "";
            try
            {
                string exp_p = @"\(.*\)\s*$";
                string expression = Regex.Match(line, exp_p).Value.Trim();

                bool? run = IsTrue(expression);

                if(run == null) { throw new InvalidOperationException(); }

                string l = "";

                Interpreter temp = new Interpreter();
                temp.Patterns.Remove("Declaration");
                temp.Variables = this.variableList;

                do
                {
                    if (temp.HasFinished) { break; }
                    l = Console.ReadLine();
                    string result = temp.Interpret(l);
                    Console.WriteLine(result);
                }
                while (true);

                if(run == true)
                {
                    this.Variables = temp.Variables;
                }
            }
            catch(Exception e)
            {
                output = e.Message;
            }
            return output;
        }
        public bool? IsTrue(string line)
        {
            bool? output = true;

            var stack = new Stack<string>();
            var postfix = new Stack<string>();

            string word = "";
            bool beenSpace = false;

            try
            {
                string exp = line.Replace(" ", "");
                exp = exp.Replace(" ", "");
                exp = exp.Replace("AND", "&");
                exp = exp.Replace("OR", "|");
                exp = exp.Replace("NOT", "!");

                string pattern = @"(\&|\||\!)|([\(\)])|([\(\)])|([a-zA-Z][a-zA-Z0-9]*|-?\d+|-?\d+.\d+)|(\<\>)|(\>\=)|(\<\=)|(\=\=)|([\<\>\=])";

                string[] l = Regex.Split(exp, pattern);

                for (int i = 0; i < l.Length; i++)
                {
                    string s = l[i];

                    if (l[i] == "") continue;
                    if (isVariable(s))
                    {
                        if (isVariableReal(s))
                        {
                            postfix.Push(s);
                        }
                    }
                    else if (s == "(")
                    {
                        stack.Push(s);
                    }
                    else if (s == ")")
                    {
                        while (stack.Peek() != "(")
                        {
                            postfix.Push(stack.Pop().ToString());
                        }
                        stack.Pop();
                    }
                    else if (Regex.Match(s, @"(\<\=)|(\>\=)|(\<\>)|(\<)|(\>)|(\=\=)|(\&)|(\|)|(\!)").Success)
                    {
                        while (stack.Count != 0 && stack.Peek() != "(" && Priority(stack.Peek()) >= Priority(s))
                        {
                            postfix.Push(stack.Pop().ToString());
                        }
                        stack.Push(s);
                    } 
                    else if(Regex.Match(s, @"^(-?\d+|-?\d+.\d+)$").Success)
                    {
                        postfix.Push(s);
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }

                while (stack.Count != 0)
                {
                    if (stack.Peek() == "(")
                    {
                        throw new InvalidOperationException();
                    }
                    else
                    {
                        postfix.Push(stack.Pop().ToString());
                    }
                }

                string[] pf = postfix.Reverse().ToArray();

                var temp = new Stack<dynamic>();
                for(int i = 0; i < pf.Length; i++)
                {
                    string s = pf[i];
                    if (isVariable(s))
                    {
                        temp.Push(variableList[s]);
                    }
                    else if (Regex.Match(s, @"(\<\=)|(\>\=)|(\<\>)|(\<)|(\>)|(\=\=)|(\&)|(\|)|(\!)").Success)
                    {
                        var y = temp.Pop();
                        var x = temp.Pop();

                        if (x.GetType() == typeof(string))
                            if(isDigit(x))
                            x = Single.Parse(x);
                        if (y.GetType() == typeof(string))
                            if(isDigit(y))
                            y = Single.Parse(y);

                        bool flag = false;
                        if (s == "&")
                            flag = x & y;
                        else if (s == "|")
                            flag = x || y;
                        else if (s == "<")
                            flag = x < y;
                        else if (s == ">")
                            flag = x > y;
                        else if (s == "<=")
                            flag = x <= y;
                        else if (s == ">=")
                            flag = x >= y;
                        else if (s == "<>")
                            flag = x != y;
                        else if (s == "==")
                            flag = x == y;

                        temp.Push(flag);
                    }
                    else if ("!".Contains(s))
                    {
                        var x = temp.Pop();
                        temp.Push(!x);
                    }
                    else
                    {
                        var x = Single.Parse(s);
                        temp.Push(s);
                    }
                }

                output = temp.Pop();
            }
            catch (Exception e)
            {
                output = null;
            }

            return output;
        }

        static int Priority(string c)
        {
            int p = 1;
            if (c == "!") p = -3;
            if (c == "&" ) p = -2;
            if (c == "|") p = -1;
            return p;
        }
        public void clearConsoleLine()
        {
            int currentLine = Console.CursorTop - 1;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLine);
        }
        public bool isReserved(string name)
        {
            return reserved.Contains(name);
        }
        public bool isVariable(string name)
        {
            return (Regex.Match(name, @"^[a-zA-Z_][a-zA-z0-9_]*").Success) ? (isReserved(name))? false : true : false;
        }
        public bool isDigit(string name)
        {
            return Regex.Match(name, @"-?\d+|-?\d+.\d+").Success;
        }
        public bool isVariableReal(string name)
        {
            return (this.variableList.ContainsKey(name)) ? true : false;
        }
        public bool isInt(dynamic var)
        {
            return (var.GetType() == typeof(int)) ? true : false;
        }
        public bool isFloat(dynamic var)
        {
            return (var.GetType() == typeof(float)) ? true : false;
        }
        public bool isBool(dynamic var)
        {
            return (var.GetType() == typeof(bool)) ? true : false;
        }

        public bool isChar(dynamic var)
        {
            return (var.GetType() == typeof(char)) ? true : false;
        }
    }
}
