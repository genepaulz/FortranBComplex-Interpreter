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
        Guid guid = Guid.NewGuid();

        Dictionary<string, dynamic> variableList;
        Dictionary<string, dynamic> dataTypes;
        Dictionary<string, string> patterns;

        List<string> reserved;

        bool hasStarted = false;
        bool hasFinished = false;
        bool whileIf = false;


        List<string[]> Program = new List<string[]>();
        List<string[]> Outputs = new List<string[]>();
        Dictionary<string, List<string[]>> subProgram = new Dictionary<string, List<string[]>>();


        // CONSTRUCTOR --------------------
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
                new {ID = "Unary", Pattern = @"^[a-zA-Z_][a-zA-z0-9_]*\s*\=\s*(\-[a-zA-Z_][a-zA-z0-9_]*|\+[a-zA-Z_][a-zA-z0-9_]*)\s*$"},
                new {ID = "Assignment", Pattern = @"^([a-zA-Z][a-zA-Z0-9]*)\s*=\s*.*"},
                new {ID = "Input", Pattern = @"^INPUT\s*:\s*([a-zA-Z_]\w*)(\s*\,\s*[a-zA-Z_]\w*)*$"},
               //new {ID = "Output", Pattern = @"^OUTPUT\s*:.*$"},
                new {ID = "Start", Pattern = @"^\s*(START)\s*$"},
                new {ID = "Stop", Pattern = @"^\s*(STOP)\s*$"},
                new {ID = "Comment", Pattern = @"^\*.*"},
                new {ID = "UnaryPlus", Pattern = @"^(([a-zA-Z_][a-zA-Z0-9_]*(\+\+|\-\-))|((\+\+|\-\-|!)[a-zA-Z_][a-zA-Z0-9_]*)|(~([a-zA-Z_][a-zA-Z0-9_]*|\d+)))"},
                
               new {ID = "IF", Pattern = @"^\s*(IF)\s*\([\w\W]+\s*\)\s*$"},
               new {ID = "ELSE", Pattern = @"^\s*(ELSE)\s*$"},
               new {ID = "WHILE", Pattern = @"^\s*(WHILE)\s*\([\w\W]+\s*\)\s*$"},
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
                "STOP",
                "IF",
                "WHILE",
                "ELSE"
            };

            this.reserved = new List<string>(reserved);
        }



        // PRE-READERS

        public void PreRead(string[] program, int start = 0)
        {
            for (int line = start; line < program.Length; line++)
            {
                string statement = program[line].Trim();

                if (statement == "") continue; // Empty Line

                string pattern = Pattern(statement);
                if (pattern == null)
                {
                    Program = null; // Syntax Error
                    break;
                }

                if (pattern == "Start") Program.Add(new[] { "Start" });
                else if (pattern == "Stop") Program.Add(new[] { "Stop" });

                else
                {
                    List<string[]> code = null;

                    int starting = line + 1;
                    int last = starting;

                    if (pattern == "IF" | pattern == "WHILE")
                    {
                        int type = 0; // 0 = IF, 1 = ELSE, 2 = WHILE

                        if (pattern == "WHILE") type = 2;

                        code = IF(program, starting, ref last, type);
                        line = last;

                        if (type == 0)
                        {
                            int foundElse = findElse(program, last);

                            if (foundElse > -1)
                            {
                                List<string[]> elseCode = null;

                                elseCode = IF(program, foundElse, ref last, 1);

                                if (elseCode == null)
                                {
                                    Program = null;
                                    break;
                                }

                                code.AddRange(elseCode);
                                line = last;
                            }
                        }
                    }
                    else if (pattern != "Start" && pattern != "Stop")
                    {
                        if (pattern == "ELSE")
                        {
                            Program = null;
                            break;
                        }

                        code = CallMethod(pattern, new[] { statement });
                    }

                    if (code == null)
                    {
                        Program = null;
                        break;
                    }

                    string[][] codes = code.ToArray();

                    foreach (string[] group in codes)
                    {
                        Program.Add(group);
                    }
                }
            }

            if (Program != null)
            {
                if (!RunProg())
                    Console.WriteLine("Something went wrong...");
                else
                {
                    foreach (KeyValuePair<string, dynamic> entry in variableList)
                    {
                        Console.WriteLine(entry.Key + ":" + entry.Value);
                    }
                }
            }
            else Console.WriteLine("Something went wrong...");
        }


        public bool RunProg(int type = 0, string subId = "") // type: 0 = Main, 1 = Sub
        {
            bool Success = true;

            bool hasStarted = false;
            bool hasFinished = false;

            int size = Program.Count;
            var progList = Program;

            if (type == 1) 
            { 
                size = subProgram[subId].Count;
                progList = subProgram[subId];
            }
            try
            {
                for (int index = 0; index < size; index++)
                {
                    string[] statement = progList[index];
                    string function = statement[0];

                    if (function == "Start")
                        if (hasStarted) Success = false;
                        else hasStarted = true;
                    else if (function == "Stop")
                    {
                        if (hasStarted)
                            hasFinished = true;
                        break;
                    }
                    else if (function == "Comment") continue;

                    else
                    {
                        string varName = statement[1];
                        string varValue = statement[2];


                        if (function == "Declaration")
                        {
                            if (!variableList.ContainsKey(varName))
                            {
                                Success = CreateVar(varName, statement[2]);
                            }
                            else Success = false;
                        }
                        else if (function == "Assignment")
                        {
                            Success = AssignVar(varName, varValue);
                        }
                        else if (function == "UnaryPlus" | function == "Unary")
                        {
                            int operType = 0;

                            if (function == "UnaryPlus")
                                if (statement[3] == "--") operType = 1;
                                else
                                if (statement[2] == "-") operType = 1;

                            if (function == "UnaryPlus")
                                Success = Increment(varName, operType);
                            else
                                Success = Positive(varName, operType);
                        }
                        else if (function == "IF_statement" | function == "ELSE" | function == "WHILE_statement")
                        {
                            string expression = varName;

                            bool? run = IsTrue(expression);

                            if (run == null)
                            {
                                Success = false;
                                break;
                            }

                            string SubId = varValue;
                            
                            if(function == "IF_statement")
                            {
                                if (run == false)
                                {
                                    if (progList[index + 1][0] == "ELSE")
                                    {
                                        index++;
                                        SubId = progList[index][2];
                                        Success = RunProg(1, SubId);
                                    }
                                }
                                else Success = RunProg(1, SubId);
                            }
                            else if(function == "WHILE_statement")
                            {
                                if (run == true)
                                {
                                    while (run == true)
                                    {
                                        Success = RunProg(1, SubId);

                                        run = IsTrue(expression);
                                    }
                                }
                            }
                        }
                    }


                    if (Success == false) break;
                }
            } 
            catch(Exception e)
            {
                Success = false;
            }
            



            return Success;
        }

        public bool Positive(string varName, int type = 0) // type: 0 = postive, 1 = negative
        {
            try
            {
                if (type == 1)
                    variableList[varName] *= -1;
                else
                    variableList[varName] = Math.Abs(variableList[varName]);
            }catch(Exception e)
            {
                return false;
            }
            return true;
        }
        public bool Increment(string varName, int type = 0) // type: 0 = increment, 1 = decrement
        {
            try
            {
                if (type == 1) variableList[varName]--;
                else variableList[varName]++;
            }
            catch(Exception e)
            {
                return false;
            }

            return true;
        }

        public bool CreateVar(string varName, string varType)
        {
            if (!isVariableReal(varName))
            {
                try
                {
                    variableList.Add(varName, dataTypes[varType]);
                }
                catch(Exception e)
                {
                    return false;
                }
                return true;
            }

            return false;
        }
        public bool AssignVar(string varName, string val)
        {
            bool Success = true;

            string varType = GetVarType(varName);
            string trimVal = val.Trim();

            var Var = dataTypes[varType];

            try
            {
                if (varType == "INT" | varType == "FLOAT")
                {
                    string[] post = Postfix.ToPostFix(trimVal);

                    if (post != null)
                        Var = Postfix.QuickMath(post, this);
                }
                if (varType == "BOOL")
                {
                    if (trimVal == "\"TRUE\"") Var = true;
                    else if (trimVal == "\"FALSE\"") Var = false;
                    else Var = IsTrue(trimVal);

                    if (Var == null)
                        Success = false;
                }
                if (varType == "CHAR")
                {
                    if (trimVal.Length != 3) Success = false;
                    else Var = trimVal[1];
                }
            }
            catch(Exception e)
            {
                Success = false;
            }
            

            if (Success) variableList[varName] = Var;

            return Success;
        }
        public string GetVarType(string varName)
        {
            string varType = "NULL";

            var Var = variableList[varName];

            if (isInt(Var)) varType = "INT";
            if (isFloat(Var)) varType = "FLOAT";
            if (isChar(Var)) varType = "CHAR";
            if (isBool(Var)) varType = "BOOL";

            return varType;
        }


        // FUNCTIONS --------------------

        public List<string[]> Declaration(string statement)
        {
            string value    = @"(?<=\bVAR\s+).*(?=AS\s*(INT|FLOAT|BOOL|CHAR))";
            string varName  = @"^[a-zA-Z_][a-zA-z0-9_]*";
            string varType  = @"\b(INT|FLOAT|BOOL|CHAR)\b";

            string values = Regex.Match(statement, value).Value;
            string[] sets = Regex.Split(values, ",");

            string variableType = Regex.Match(statement, varType).Value;

            List<string[]> codes = new List<string[]>();

            foreach (string set in sets)
            {
                string line = set.Trim();
                string variableName = Regex.Match(line, varName).Value;

                if (isVariable(variableName))
                {
                    codes.Add(new[] { "Declaration", variableName, variableType });
                    codes.Add(CallMethod("Assignment", new[] { line }).ToArray()[0]);
                }
                else return null; // Invalid Variable Name
            }

            return codes;
        }
        public List<string[]> Assignment(string statement)
        {
            string variableName = "";
            string variable_p = @"^[a-zA-Z_][a-zA-z0-9_]*";
            variableName = Regex.Match(statement, variable_p).Value;

            List<string[]> codes = new List<string[]>();

            if (isVariable(variableName))
            {
                string[] trimmed = Regex.Split(statement, "=");

                string value = trimmed[trimmed.Length - 1].Trim();

                if (trimmed.Length > 1)
                    for (int index = 1; index < trimmed.Length; index++)
                    {
                        string cell = trimmed[index-1].Trim();

                        if (!isVariable(cell))
                            if ((index + 1) != trimmed.Length)
                                return null;

                        codes.Add(new[] { "Assignment", cell, value });
                    }
                else if (trimmed.Length == 1)
                    codes.Add(new[] { "Assignment", trimmed[0].Trim(), "Default" });
                else
                    return null;
            }
            else return null;

            return codes;
        }
        public List<string[]> Input(string statement)
        {
            string[] variables = Regex.Matches(statement, @"[^(INPUT:\,\s+)]\w*")
                .Cast<Match>()
                .Select(m => m.Value)   
                .ToArray();

            List<string[]> codes = new List<string[]>();

            foreach (string variable in variables)
            {
                if (!isVariable(variable))
                    return null;

                codes.Add(new[] { "Input", variable });
            }

            return codes;
        }
        public List<string[]> Comment(string statement)
        {
            return new List<string[]>() { new[] { "Comment" } };
        }
        public List<string[]> UnaryPlus(string statement)
        {
            string varName = @"[a-zA-Z_][a-zA-Z0-9_]*";
            string varOperator = @"(\+\+|--)";

            var variableName = Regex.Match(statement, varName).Value.Trim();
            var variableOperator = Regex.Match(statement, varOperator).Value;

            List<string[]> codes = new List<string[]>();

            if (!isVariable(variableName)) return null;

            codes.Add(new[] { "UnaryPlus", variableName, variableOperator });

            return codes;
        }
        public List<string[]> Unary(string statement)
        {
            string varName = @"^[a-zA-Z_][a-zA-Z0-9_]*";
            string varOperator = @"(\+|\-)";

            var variableName = Regex.Match(statement, varName).Value.Trim();
            var variableOperator = Regex.Match(statement, varOperator).Value;

            List<string[]> codes = new List<string[]>();

            if (!isVariable(variableName)) return null;

            string[] trimmed = Regex.Split(statement, "=");

            string targetVariable = Regex.Match(trimmed[1], @"[a-zA-Z_][a-zA-Z0-9_]*").Value;

            
            codes.Add(new[] { "Unary", variableName, targetVariable, variableOperator });

            return codes;
        }
        public List<string[]> IF(string[] program, int start, ref int last, int type = 0) // SUPPORTS BOTH WHILE & ELSE
        {
            List<string[]> SubProgram = new List<string[]>();

            bool hasStarted = false;
            bool hasFinished = false;

            for (int line = start; line < program.Length; line++)
            {
                last = line;
                string statement = program[line].Trim();

                if (statement == "") continue; // Empty Line

                string pattern = Pattern(statement);
                if (pattern == null)
                {
                    SubProgram = null; // Syntax Error
                    break;
                }

                if (pattern == "Start")
                {
                    if (hasStarted) // Already Started
                    {
                        SubProgram = null;
                        break;
                    }
                    else
                    {
                        hasStarted = true;
                        SubProgram.Add(new[] { "Start" });
                    }
                }
                else if (pattern == "Stop")
                {
                    if(!hasStarted) // Haven't Started
                        SubProgram = null;
                    else
                    {
                        hasFinished = true;
                        SubProgram.Add(new[] { "Stop" });
                    }
                    break;
                }

                else
                {
                    List<string[]> code = null;

                    if ((pattern == "IF" | pattern == "WHILE" ) && hasStarted)
                    {
                        int starting = line + 1;
                        int last_index = starting;

                        int subtype = 0; // 0 = IF, 1 = ELSE, 2 = WHILE

                        if (pattern == "WHILE") subtype = 2;

                        code = IF(program, starting, ref last_index, subtype);

                        line = last_index;

                        if(subtype == 0)
                        {
                            int foundElse = findElse(program, last_index);

                            if (foundElse > -1)
                            {
                                List<string[]> elseCode = null;

                                elseCode = IF(program, foundElse, ref last, 1);

                                if (elseCode == null)
                                {
                                    Program = null;
                                    break;
                                }

                                code.AddRange(elseCode);
                                line = last;
                            }
                        }
                    }

                    else if (pattern != "Start" && pattern != "Stop" && hasStarted)
                    {
                        if(pattern == "ELSE")
                        {
                            Program = null;
                            break;
                        }

                        code = CallMethod(pattern, new[] { statement });
                    }

                    if (code == null)
                    {
                        SubProgram = null;
                        break;
                    }

                    string[][] codes = code.ToArray();

                    foreach (string[] group in codes)
                    {
                        SubProgram.Add(group);
                    }
                }
            }

            if (SubProgram == null) return null;


            string subId = GetUniqueId();

            subProgram.Add(subId, SubProgram);

            string funct = "IF_statement";
            string expression_pattern = @"\(.*\)\s*$";
            string expression = "";
            
            if (type == 0 | type == 2)
                expression = Regex.Match(program[start - 1], expression_pattern).Value.Trim();
            else
                expression = funct = "ELSE";

            if (type == 2)
                funct = "WHILE_statement";

            return new List<string[]>()
            {
                new[] { funct, expression, subId }
            };
        }
        public int findElse(string[] program, int start) // FINDS POS ELSE_statement
        {
            int foundIndex = -1;
            start += 1;
            for(int index = start; index < program.Length; index++)
            {
                string statement = program[index];

                if (statement == "") continue; // Empty Line
                else
                {
                    string pattern = Pattern(statement);
                    if (pattern == null)
                        break;

                    if (pattern == "ELSE")
                    {
                        foundIndex = index + 1;
                        break;
                    }
                    else
                        break;
                }
            }

            return foundIndex;
        }

        // HELPER FUNCTIONS --------------------
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
            return (Regex.Match(name, @"^[a-zA-Z_][a-zA-z0-9_]*\s*$").Success) ? (isReserved(name))? false : true : false;
        }
        public bool isVariableReal(string name)
        {
            return (this.variableList.ContainsKey(name)) ? true : false;
        }
        public bool isDigit(string name)
        {
            return Regex.Match(name, @"-?\d+|-?\d+.\d+").Success;
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

        private List<string[]> CallMethod(string methodName, object[] args = null) // METHOD CALLER
        {
            MethodInfo method = typeof(Interpreter).GetMethod(methodName);
            return (List<string[]>) method.Invoke(this, args);
        }
        private string Pattern(string statement)
        {
            foreach (KeyValuePair<string, string> pattern in patterns)
            {
                if (Regex.Match(statement, pattern.Value).Success)
                    return pattern.Key;
            }

            return null;
        }


        public string GetUniqueId(int length = 12)
        {
            var rndDigits = new System.Text.StringBuilder().Insert(0, "0123456789", length).ToString().ToCharArray();
            string ID = string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
                
            while (subProgram.ContainsKey(ID))
                ID = string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));

            return ID;
        }


        // BOOLEAN FUNCTIONS --------------------
        public string getBoolExpression(string line)
        {
            string exp_p = @"\(.*\)\s*$";
            return Regex.Match(line, exp_p).Value.Trim();
        }
        public bool? IsTrue(string[] line)
        {
            bool? output = true;
            var stack = new Stack<string>();
            var postfix = new Stack<string>();

            try
            {
                string[] pf = (string[])line.Clone();

                if (pf == null) throw new Exception();

                var temp = new Stack<dynamic>();
                for (int i = 0; i < pf.Length; i++)
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
                            if (isDigit(x))
                                x = Single.Parse(x);
                        if (y.GetType() == typeof(string))
                            if (isDigit(y))
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
        public bool? IsTrue(string line)
        {
            bool? output = true;
            var stack = new Stack<string>();
            var postfix = new Stack<string>();

            try
            {
                string[] pf = BoolToPostfix(line);

                if (pf == null) throw new Exception();

                var temp = new Stack<dynamic>();
                for (int i = 0; i < pf.Length; i++)
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
                            if (isDigit(x))
                                x = Single.Parse(x);
                        if (y.GetType() == typeof(string))
                            if (isDigit(y))
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
        public string[] BoolToPostfix(string line)
        {
            string exp = line.Replace(" ", "");
            exp = exp.Replace(" ", "");
            exp = exp.Replace("AND", "&");
            exp = exp.Replace("OR", "|");
            exp = exp.Replace("NOT", "!");

            string pattern = @"(\&|\||\!)|([\(\)])|([\(\)])|([a-zA-Z][a-zA-Z0-9]*|-?\d+|-?\d+.\d+)|(\<\>)|(\>\=)|(\<\=)|(\=\=)|([\<\>\=])";

            string[] l = Regex.Split(exp, pattern);

            var stack = new Stack<string>();
            var postfix = new Stack<string>();

            try
            {
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
                    else if (Regex.Match(s, @"^(-?\d+|-?\d+.\d+)$").Success)
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
            }
            catch (Exception e)
            {
                return null;
            }
            return postfix.Reverse().ToArray();
        }
        private static int Priority(string c)
        {
            int p = 1;
            if (c == "!") p = -3;
            if (c == "&") p = -2;
            if (c == "|") p = -1;
            return p;
        }


        // PROPERTIES --------------------
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

    }
}
