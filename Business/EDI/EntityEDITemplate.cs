using Business.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.EDI
{
    public abstract class EntityEDITemplate : XmlEDITemplateBase
    {
        public virtual bool Check(out EntityValidationResult result)
        {
            result = ValidationHelper.ValidateEntity(this);
            return !result.HasError;
        }
    }
}
