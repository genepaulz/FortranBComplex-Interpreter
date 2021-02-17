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

        public Interpreter()
        {
            this.variableList = new Dictionary<string, dynamic>();
            this.dataTypes = new Dictionary<string, dynamic>(4);

            this.dataTypes.Add("INT", 0);
            this.dataTypes.Add("FLOAT",0.00);
            this.dataTypes.Add("BOOL", false);
            this.dataTypes.Add("CHAR", ' ');

            var patterns = new[] { 
                new {ID = "Declaration" , Pattern = @"(\bVAR)\s+([a-zA-Z_][a-zA-Z0-9_]*|[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*(([a-zA-Z_][a-zA-Z0-9_]*)|((-?\d*)|(-?\d*\.\d*))|("".+"")|('\w'))(\s*[+-/*]\s*(([a-zA-Z_][a-zA-Z0-9_]*)|((-?\d*)|(-?\d*\.\d*))|("".+"")|('\w')))*)(\s*,\s*(([a-zA-Z_][a-zA-Z0-9_]*|[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*(([a-zA-Z_][a-zA-Z0-9_]*)|((-?\d*)|(-?\d*\.\d*))|("".+"")| ('\w'))(\s*[+-/*]\s*(([a-zA-Z_][a-zA-Z0-9_]*)|((-?\d*)|(-?\d*\.\d*))|("".+"")| ('\w')))*)))*\s+(AS (INT|FLOAT|BOOL|CHAR)\b)"},
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
        public string Interpret(string line)
        {
            string output = "";

            foreach(KeyValuePair<string,string> pattern in this.patterns)
            {
                Match match = Regex.Match(line, pattern.Value);

                if (match.Success)
                {
                    MethodInfo info = this.GetType().GetMethod(pattern.Key);
                    output = (string)info.Invoke(this, new object[] {line});
                } else
                {
                    output = "Exception Error: Unsa mana imo ge pangita uy";
                }
            }

            return output;
        }

        public string Input(string line)
        {
            string output = "";


            return output;
        }
        public string Declaration(string line)
        {
            string output = "";

            if (!hasStarted)
            {
                string trimmer_p    = @"(?<=\bVAR\s+).*(?=AS\s*(INT|FLOAT|BOOL|CHAR))";
                string variables_p  = @"^[a-zA-Z_][a-zA-z0-9_]*";

                string trimmed = Regex.Match(line,trimmer_p).Value;
                string[] sets = Regex.Split(trimmed, ",");


                foreach(string set in sets)
                {
                    string variable_name = Regex.Match(set, variables_p).Value;

                    if (!variableList.ContainsKey(variable_name))
                    {
                        variableList.Add(variable_name, null);

                    } else
                    {
                        output = "Exception Error: Naa naman ni na Vars Bads";
                        break;
                    }
                }

            } else
            {
                output = "Exception Error: Nag Start naman ang Program eyy!";
            }

            return output;
        }

        public string Assignment(string line)
        {
            string output = "";

            string variables_p      = @"^[a-zA-Z_][a-zA-z0-9_]*";
            string variablesType_p  = @"\b(INT|FLOAT|BOOL|CHAR)\b";
            string values_p         = @"(?<=\b[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*).*";
            string operators_p      = @"[*+/-]";


            string variable_type = Regex.Match(line, variablesType_p).Value;

            if (!hasStarted)
            {

                string trimmer_p = @"(?<=\bVAR\s+).*(?=AS\s*(INT|FLOAT|BOOL|CHAR))";

                string trimmed = Regex.Match(line, trimmer_p).ToString();
                string[] sets = Regex.Split(trimmed, ",");

                foreach(string set in sets)
                {
                    string variable = Regex.Match(set, variables_p).Value;
                    string values = Regex.Match(set, values_p).Value;

                    string[] value = Regex.Split(values, operators_p);
                    string[] oper = Regex.Matches(values, operators_p)
                        .OfType<Match>()
                        .Select(m => m.Groups[0].Value)
                        .ToArray();

                    var v = dataTypes[variable_type];

                    if (value.Length > 0)
                    {
                        string val = value[0];

                        if (Regex.Match(val, variables_p).Success)
                        {
                            v = (variableList.ContainsKey(val)) ? variableList[val] : null;

                            if (v == null)
                            {
                                // REMOVE ITEM
                                variableList.Remove(variable);
                                output = "Exception Error: Dile man ata ni buhi ang variable";
                                break;
                            }
                        }
                        else
                        {
                            try
                            {
                                if (v.GetInfo() == typeof(int))
                                {
                                    v = Int32.Parse(val);
                                }
                                else if (v.GetInfo() == typeof(float))
                                {
                                    v = Single.Parse(val);
                                }
                                else
                                {
                                    v = val;
                                }
                            }
                            catch (Exception)
                            {
                                output = "Exception Error: Sayop Data type da";
                                break;
                            }
                        }

                        if(value.Length > 1)
                        {
                            int pos = 1;
                            for(int n = 0; n < oper.Length; n++)
                            {

                            }
                        }
                    }
                }

            } else
            {

            }

            return output;
        }
    }
}
