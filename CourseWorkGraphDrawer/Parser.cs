using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CourseWorkGraphDrawer
{
    public static class Parser
    {
        static Dictionary<string, int> operationPrioriry = new Dictionary<string, int>();
        static string[] unarOperations = {
            "abs",
            "sqrt",
            "sin",
            "cos",
            "tan",
            "asin",
            "acos",
            "atan",
            "log",
            "lg",
            "cosh",
            "sinh",
            "tanh"
        };
        static Stack<string> numbers = new Stack<string>();
        static Stack<string> operations = new Stack<string>();
        static int breaketsCount = 0;

        static Parser()
        {

            operationPrioriry.Add("+", 0);
            operationPrioriry.Add("-", 0);
            operationPrioriry.Add("*", 1);
            operationPrioriry.Add("/", 1);
            operationPrioriry.Add("^", 2);
            operationPrioriry.Add("abs", 3);
            operationPrioriry.Add("sqrt", 3);
            operationPrioriry.Add("sin", 3);
            operationPrioriry.Add("cos", 3);
            operationPrioriry.Add("tan", 3);
            operationPrioriry.Add("asin", 3);
            operationPrioriry.Add("acos", 3);
            operationPrioriry.Add("atan", 3);
            operationPrioriry.Add("log", 3);
            operationPrioriry.Add("lg", 3);
            operationPrioriry.Add("cosh", 3);
            operationPrioriry.Add("sinh", 3);
            operationPrioriry.Add("tanh", 3);
        }
        public static string Parse(string origin)
        {
            char[] parseArray;
            origin = origin.Replace(" ", "");
            parseArray = origin.ToCharArray();
            StringBuilder finalExpression = new StringBuilder();


            string num = "";
            string operation = "";

            Regex numXMultiplication = new Regex(@"([\d\.]+)[x]");
            for (int i = 0; i < parseArray.Length; i++)
            {
                char p = parseArray[i];
                if (p >= '0' && p <= '9' || p == '.' || p == 'e' || p == 'x' || p == 'p')
                {
                    num += p;
                    if (i == parseArray.Length - 1)
                    {
                        num = numXMultiplication.Replace(num, numXMultiplication.Match(num).Groups[1].Value + "*x");
                        num = num.Replace("p", "Math.PI");
                        num = num.Replace("e", "Math.E");
                        numbers.Push(num);
                    }
                }
                else if (num != "")
                {
                    num = numXMultiplication.Replace(num, numXMultiplication.Match(num).Groups[1].Value + "*x");
                    num = num.Replace("p", "Math.PI");
                    num = num.Replace("e", "Math.E");
                    numbers.Push(num);
                    num = "";
                }

                if (p >= 'a' && p <= 'z' && p != 'e' && p != 'p' && p != 'x')
                {
                    operation += p;
                }
                else if (operation != "")
                {
                    TryPushOperation(operation);
                    operation = "";
                }
                if (p == '^' || p == '+' || p == '-' || p == '*' || p == '/' || p == '(' || p == ')')
                {
                    operation = p.ToString();
                    TryPushOperation(operation);
                    operation = "";
                }
            }


            while (operations.Count > 0)
            {
                if (operations.Peek() == "(")
                {
                    numbers.Push(operations.Pop() + numbers.Pop() + ")");
                    continue;
                }
                if (numbers.Count == 1 && operations.Count == 1 && operations.Peek() == "-")
                {
                    numbers.Push(operations.Pop() + numbers.Pop());
                    continue;
                }
                DoOperation(operations.Pop());
            }
            if(breaketsCount > 0)
            {
                throw new ParserException("Слишком много открывающих скобок");
            }
            return numbers.Pop();
        }

        private static bool TryPushOperation(string currentOperation)
        {
            if (currentOperation == ")" && breaketsCount == 0)
                throw new ParserException("Слишком много закрывающих скобок");
            if (currentOperation == ")" && breaketsCount > 0)
            {
                if (operations.Peek() == "(")
                {
                    numbers.Push(operations.Pop() + numbers.Pop() + currentOperation);
                }
                DoOperation(operations.Pop());
                breaketsCount--;
            }
            else if (currentOperation == "(")
            {
                operations.Push(currentOperation);
                breaketsCount++;
            }
            else if (operations.Count > 0)
            {
                if (operations.Peek() == "(")
                {
                    operations.Push(currentOperation);
                }
                else
                {
                    if (currentOperation != ")")
                    {
                        int currentPrioriry = operationPrioriry[currentOperation];
                        int previousPrioriry = operationPrioriry[operations.Peek()];
                        if (currentPrioriry >= previousPrioriry)
                        {
                            operations.Push(currentOperation);
                        }
                        else
                        {
                            DoOperation(operations.Pop());
                            operations.Push(currentOperation);
                        }
                    }
                    else
                    {
                        if (operations.Peek() == "(")
                        {
                            operations.Pop();
                        }
                        DoOperation(operations.Pop());
                    }
                }
            }
            else
            {
                operations.Push(currentOperation);
            }
            return true;
        }

        private static void DoOperation(string operation)
        {
            string res = "";
            string csOperation = operation.ToString();
            if (operation.Contains("arcsin") || operation.Contains("arccos") || operation.Contains("arctan"))
            {
                csOperation = csOperation.Replace("arcsin", "Math.Asin");
                csOperation = csOperation.Replace("arccos", "Math.Acos");
                csOperation = csOperation.Replace("arctan", "Math.Atan");
            }
            else
            {
                csOperation = csOperation.Replace("sin", "Math.Sin");
                csOperation = csOperation.Replace("cos", "Math.Cos");
                csOperation = csOperation.Replace("tan", "Math.Tan");
            }
            csOperation = csOperation.Replace("sqrt", "Math.Sqrt");
            csOperation = csOperation.Replace("abs", "Math.Abs");
            csOperation = csOperation.Replace("log", "Math.Log");
            csOperation = csOperation.Replace("lg", "Math.Log10");
            if (unarOperations.Contains(operation))
            {
                res = $"{csOperation}({numbers.Pop()})";
            }
            else if (csOperation == "^")
            {
                string num2 = numbers.Pop();
                string num1 = numbers.Pop();
                res = $"Math.Pow({num1},{num2})";
            }
            else
            {
                string num2 = numbers.Pop();
                string num1 = numbers.Pop();
                res = $"{num1}{csOperation}{num2}";
            }
            numbers.Push(res);
        }
    }


    public class ParserException : Exception
    {
        public ParserException(string message) : base(message) { }
    }
}
