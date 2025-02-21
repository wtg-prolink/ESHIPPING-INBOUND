using Business.EDI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Import.DNImport
{
    class EDI : XmlEDIBase
    {
        public override XmlEDINode CreateXmlEDINode()
        {
            return new XmlEDINode("Root");
        }
    }
}
