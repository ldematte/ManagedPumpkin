using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Pumpkin;

namespace PumpkinTests {
    [TestClass]
    public class ResourceLimitationTests {

        [TestMethod]
        public void ForkBomb() {

            // Invoke directly, just to see what happens.
            // My (dual core) machine does notice, but the rate of creation slows down after 100 or so.
            // At ~700, it throws OutOfMemoryException
            //Snippets.ForkBomb.ThreadFunc();

        }

        [TestMethod]
        public void LimitNumberOfAllocations() {
            var snippetSource = File.ReadAllText(@"..\..\Tests\OutOfMemory.cs");
            var snippetAssembly = Pumpkin.SnippetCompiler.CompileWithCSC(snippetSource);

            var patchedAssembly = SnippetCompiler.PatchAssembly(snippetAssembly.Item1, "Snippets.OutOfMemory");

            var snippetResult = SnippetRunner.Run(patchedAssembly, "Snippets.OutOfMemory");

            Assert.IsFalse(snippetResult.Success);
            Assert.IsNotNull(snippetResult.Exception);

            Assert.AreEqual(typeof(TooManyAllocationsExceptions), snippetResult.Exception.GetType());
        }

        [TestMethod]
        public void MonitorThreadCreation() {
            var snippetSource = File.ReadAllText(@"..\..\Tests\CreateThread.cs");
            var snippetAssembly = Pumpkin.SnippetCompiler.CompileWithCSC(snippetSource);

            var patchedAssembly = SnippetCompiler.PatchAssembly(snippetAssembly.Item1, "Snippets.CreateThread");
            var snippetResult = SnippetRunner.Run(patchedAssembly, "Snippets.CreateThread");
            
            Assert.IsTrue(snippetResult.Success);
            Assert.AreEqual(1, snippetResult.Monitor.NumberOfThreadStarts);
        }
    }
}
