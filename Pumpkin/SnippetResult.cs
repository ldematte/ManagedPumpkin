using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pumpkin {

    enum SnippetStatus {
        Ok,
        OutOfTime,
        TooManyThreads,
        TooMuchMemory
    }

    [Serializable]
    public class SnippetResult {

        private readonly bool success;
        private readonly Exception exception;
        private readonly List<string> output;

        public SnippetResult(List<string> output) {
            this.success = true;
            this.output = output;
        }

        public SnippetResult(Exception exception) {
            this.success = false;
            this.exception = exception;
        }

        public Exception Exception { get { return exception; } }

        public bool Success { get { return success; } }

        public IReadOnlyList<string> Output { get { return output; } }
    }
}
