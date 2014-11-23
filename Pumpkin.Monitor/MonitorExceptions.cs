using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;

namespace Pumpkin {

    [Serializable]
    [SecuritySafeCritical]
    public class TooManyAllocationsExceptions : Exception {

        public TooManyAllocationsExceptions() { }
        public TooManyAllocationsExceptions(SerializationInfo info, StreamingContext context)
            : base (info, context) { }
    }
}
