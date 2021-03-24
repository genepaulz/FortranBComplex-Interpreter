using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    class Postfix
    {
        public static float QuickMath(string[] post, Interpreter n)
        {
            float ans = 0.0f;
            Stack<float> stack = new Stack<float>();

            try
            {
                foreach (string s in post)
                {
                    float value = 0.0f;
                    if (n.isVariableReal(s))
                    {
                        value = (float) n.Variables[s];
                        stack.Push(value);
                    }
                    else if ("+/*-".Contains(s))
                    {
                        float y = stack.Pop();
                        float x = stack.Pop();

                        if (s == "+")
                            value = x + y;
                        else if (s == "-")
                            value = x - y;
                        else if (s == "/")
                            value = x / y;
                        else if (s == "*")
                            value = x * y;

                        stack.Push(value);
                    }
                    else
                    {
                        value = Single.Parse(s);
                        stack.Push(value);
                    }
                }
            } catch (Exception e)
            {
                ans = 0.0f;
            }

            ans = stack.Pop();
            return ans;
        }
        public static string[] ToPostFix(string infixArray)
        {
            var stack = new Stack<char>();
            var postfix = new Stack<string>();

            string word = "";
            bool beenSpace = false;

            try
            {
                for (int i = 0; i < infixArray.Length; i++)
                {
                    char c = infixArray[i];
                    if (Char.IsWhiteSpace(c))
                    {
                        beenSpace = true;
                        continue;
                    }
                    else if (Char.IsDigit(c) | Char.IsLetter(c))
                    {
                        if(word != "" && beenSpace)
                        {
                            throw new InvalidOperationException();
                        }
                        beenSpace = false;
                        word += c;
                    }
                    else if ("+/*-".Contains(c))
                    {
                        beenSpace = false;
                        if(word != "")
                        {
                            postfix.Push(word);
                            word = "";
                        }

                        // DONT DELETE THIS.. IM STILL GOING TO WORK ON IT
                        //int front = infixArray.IndexOf(infixArray.Skip(i + 1).FirstOrDefault(k => !char.IsWhiteSpace(k)));
                        int back = infixArray.IndexOf(infixArray.Reverse().Skip(infixArray.Length - i).FirstOrDefault(k => !char.IsWhiteSpace(k)));

                        if(c == '-')
                        {
                            if (back != -1)
                            {
                                if (infixArray[back] == '(' | "+/*-".Contains(infixArray[back]))
                                {
                                    postfix.Push("0");
                                    stack.Push(c);
                                    continue;
                                }
                            }
                            else if (i == 0)
                            {
                                postfix.Push("0");
                                stack.Push(c);
                                continue;
                            }
                        }
                        

                        while (stack.Count != 0 && stack.Peek() != '(' && Priority(stack.Peek()) >= Priority(c))
                        {
                            postfix.Push(stack.Pop().ToString());
                        }
                        stack.Push(c);
                    }
                    else if (c == '(')
                    {
                        stack.Push(c);
                    }
                    else if (c == ')')
                    {
                        postfix.Push(word);
                        word = "";
                        while (stack.Peek() != '(')
                        {
                            postfix.Push(stack.Pop().ToString());
                        }
                        stack.Pop();
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }

                if (word != "") postfix.Push(word);

                while (stack.Count != 0)
                {
                    if (stack.Peek() == '(')
                    {
                        throw new InvalidOperationException();
                    }
                    else
                    {
                        postfix.Push(stack.Pop().ToString());
                    }
                }
            } catch (Exception e)
            {
                return null;
            }

            return postfix.Reverse().ToArray();
        }

        static int Priority(char c)
        {
            int p = -1;
            if (c == '*' | c == '/' | c == '%') p = 2;
            if (c == '+' | c == '-') p = 1;
            return p;
        }
    }
}
