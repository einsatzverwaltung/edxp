using EmergencyDataExchangeProtocol.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Validation
{
    public class ObjectReferenceValidationAttribute : ValidationAttribute
    {
        private EmergencyObjectDataTypes _typ;

        public ObjectReferenceValidationAttribute(EmergencyObjectDataTypes objectType)
        {
            _typ = objectType;
        }

        protected override ValidationResult IsValid(
            object value, ValidationContext validationContext)
        {
            var val = (Guid?)validationContext.ObjectInstance;

            // TODO
            if (val.HasValue)
            {
                /* Referenz prüfen */
                //    return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }

        public EmergencyObjectDataTypes Typ => _typ;

        public string GetErrorMessage()
        {
            return $"Der Wert darf nicht kleiner sein als {_typ}.";
        }
    }
}
