using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Validation
{
    public class LowerBoundValidationAttribute : ValidationAttribute
    {
        private double _lb;

        public LowerBoundValidationAttribute(double lowerBound)
        {
            _lb = lowerBound;
        }

        protected override ValidationResult IsValid(
            object value, ValidationContext validationContext)
        {
            var val = (double)validationContext.ObjectInstance;
            
            if (val < _lb)
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }

        public double LowerBound => _lb;

        public string GetErrorMessage()
        {
            return $"Der Wert darf nicht kleiner sein als {_lb}.";
        }
    }
}

