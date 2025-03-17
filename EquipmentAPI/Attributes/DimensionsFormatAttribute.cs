using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EquipmentAPI.Attributes
{
    public class DimensionsFormatAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) {
                return ValidationResult.Success;
            }

            var dimensions = value.ToString();
            var regex = new Regex(@"^\d+\s*X\s*\d+\s*X\s*\d+$");

            if (!regex.IsMatch(dimensions))
            {
                return new ValidationResult("The Dimensions property must be in the format '1 X 12 X 13', with or without spaces.");
            }

            return ValidationResult.Success;
        }
    }
}
