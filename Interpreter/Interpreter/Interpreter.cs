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

        bool hasStarted = false;
        bool hasFinished = false;
        

        public Interpreter()
        {
            this.variableList = new Dictionary<string, dynamic>();
            this.dataTypes = new Dictionary<string, dynamic>(4);

            this.dataTypes.Add("INT", 0);
            this.dataTypes.Add("FLOAT", 0.00);
            this.dataTypes.Add("BOOL", false);
            this.dataTypes.Add("CHAR", ' ');

            var patterns = new[] {
                new {ID = "Declaration" , Pattern = @"(\bVAR)\s+([a-zA-Z_][a-zA-Z0-9_]*|[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*(([a-zA-Z_][a-zA-Z0-9_]*)|((-?\d*)|(-?\d*\.\d*))|('\w'))(\s*[+-/*]\s*(([a-zA-Z_][a-zA-Z0-9_]*)|((-?\d*)|(-?\d*\.\d*))|('\w')))*)(\s*,\s*(([a-zA-Z_][a-zA-Z0-9_]*|[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*(([a-zA-Z_][a-zA-Z0-9_]*)|((-?\d*)|(-?\d*\.\d*))|('\w'))(\s*[+-/*]\s*(([a-zA-Z_][a-zA-Z0-9_]*)|((-?\d*)|(-?\d*\.\d*))|('\w')))*)))*\s+(AS (INT|FLOAT|BOOL|CHAR)\b)"},
                new {ID = "Start", Pattern = @"^START$"},
                new {ID = "Stop", Pattern = @"^STOP$"},
                new {ID = "Input", Pattern = @"^INPUT\s*:\s*([a-zA-Z_]\w*)(\s*\,\s*[a-zA-Z_]\w*)*$"},
                new {ID = "Comment", Pattern = @"^\*.*"},
            };

            this.patterns = patterns.ToDictionary(n => n.ID, n => n.Pattern);

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
                            case "Declaration":
                                output = Declaration(line);
                                break;

                            case "Start":
                                hasStarted = true;
                                break;

                            case "Stop":
                                if (hasStarted)
                                    hasFinished = true;
                                else
                                    throw new Exception();
                                break;
                            case "Input":
                                output = Input(line);
                                break;
                               
                        }

                        return output;
                    }
                }

                throw new Exception();
                
            } 
            catch(Exception e)
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

                        if (!variableList.ContainsKey(variableName))
                        {
                            variableList.Add(variableName, dataTypes[variableType]);

                            output = Assignment(temp, variableType, variableName);
                        }
                        else
                        {
                            throw new Exception(); // VARIABLE ALREADY DECLARED
                        }
                    }
                }
                else
                {
                    throw new Exception(); // START NA PROGRAM
                }
            }
            catch(Exception e)
            {
                output = e.Message;
            }
            

            return output;
        }

        public string Assignment(string line, string variableType, string variableName)
        {
            string output = "";

            string variables_p  = @"^[a-zA-Z_][a-zA-z0-9_]*";
            string values_p     = @"(?<=\b[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*).*";
            string operators_p  = @"[*+/-]";

            string[] sets = Regex.Split(line, ",");

            var v = dataTypes[variableType];
            var total = dataTypes[variableType];

            try
            {
                foreach (string set in sets)
                {
                    string variable = Regex.Match(set, variables_p).Value;

                    string values = Regex.Match(set, values_p).Value;
                    string[] value = Regex.Split(values, operators_p);
                    string[] oper = Regex.Matches(values, operators_p)
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .ToArray();

                    int pos = -1;

                    if (v.GetType() == typeof(bool) | v.GetType() == typeof(char))
                    {
                        if (value.Length > 1) { throw new FormatException(); }
                    }

                    for (int n = 0; n < value.Length; n++)
                    {
                        string op = "";
                        string val = value[n].Trim();

                        if (isVariable(val))
                        {
                            if (isVariableReal(val))
                            {
                                v = variableList[val];
                                
                            }
                            else
                            {
                                throw new NullReferenceException();
                            }
                        }
                        else
                        {
                            if (v.GetType() == typeof(int)) { v = Int32.Parse(value[n]);}
                            else if (v.GetType() == typeof(float)) { v = Single.Parse(value[n]); }
                            else if (v.GetType() == typeof(bool)) 
                            {
                                if (value[n] == "\"TRUE\"")
                                    v = true;
                                else if (value[n] == "\"FALSE\"")
                                    v = false;
                                else
                                    throw new FormatException();
                            }
                            else { v = value[n]; }
                               
                        }

                        if (pos != -1)
                        {
                            op = oper[pos];

                            switch (op)
                            {
                                case "+":
                                    total += v; break;
                                case "-":
                                    total -= v; break;
                                case "/":
                                    total /= v; break;
                                case "*":
                                    total *= v; break;
                            }
                        }
                        else
                        {
                            total = v;
                        }

                        pos++;
                    }
                }
            } catch (Exception e)
            {
                return output = e.Message;
            }

            variableList[variableName] = total;

            return output;
        }

        public bool isVariable(string name)
        {
            return (Regex.Match(name, @"^[a-zA-Z_][a-zA-z0-9_]*").Success) ? true : false;
        }

        public bool isVariableReal(string name)
        {
            return (this.variableList.ContainsKey(name)) ? true : false;
        }

        public bool isDigit(string name)
        {
            return (Regex.Match(name, @"^[0-9]+").Success) ? true : false;
        }

        public bool isFloat(string name)
        {
            return (Regex.Match(name, @"^[0-9]+\.[0-9]+").Success) ? true : false;
        }
        public bool isBool(string name)
        {
            return (Regex.Match(name, @"^(TRUE|FALSE)$").Success) ? true : false;
        }

        public bool isChar(string name)
        {
            return (Regex.Match(name, @"^\'.?\'$").Success) ? true : false;
        }
        public string Input(string line)
        {
            string output = "";
            try
            {
                if (hasStarted)
                {
                    string[] variable_name = Regex.Matches(line, @"[^(INPUT:\,\s+)]\w*")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .ToArray();
                    
                   string input = Console.ReadLine();

                    string[] values = Regex.Matches(input, @"[^(INPUT:\,\s +)][a-zA-Z_\.\'\-\u00220-9]+")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .ToArray();

                    int i = 0; //counter

                    foreach (string item in variable_name)
                    {
                        
                        if (isVariableReal(item)) //if naa
                        {
                            var v = variableList[item];
                           /* Console.WriteLine("" + values[i]);
                            Console.WriteLine("" + v.GetType());
                            Console.WriteLine("" + v);*/
                            if (v.GetType() == typeof(int)) //kani ray mailhan
                            {
                               // var a = values[i];
                                Console.WriteLine("oh dzae int");
                                if (isDigit(values[i]))
                                {
                                    variableList[item] = Int32.Parse(values[i]);
                                    Console.WriteLine("item: " + variableList[item]);
                                    Console.WriteLine("SUCCESS!");
                                }
                                else
                                {
                                    throw new FormatException();
                                }
                            }
                            else if (isFloat(v)) // GetType not woorking pa.. string iya return --d cya musudT.T[manomanosajud]
                            {
                                Console.WriteLine("Oh float");
                                if (isFloat(values[i]))
                                {
                                    variableList[item] = Single.Parse(values[i]);
                                    Console.WriteLine("item: " + variableList[item]);
                                    Console.WriteLine("SUCCESS!");
                                }
                                else
                                {
                                    throw new FormatException();
                                }
                            }
                            else if (isChar(v)) // GetType not working pa.. string iya pud:( [manomano nlang sa:D]
                            {
                                Console.WriteLine("Oh char");
                                if (isChar(values[i]))
                                {
                                    variableList[item] = values[i];
                                    Console.WriteLine("item: " + variableList[item]);
                                    Console.WriteLine("SUCCESS!");
                                }
                                else
                                {
                                    throw new FormatException();
                                }
                            }
                            else if (v.GetType() == typeof(bool))  //nigana...Iguess
                            {
                                Console.WriteLine("Oh bool");
                                if (values[i] == "\"TRUE\"")
                                    variableList[item] = true;
                                else if (values[i] == "\"FALSE\"")
                                    variableList[item] = false;
                                else                                
                                    throw new FormatException();
                                
                                //Console.WriteLine("item: " + variableList[item]);
                                //Console.WriteLine("SUCCESS!");

                            }
                            i++;
                            
                        }
                        else
                        {
                            throw new NullReferenceException();
                        }
                    }

                }
            }
            catch (Exception e)
            {
                output = e.Message;
            }

            return output;
        }

        
    }

}
