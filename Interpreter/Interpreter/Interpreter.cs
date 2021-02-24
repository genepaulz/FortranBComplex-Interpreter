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
                new {ID = "Declaration" , Pattern = @"(\bVAR)\s+([a-zA-Z_][a-zA-Z0-9_]*|[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*(([a-zA-Z_][a-zA-Z0-9_]*)|((-?\d*)|(-?\d*.\d*))|("".+"")|('\w')))(\s*,\s*(([a-zA-Z_][a-zA-Z0-9_]*|([a-zA-Z_][a-zA-Z0-9_]*|[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*(([a-zA-Z_][a-zA-Z0-9_]*)|((-?\d*)|(-?\d*.\d*))|("".+"")|('\w'))))))*\s+(AS (INT|FLOAT|BOOL|CHAR)\b)"},
                new {ID = "Assignment", Pattern = @"[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*(([a-zA-Z_][a-zA-Z0-9_]*)|((-?\d*)|(-?\d*\.\d*))|('\w'))(\s*[+-/*]\s*(([a-zA-Z_][a-zA-Z0-9_]*)|((-?\d*)|(-?\d*\.\d*))|('\w')))*"},
                new {ID = "Input", Pattern = @"^INPUT\s*:\s*([a-zA-Z_]\w*)(\s*\,\s*[a-zA-Z_]\w*)*$"},
                new {ID = "Output", Pattern = @"^OUTPUT\s*:\s.*$"},
                new {ID = "Start", Pattern = @"^START$"},
                new {ID = "Stop", Pattern = @"^STOP$"},
                new {ID = "Comment", Pattern = @"^\*.*"}
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
                            case "Declaration": output = Declaration(line); break;
                            case "Assignment": output = Assignment(line); break;
                            case "Input": output = Input(line); break;
                            case "Output": output = Output(line); break;

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

                        if (!variableList.ContainsKey(variableName))
                        {
                            variableList.Add(variableName, dataTypes[variableType]);

                            output = Assignment(temp, variableType, variableName);
                        }
                        else
                        {
                            variableList.Remove(variableName);
                            throw new Exception(); // VARIABLE ALREADY DECLARED
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

        public string Assignment(string line, string variableType = "", string variableName = "")
        {
            string output = "";

            string variables_p = @"^[a-zA-Z_][a-zA-z0-9_]*";
            string values_p = @"(?<=\b[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*).*";
            string operators_p = @"[*+/-]";

            string[] sets = Regex.Split(line, ",");

            try
            {
                if (variableName == "" && variableType == "")
                {
                    variableName = Regex.Match(line, variables_p).Value;

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
                var total = dataTypes[variableType];

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

                    if (value[0] != "")
                    {
                        if (v.GetType() == typeof(bool) | v.GetType() == typeof(char))
                        {
                            if (value.Length > 1) { throw new FormatException(); }
                        }

                        foreach (string target in value)
                        {
                            string op = "";
                            string val = target.Trim();

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
                                if (v.GetType() == typeof(int)) { v = Int32.Parse(target); }
                                else if (v.GetType() == typeof(float)) { v = Single.Parse(target); }
                                else if (v.GetType() == typeof(bool))
                                {
                                    if (target == "\"TRUE\"")
                                        v = true;
                                    else if (target == "\"FALSE\"")
                                        v = false;
                                    else
                                        throw new FormatException();
                                }
                                else { if (target.Length > 0) v = target[1]; }
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
                }

                variableList[variableName] = total;

                return output;
            }
            catch (Exception e)
            {
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

                    if (areAllVariable(variables))
                    {
                        if (areAllVariableReal(variables))
                        {
                            var input = Console.ReadLine();
                            string[] inputs = Regex.Matches(input, @"[^(\,\s +)][a-zA-Z_\.\'\-\u00220-9]*") // get muna kase the inputs
                            .Cast<Match>()
                            .Select(m => m.Value)
                            .ToArray();
                            int counter = 0;

                            foreach (string variable in variables)
                            {
                                var v = variableList[variable];
                                var i = inputs[counter];

                                if (isInt(v))
                                {
                                    v = Int32.Parse(i);
                                }
                                else if (isFloat(v))
                                {
                                    v = Single.Parse(i);
                                }
                                else if (isBool(v))
                                {

                                    if (i == "TRUE")
                                        v = true;
                                    else if (i == "FALSE")
                                        v = false;
                                    else
                                        throw new InvalidCastException();
                                }
                                else if (isChar(v))
                                {
                                    if (Regex.Match(i, @"^\w$").Success)
                                    {
                                        v = i;
                                    }
                                    else
                                    {
                                        throw new FormatException();
                                    }
                                }
                                variableList[variable] = v;
                                counter++;
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

        public string Output(string line)
        {
            string output = "";
            string outputString = "";
            /*string de = "\nDebug\n";
            string bug = "\nIDebug\n";*/
            try
            {
                if (hasStarted)
                {
                    string[] inputs = Regex.Matches(line, @"[^(OUTPUT:)\s](\""(.*?)\"")|[^(OUTPUT:)\s](\S*)")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .ToArray();
                    foreach (string input in inputs)
                    {
                        //bug += input + "\n";
                        if (isVariableReal(input))
                        {
                            outputString += "" + variableList[input] + "";
                        }
                        else if (input == "&")
                        {
                            continue;
                        }
                        else if (Regex.IsMatch(input, @"(\""(.*?)\"")|(\S*)"))
                        {
                            if (input.Contains("#"))
                            {
                                if (input.Contains("["))
                                {
                                    outputString += "" + input[2] + "";
                                }
                                else
                                {
                                    outputString += "\n";
                                }
                            }
                            else if (input.Contains("["))
                            {
                                outputString += "" + input[2] + "";
                            }
                            else
                            {
                                outputString += "" + input + "";
                            }
                            //de += buffer + "\n";
                        }
                        else
                        {
                            throw new NullReferenceException();
                        }

                    }
                    Console.WriteLine(outputString);
                    /*Console.WriteLine(bug);
                    Console.WriteLine(de);*/
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

        public bool areAllVariableReal(string[] variables)
        {
            bool flag = true;
            foreach (string variable in variables)
            {

                if (!isVariableReal(variable))
                {
                    flag = false;
                }

            }
            return flag;
        }

        public bool areAllVariable(string[] variables)
        {
            bool flag = true;
            foreach (string variable in variables)
            {
                if (!isVariable(variable))
                {
                    flag = false;
                }
            }
            return flag;
        }


        public bool isVariable(string name)
        {
            return (Regex.Match(name, @"^[a-zA-Z_][a-zA-z0-9_]*").Success) ? true : false;
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
