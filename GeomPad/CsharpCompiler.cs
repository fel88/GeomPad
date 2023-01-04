using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace GeomPad
{
    public static class CsharpCompiler
    {
        public static CompilerResults CompileCodeInMem(string prog)
        {
            return CompileCodeInMem(new string[] { prog });
        }
        public static CompilerResults CompileCodeInMem(string[] prog)
        {
            var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } });

            var parameters = new CompilerParameters(new[] { "mscorlib.dll"/*, "System.Core.dll" */});

            var assemblies = AppDomain.CurrentDomain
                            .GetAssemblies()
                            .Where(a => !a.IsDynamic)
                            .Select(a => a.Location);

            parameters.ReferencedAssemblies.AddRange(assemblies.ToArray());

            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;
            parameters.IncludeDebugInformation = true;
            CompilerResults results = csc.CompileAssemblyFromSource(parameters, prog);
            
            return results;
        }
    }
}
