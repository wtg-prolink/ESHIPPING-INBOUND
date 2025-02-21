using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Business.Attribute
{
    public class RequiredIfAttribute : ValidationAttribute
    {
        private RequiredAttribute innerAttribute = new RequiredAttribute();
        public string DependentProperty { get; set; }
        public object TargetValue { get; set; }

        public RequiredIfAttribute(string dependentProperty, object targetValue)
        {
            this.DependentProperty = dependentProperty;
            this.TargetValue = targetValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var field = validationContext.ObjectInstance.GetType().GetProperty(DependentProperty);
            if (field != null)
            {
                var dependentValue = field.GetValue(validationContext.ObjectInstance, null);
                if (Object.Equals(dependentValue, TargetValue))
                {
                    if (!innerAttribute.IsValid(value))
                        return new ValidationResult(ErrorMessage);
                }
            }
            return ValidationResult.Success;
        }
    }
}
