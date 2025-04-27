using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Mobishare.Core.Data;

namespace Mobishare.Core.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class UniqueCityNameAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var dbContext = (ApplicationDbContext)validationContext
            .GetService(typeof(ApplicationDbContext));

        if (dbContext == null)
            throw new InvalidOperationException("DbContext not available.");

        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return new ValidationResult("City name is required.");

        var cityName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.ToString()!);

        var instance = validationContext.ObjectInstance;
        var idProperty = instance.GetType().GetProperty("Id");
        var id = idProperty != null ? (int)(idProperty.GetValue(instance) ?? 0) : 0;

        var exists = dbContext.VehicleTypes
            .Any(v => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(v.Model) == cityName && v.Id != id);

        if (exists)
            return new ValidationResult(ErrorMessage ?? $"City '{cityName}' already exists.");

        return ValidationResult.Success;
    }
}