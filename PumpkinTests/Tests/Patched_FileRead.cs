using System;
using System.IO;

namespace Snippets {

    public class Patched_FileRead {

        public static void SnippetMain(Pumpkin.Monitor monitor) {
            File.ReadAllText("C:\\Temp\\file.txt");
        }
    }
}