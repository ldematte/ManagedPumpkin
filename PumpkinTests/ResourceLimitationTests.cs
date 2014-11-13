using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
