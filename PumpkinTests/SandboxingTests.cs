using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace Pumpkin {
    [TestClass]
    public class SandboxingTests {

        [TestMethod]
        public void AlreadyPatched_ReadFromLocalFails() {
            var snippetSource = File.ReadAllText(@"..\..\Tests\Patched_FileRead.cs");
            var snippetAssembly = Pumpkin.SnippetCompiler.CompileWithCSC(snippetSource);

            var snippetResult = SnippetRunner.Run(snippetAssembly.Item1, "Snippets.Patched_FileRead");

            Assert.IsFalse(snippetResult.Success);
            Assert.IsNotNull(snippetResult.Exception);

            Assert.AreEqual(typeof(System.Security.SecurityException), snippetResult.Exception.GetType());

            var se = (System.Security.SecurityException)snippetResult.Exception;
            Assert.AreEqual(typeof(System.Security.Permissions.FileIOPermission), se.FirstPermissionThatFailed.GetType());
        }

        [TestMethod]
        public void AlreadyPatched_HelloWorld() {
            var snippetSource = File.ReadAllText(@"..\..\Tests\Patched_HelloWorld.cs");
            var snippetAssembly = Pumpkin.SnippetCompiler.CompileWithCSC(snippetSource);

            var snippetResult = SnippetRunner.Run(snippetAssembly.Item1, "Snippets.Patched_HelloWorld");

            Assert.IsTrue(snippetResult.Success);
            Assert.IsNull(snippetResult.Exception);

            Assert.AreEqual("Hello world!", snippetResult.Output.FirstOrDefault());
        }

        [TestMethod]
        public void SandboxShouldRegisterExceptions() {
            var snippetSource = File.ReadAllText(@"..\..\Tests\ThrowException.cs");
            var snippetAssembly = Pumpkin.SnippetCompiler.CompileWithCSC(snippetSource);

            var patchedAssembly = SnippetCompiler.PatchAssembly(snippetAssembly.Item1, "Snippets.ThrowException");
            File.WriteAllBytes(@"C:\Temp\out.dll", patchedAssembly);

            var snippetResult = SnippetRunner.Run(patchedAssembly, "Snippets.ThrowException");

            Assert.IsFalse(snippetResult.Success);
            Assert.IsNotNull(snippetResult.Exception);

            Assert.AreEqual(typeof(Exception), snippetResult.Exception.GetType());
            Assert.AreEqual("TEST", snippetResult.Exception.Message);
        }
    }
}
