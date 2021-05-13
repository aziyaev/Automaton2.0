using System;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Labyrinth
{
    public delegate double Del(double x);

    //генератор лямбда-выражений для функций
    class Functions
    {
        public static FunctionType currentFunctionType = FunctionType.EQUALS;

        private static Delegate func;

        private static string begin = @"using System;
        namespace Labyrinth
        {
                public delegate double Del(double x);
                public static class LambdaCreator{
                    public static Del Create(){
                            return (x)=>";
                            private static string end = @";} } }";

        public static bool createLambda(string function)
        {
            try
            {
                string middle = function;
                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerParameters parameters = new CompilerParameters();
                parameters.GenerateInMemory = true;
                parameters.ReferencedAssemblies.Add("System.dll");
                CompilerResults results = provider.CompileAssemblyFromSource(parameters, begin + middle + end);
                var cls = results.CompiledAssembly.GetType("Labyrinth.LambdaCreator");
                var method = cls.GetMethod("Create", BindingFlags.Static | BindingFlags.Public);
                func = (method.Invoke(null, null) as Delegate);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static double getResult(double argument)
        {
            return (double) func.DynamicInvoke(argument);
        }

        public enum FunctionType { EQUALS, GREATEN, LESS}
    }
}
