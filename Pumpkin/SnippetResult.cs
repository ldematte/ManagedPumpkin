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
        private readonly Monitor monitor;

        public SnippetResult(Monitor monitor) {
            this.success = true;
            this.monitor = monitor;
        }

        public SnippetResult(Exception exception, Monitor monitor) {
            this.success = false;
            this.exception = exception;
            this.monitor = monitor;
        }

        public Exception Exception { get { return exception; } }

        public bool Success { get { return success; } }

        public IReadOnlyList<string> Output { get { return monitor.output; } }

        public Monitor Monitor { get { return monitor; } }
    }
}
