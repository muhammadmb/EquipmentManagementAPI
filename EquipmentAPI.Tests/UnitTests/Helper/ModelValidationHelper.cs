using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EquipmentAPI.Tests.UnitTests.Helper
{
    public static class ModelValidationHelper
    {
        public static void ValidateModel(ControllerBase controller, object model)
        {
            var validationContext = new ValidationContext(model, null, null);
            var validationResults = new List<ValidationResult>();

            Validator.TryValidateObject(model, validationContext, validationResults, true);

            foreach (var validationResult in validationResults)
            {
                var key = validationResult.MemberNames.FirstOrDefault() ?? "";
                controller.ModelState.AddModelError(key, validationResult.ErrorMessage);
            }
        }
    }
}
