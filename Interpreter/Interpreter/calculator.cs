using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
namespace calculator
{
    public enum OperatorType { MULTIPLY, DIVIDE, ADD, SUBTRACT, EXPONENTIAL, OPAREN, CPAREN };
    public interface Element
    {
    }
 
    public class NumberElement : Element
    {
        double number;
        public Double getNumber()
        {
            return number;
        }
 
        public NumberElement(String number)
        {
            this.number = Double.Parse(number);
        }
 
        public override String ToString()
        {
            return ((int)number).ToString();
        }
    }
 
    public class OperatorElement : Element
    {
        public OperatorType type;
        char c;
        public OperatorElement(char op)
        {
            c = op;
            if (op == '+')
                type = OperatorType.ADD;
            else if (op == '-')
                type = OperatorType.SUBTRACT;
            else if (op == '*')
                type = OperatorType.MULTIPLY;
            else if (op == '/')
                type = OperatorType.DIVIDE;
            else if (op == '^')
                type = OperatorType.EXPONENTIAL;
            else if (op == '(')
                type = OperatorType.OPAREN;
            else if (op == ')')
                type = OperatorType.CPAREN;
        }
 
        public override String ToString()
        {
            return c.ToString();
        }
    }
 
    public class Parser
    {
        List<Element> e = new List<Element>();
        public List<Element> Parse(String s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (Char.IsDigit(c))
                    sb.Append(c);
                if (i + 1 < s.Length)
                {
                    char d = s[i + 1];
                    if (Char.IsDigit(d) == false && sb.Length > 0)
                    {
                        e.Add(new NumberElement(sb.ToString()));
                        //clears stringbuilder
                        sb.Remove(0, sb.Length);
                    }
                }
 
                if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^'
                        || c == '(' || c == ')')
                    e.Add(new OperatorElement(c));
            }
            if (sb.Length > 0)
                e.Add(new NumberElement(sb.ToString()));
 
            return e;
        }
    }
 
 
    public class InfixToPostfix
    {
        List<Element> converted = new List<Element>();
        int Precedence(OperatorElement c)
        {
            if (c.type == OperatorType.EXPONENTIAL)
                return 2;
            else if (c.type == OperatorType.MULTIPLY || c.type == OperatorType.DIVIDE)
                return 3;
            else if (c.type == OperatorType.ADD || c.type == OperatorType.SUBTRACT)
                return 4;
            else
                return Int32.MaxValue;
        }
 
        public void ProcessOperators(Stack<Element> st, Element element, Element top)
        {
            while (st.Count > 0 && Precedence((OperatorElement)element) >= Precedence((OperatorElement)top))
            {
                Element p = st.Pop();
                if (((OperatorElement)p).type == OperatorType.OPAREN)
                    break;
                converted.Add(p);
                if (st.Count > 0)
                    top = st.First();
            }
        }
        public List<Element> ConvertFromInfixToPostFix(List<Element> e)
        {
            List<Element> stack1 = new List<Element>(e);
            Stack<Element> st = new Stack<Element>();
            for (int i = 0; i < stack1.Count; i++)
            {
                Element element = stack1[i];
                if (element.GetType().Equals(typeof(OperatorElement)))
                {
                    if (st.Count == 0 ||
                            ((OperatorElement)element).type == OperatorType.OPAREN)
                        st.Push(element);
                    else
                    {
                        Element top = st.First();
                        if (((OperatorElement)element).type == OperatorType.CPAREN)
                            ProcessOperators(st, element, top);
                        else if (Precedence((OperatorElement)element) < Precedence((OperatorElement)top))
                            st.Push(element);
                        else
                        {
                            ProcessOperators(st, element, top);
                            st.Push(element);
                        }
                    }
                }
                else
                    converted.Add(element);
            }
 
            //pop all operators in stack
            while (st.Count > 0)
            {
                Element b1 = st.Pop();
                converted.Add(b1);
            }
 
            return converted;
        }
 
        public override String ToString()
        {
            StringBuilder s = new StringBuilder();
            for (int j = 0; j < converted.Count; j++)
                s.Append(converted[j].ToString() + " ");
            return s.ToString();
        }
    }
 
    public class PostFixEvaluator
    {
        Stack<Element> stack = new Stack<Element>();
 
        NumberElement calculate(NumberElement left, NumberElement right, OperatorElement op)
        {
            Double temp = Double.MaxValue;
            if (op.type == OperatorType.ADD)
                temp = left.getNumber() + right.getNumber();
            else if (op.type == OperatorType.SUBTRACT)
                temp = left.getNumber() - right.getNumber();
            else if (op.type == OperatorType.MULTIPLY)
                temp = left.getNumber() * right.getNumber();
            else if (op.type == OperatorType.DIVIDE)
                temp = left.getNumber() / right.getNumber();
            else if (op.type == OperatorType.EXPONENTIAL)
                temp = Math.Pow(left.getNumber(), right.getNumber());
 
            return new NumberElement(temp.ToString());
        }
        public Double Evaluate(List<Element> e)
        {
            List<Element> v = new List<Element>(e);
            for (int i = 0; i < v.Count; i++)
            {
                Element element = v[i];
                if (element.GetType().Equals(typeof(NumberElement)))
                    stack.Push(element);
                if (element.GetType().Equals(typeof(OperatorElement)))
                {
                    NumberElement right = (NumberElement)stack.Pop();
                    NumberElement left = (NumberElement)stack.Pop();
                    NumberElement result = calculate(left, right, (OperatorElement)element);
                    stack.Push(result);
                }
            }
            return ((NumberElement)stack.Pop()).getNumber();
        }
    }
 
    class Program
    {
        public double Calculate(String s)
        {
            Parser p = new Parser();
            List<Element> e = p.Parse(s);
            InfixToPostfix i = new InfixToPostfix();
            e = i.ConvertFromInfixToPostFix(e);
 
            PostFixEvaluator pfe = new PostFixEvaluator();
            return pfe.Evaluate(e);
        }
 
        /*static void Main(string[] args)
        {
            Program c = new Program();
            double d = c.Calculate("4+6+9*8-(5*6+9)^2");
            Console.WriteLine(d);
        }*/
    }
}