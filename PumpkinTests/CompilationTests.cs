using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pumpkin;
using System.IO;

namespace PumpkinTests {
    [TestClass]
    public class CompilationTests {
        [TestMethod]
        public void CSharpProviderAssembliesShouldBeTransparent() {

            var snippetSource = File.ReadAllText(@"..\..\Tests\FileRead.cs");
            var snippetAssembly = Pumpkin.SnippetCompiler.CompileWithCSC(snippetSource);

            var assembly = System.Reflection.Assembly.Load(snippetAssembly.Item1);
            Assert.IsTrue(assembly.GetType("Snippets.FileRead").GetMethod("SnippetMain").IsSecurityTransparent);
        }
    }
}
