﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CourseWorkGraphDrawer
{
    public static class Parser
    {
        static Dictionary<string, int> operationOrder = new Dictionary<string, int>();
        static string[] unarOperations = {
            "Math.Log",
            "Math.Log10",
            "Math.Sin",
            "Math.Cos",
            "Math.Tan",
            "Math.Sinh",
            "Math.Cosh",
            "Math.Tanh",
            "Math.Exp",
            "Math.Abs",
            "Math.Sqrt"
        };

        static Stack<string> numbers = new Stack<string>();
        static Stack<string> operations = new Stack<string>();

        static Parser()
        {

            operationOrder.Add("+", 1);
            operationOrder.Add("-", 1);
            operationOrder.Add("*", 2);
            operationOrder.Add("/", 2);
            operationOrder.Add("^", 3);
            operationOrder.Add("Math.Abs", 3);
            operationOrder.Add("Math.Sqrt", 3);
            operationOrder.Add("Math.Sin", 3);
            operationOrder.Add("Math.Cos", 3);
            operationOrder.Add("Math.Tan", 3);
            operationOrder.Add("Math.Asin", 3);
            operationOrder.Add("Math.Acos", 3);
            operationOrder.Add("Math.Atan", 3);
            operationOrder.Add("Math.Log", 3);
            operationOrder.Add("Math.Log10", 3);
            operationOrder.Add("Math.Cosh", 3);
            operationOrder.Add("Math.Sinh", 3);
            operationOrder.Add("Math.Tanh", 3);
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
                if (p == '-')
                {
                    if ((parseArray[i + 1] >= '0' && parseArray[i + 1] <= '9') || parseArray[i + 1] == 'e' || parseArray[i + 1] == 'x' || parseArray[i + 1] == 'p')
                    {
                        if (i > 0)
                        {
                            if (parseArray[i - 1] != ')')
                            {
                                num += p;
                                continue;
                                //p = parseArray[index];
                            }
                        }
                    }
                }
                else if ((p >= '0' && p <= '9' || p == '.') || p == 'e' || p == 'x' || p == 'p')
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
                    operation = operation.Replace("arcsin", "Math.Asin");
                    operation = operation.Replace("arccos", "Math.Acos");
                    operation = operation.Replace("arctan", "Math.Atan");
                    operation = operation.Replace("sin", "Math.Sin");
                    operation = operation.Replace("cos", "Math.Cos");
                    operation = operation.Replace("tan", "Math.Tan");
                    operation = operation.Replace("sqrt", "Math.Sqrt");
                    operation = operation.Replace("abs", "Math.Abs");
                    operation = operation.Replace("exp", "Math.Exp");
                    operation = operation.Replace("log", "Math.Log");
                    operation = operation.Replace("lg", "Math.Log10");
                    operation = operation.Replace("random", "random.Next");

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
                    operations.Pop();
                    continue;
                }
                DoOperation(operations.Pop());
            }
            if (numbers.Count > 1)
            {
                DoOperation("*");
            }

            return numbers.Pop();
        }



        private static void TryPushOperation(string currentOperation)
        {
            if (currentOperation == ")")
            {
                if (operations.Peek() == "(")
                {
                    operations.Pop();
                }
                DoOperation(operations.Pop());
            }
            else if (currentOperation == "(")
            {
                operations.Push(currentOperation);
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
                        int currentPrioriry = operationOrder[currentOperation];
                        int previousPrioriry = operationOrder[operations.Peek()];
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
}