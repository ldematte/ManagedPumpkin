using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Pumpkin
{

    [SecuritySafeCritical]
    public class Monitor {

        public List<String> output = new List<String>();

        [SecuritySafeCritical]
        public void Console_WriteLine(string arg) {
            output.Add(arg);
        }

    }
}
