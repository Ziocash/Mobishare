using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Mobishare.Core.Data;
using NetTopologySuite.IO;

namespace Mobishare.Core.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class HasParkingInCity : ValidationAttribute
{
    protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
    {
        var dbContext = validationContext.GetRequiredService<ApplicationDbContext>();

        if (dbContext == null)
            throw new InvalidOperationException("DbContext not available.");

        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return new ValidationResult("Prking slot is required.");

        // var property = validationContext.ObjectType.GetProperty("Id");
        // var id = property?.GetValue(validationContext.ObjectInstance);
        var AllCities = dbContext.Cities;
        var reader = new WKTReader();
        var newParkingSlotGeometry = reader.Read(value.ToString());

        foreach (var city in AllCities)
        {
            var cityGeometryToCheck = reader.Read(city.PerimeterLocation);

            bool contained = cityGeometryToCheck.Contains(newParkingSlotGeometry);
            if (contained) return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage ?? "Parking slot area is not in the city.");
    }
}
