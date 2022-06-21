using System;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace CourseWorkGraphDrawer
{
    public static class Compiler
    {
        readonly static string CodeTemplate = @"
using System;
class Executor
{
    private static Random random = new Random();
    public static double Execute(double x)
    {
        return -1 *(<code>);
    }
}
";

        public static Func<double, double> GetDelegate(string code)
        {
            code = Parser.Parse(code);
            CompilerParameters options = new CompilerParameters
            {
                GenerateInMemory = true
            };
            CompilerResults result = new CSharpCodeProvider().CompileAssemblyFromSource(options, CodeTemplate.Replace("<code>", code));
            if (!result.Errors.HasErrors)
                return (Func<double, double>)Delegate.CreateDelegate(
                    typeof(Func<double, double>),
                    result.CompiledAssembly.GetType("Executor").GetMethod(
                        "Execute",
                      BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public));
            throw new CompileException();
        }
    }
    public class CompileException : Exception { }
}
