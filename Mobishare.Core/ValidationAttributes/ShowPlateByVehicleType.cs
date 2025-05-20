using System.ComponentModel.DataAnnotations;
using Mobishare.Core.Data;

namespace Mobishare.Core.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class UniqueVehicleTypeModelValidatorAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var dbContext = (ApplicationDbContext)validationContext
            .GetService(typeof(ApplicationDbContext));

        if (dbContext == null)
            throw new InvalidOperationException("DbContext not available.");

        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return new ValidationResult("Licence Plate is required.");

        return ValidationResult.Success;
    }
}