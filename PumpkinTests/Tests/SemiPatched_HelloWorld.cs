using System;
using System.IO;

namespace Snippets {

    public class SemiPatched_HelloWorld {

        // This main is already "patched" (monitor inserted as an arg) but 
        // it still uses Console.WriteLine
        public static void SnippetMain(Pumpkin.Monitor monitor) {
            Console.WriteLine("Hello world!");
        }
    }
}