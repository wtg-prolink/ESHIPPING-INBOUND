using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Business.Utils
{
    public class ValidationHelper
    {
        public static EntityValidationResult ValidateEntity<T>(T entity)
            where T : class
        {
            return new EntityValidator<T>().Validate(entity);
        }
    }
    public class EntityValidationResult
    {
        public IList<ValidationResult> Errors { get; private set; }
        public bool HasError
        {
            get { return Errors.Count > 0; }
        }

        public EntityValidationResult(IList<ValidationResult> errors = null)
        {
            Errors = errors ?? new List<ValidationResult>();
        }
    }

    public class EntityValidator<T> where T : class
    {
        public EntityValidationResult Validate(T entity)
        {
            var validationResults = new List<ValidationResult>();
            ValidationContext vc = new ValidationContext(entity, null, null);
            Validator.TryValidateObject(entity, vc, validationResults, true);
            return new EntityValidationResult(validationResults);
        }
    }

    public class EntityValidationResultException : Exception
    {
        public EntityValidationResultException(EntityValidationResult result, bool onlyShowValidationMsg=false)
        {
            Result = result;
            OnlyShowValidationMsg = onlyShowValidationMsg;
        }

        public bool OnlyShowValidationMsg
        {
            get;
            set;
        }

        public override string Message
        {
            get
            {
                if (OnlyShowValidationMsg) return ValidationMsg;
                string msg = base.Message;
                if (Result == null || !Result.HasError) return msg;
                return string.Format("{0}{1}{2}", ValidationMsg, Environment.NewLine, msg);
            }
        }

        public string ValidationMsg
        {
            get
            {
                if (Result == null || !Result.HasError) return string.Empty;
                return string.Format("Validation not match! {0}{1}", Environment.NewLine,
                    string.Join(Environment.NewLine, Result.Errors.Select(item => item.ErrorMessage)));
            }
        }

        public EntityValidationResult Result
        {
            get;
            private set;
        }
    }
}
