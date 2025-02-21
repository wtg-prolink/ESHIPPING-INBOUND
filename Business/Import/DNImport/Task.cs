using Prolink.Task;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Business.Import.DNImport
{
    class Task : IPlanTask
    {
        public void Run(IPlanTaskMessenger messenger)
        {
            EDIStructures.Class1 c = new EDIStructures.Class1();
            c.TestSend();
            c.TestSendList();
        }
    }
}
