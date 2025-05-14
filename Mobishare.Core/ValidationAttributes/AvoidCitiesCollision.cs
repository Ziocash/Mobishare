using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Mobishare.Core.Data;
using NetTopologySuite.IO;

namespace Mobishare.Core.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class AvoidCitiesCollision : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var dbContext = validationContext.GetRequiredService<ApplicationDbContext>();

        if (dbContext == null)
            throw new InvalidOperationException("DbContext not available.");
        
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return new ValidationResult("City area is required.");
        
        var property = validationContext.ObjectType.GetProperty("Id");
        var id = property?.GetValue(validationContext.ObjectInstance);
        var AllCities = dbContext.Cities;
        var reader = new WKTReader();
        var newCityGeometry = reader.Read(value.ToString());

        foreach (var city in AllCities)
        {
            if (id != null && city.Id == (int)id) continue;
            var cityGeometryToCheck = reader.Read(city.PerimeterLocation);

            bool intersects = cityGeometryToCheck.Intersects(newCityGeometry);
            if (intersects) return new ValidationResult(ErrorMessage ?? "City area intersects with an existing city.");
        }
        
        return ValidationResult.Success;
    }
}
