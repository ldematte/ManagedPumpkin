using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;

namespace Pumpkin {
    public class SnippetCompiler {

        public static Tuple<byte[], Guid> CompileWithCSC(string snippetSource) {

            var options = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
            var csc = new CSharpCodeProvider(options);

            var snippetGuid = Guid.NewGuid();

            var compilerParams = new CompilerParameters();
            compilerParams.OutputAssembly = @"C:\Temp\snippet" + snippetGuid.ToString() + ".dll";

            // Add the assemblies we use in our snippet, implicit or explicit.
            // Our own monitor always goes in
            compilerParams.ReferencedAssemblies.Add(typeof(Pumpkin.Monitor).Assembly.Location);

            var assemblyInfoCs = "[assembly: System.Security.SecurityTransparent]";

            var compilerResults = csc.CompileAssemblyFromSource(compilerParams, snippetSource, assemblyInfoCs);
            // TODO: handle compilation errors

            return Tuple.Create(File.ReadAllBytes(compilerResults.PathToAssembly), snippetGuid);
        }
    }
}
