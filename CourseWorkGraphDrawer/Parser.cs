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
            "Math.Abs",
            "Math.Sqrt",
            "Math.Sin",
            "Math.Cos",
            "Math.Tan",
            "Math.Asin",
            "Math.Acos",
            "Math.Atan",
            "Math.Log",
            "Math.Log10",
            "Math.Cosh",
            "Math.Sinh",
            "Math.Tanh"
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
            operationPrioriry.Add("Math.Abs", 3);
            operationPrioriry.Add("Math.Sqrt", 3);
            operationPrioriry.Add("Math.Sin", 3);
            operationPrioriry.Add("Math.Cos", 3);
            operationPrioriry.Add("Math.Tan", 3);
            operationPrioriry.Add("Math.Asin", 3);
            operationPrioriry.Add("Math.Acos", 3);
            operationPrioriry.Add("Math.Atan", 3);
            operationPrioriry.Add("Math.Log", 3);
            operationPrioriry.Add("Math.Log10", 3);
            operationPrioriry.Add("Math.Cosh", 3);
            operationPrioriry.Add("Math.Sinh", 3);
            operationPrioriry.Add("Math.Tanh", 3);
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
                    if (operation.Contains("arcsin") || operation.Contains("arccos") || operation.Contains("arctan"))
                    {
                        operation = operation.Replace("arcsin", "Math.Asin");
                        operation = operation.Replace("arccos", "Math.Acos");
                        operation = operation.Replace("arctan", "Math.Atan");
                    }
                    else
                    {
                        operation = operation.Replace("sin", "Math.Sin");
                        operation = operation.Replace("cos", "Math.Cos");
                        operation = operation.Replace("tan", "Math.Tan");
                    }
                    operation = operation.Replace("sqrt", "Math.Sqrt");
                    operation = operation.Replace("abs", "Math.Abs");
                   
                    operation = operation.Replace("log", "Math.Log");
                    operation = operation.Replace("lg", "Math.Log10");
                    

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
            if (unarOperations.Contains(operation))
            {
                res = $"{operation}({numbers.Pop()})";
            }
            else if (operation == "^")
            {
                string num2 = numbers.Pop();
                string num1 = numbers.Pop();
                res = $"Math.Pow({num1},{num2})";
            }
            else
            {
                string num2 = numbers.Pop();
                string num1 = numbers.Pop();
                res = $"{num1}{operation}{num2}";
            }
            numbers.Push(res);
        }
    }


    public class ParserException : Exception
    {
        public ParserException(string message) : base(message) { }
    }
}
