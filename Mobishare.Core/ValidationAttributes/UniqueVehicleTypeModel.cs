using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class UniqueVehicleTypeModelAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
{
    var dbContext = (ApplicationDbContext)validationContext
        .GetService(typeof(ApplicationDbContext));

    if (dbContext == null)
        throw new InvalidOperationException("DbContext not available.");

    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        return new ValidationResult("Model name is required.");

    var modelName = value.ToString()!.ToUpperInvariant();

    // 👇 Questa parte è CRUCIALE
    var instance = validationContext.ObjectInstance;
    var idProperty = instance.GetType().GetProperty("Id");
    var id = idProperty != null ? (int)(idProperty.GetValue(instance) ?? 0) : 0;

    var exists = dbContext.VehicleTypes
        .Any(v => v.Model.ToUpper() == modelName && v.Id != id);

    if (exists)
        return new ValidationResult(ErrorMessage ?? $"Model '{modelName}' already exists.");

    return ValidationResult.Success;
}
}