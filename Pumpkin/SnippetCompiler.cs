using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

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

        public static bool CheckAssemblyAgainstWhitelist(byte[] assembly, List<ListEntry> whiteList) {
            
            // Use cecil to inspect this assembly.
            // We check if a class and/or a method is allowed.
            // We allow creation of any object, and call of any method inside the assembly itself

            using (var memoryStream = new MemoryStream(assembly)) {
                var module = ModuleDefinition.ReadModule(memoryStream);

                var ownTypes = module.Types.ToDictionary(type => type.FullName);

                foreach (var type in module.Types) {
                    foreach (var method in type.Methods) {
                        var il = method.Body.GetILProcessor();

                        var methodCallsAndConstruction = method.Body.Instructions.
                            Where(i => i.OpCode == OpCodes.Newobj || i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Calli || i.OpCode == OpCodes.Callvirt).
                            Select(i => (MethodReference)i.Operand);

                        foreach (var methodRef in methodCallsAndConstruction) {
                            var declaringType = methodRef.DeclaringType;
                            var declaringTypeAssembly = ((Mono.Cecil.AssemblyNameReference)declaringType.Scope).FullName;
                            var methodName = methodRef.Name;
                            System.Diagnostics.Debug.WriteLine("{0}:{1}::{2} - {3}", declaringTypeAssembly, declaringType.FullName, methodName, methodRef.ReturnType.FullName);

                            if (!ListEntry.Matches(whiteList, declaringTypeAssembly, declaringType.FullName, methodName))
                                return false;                            
                        }
                    }
                }
            }
            return true;
        }

       
        /*
        public static byte[] PatchAssembly(byte[] compiledSnippet, string className) {

            using (var memoryStream = new MemoryStream(compiledSnippet)) {
                var module = ModuleDefinition.ReadModule(memoryStream);

                // Retrieve the target method we want to patch
                var targetType = module.Types.Single(t => t.Name == className);
                var runMethod = targetType.Methods.Single(m => m.Name == "SnippetMain");

                // Get a ILProcessor for the method
                var il = runMethod.Body.GetILProcessor();
                // Retrieve instructions calling "Console.WriteLine"
                var callsToPatch = runMethod
                    .Body
                    .Instructions
                    .Where(i =>
                        (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt)
                        && ((MethodReference)i.Operand).DeclaringType.Name == "Console");

                // Retrieve the Console_WriteLine method
                var myConsoleWriteline = monitorType.Methods.Single(m => m.Name == "Console_WriteLine");

                // Create a new instruction to call the new method
                var patchedCall = il.Create(OpCodes.Call, myConsoleWriteline);

                // Replace the call
                foreach (var callToPatch in callsToPatch) {
                    il.Replace(callToPatch, myConsoleWriteline);
                }
                // Write the module
                using (var outputAssembly = new MemoryStream()) {
                    module.Write(outputAssembly);
                    return outputAssembly.ToArray();
                }
            }
        }*/
    }
}
