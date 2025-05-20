using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Mobishare.Core.Data;
using NetTopologySuite.IO;

namespace Mobishare.Core.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class AvoidParkingSlotCollision : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var dbContext = validationContext.GetRequiredService<ApplicationDbContext>();

        if (dbContext == null)
            throw new InvalidOperationException("DbContext not available.");

        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return new ValidationResult("Parking Slot area is required.");

        var property = validationContext.ObjectType.GetProperty("Id");
        var id = property?.GetValue(validationContext.ObjectInstance);
        var AllParkingSlots = dbContext.ParkingSlots;
        var reader = new WKTReader();
        var newParkingSlotGeometry = reader.Read(value.ToString());

        foreach (var parkingSlot in AllParkingSlots)
        {
            if (id != null && parkingSlot.Id == (int)id) continue;
            var parkingSlotGeometryToCheck = reader.Read(parkingSlot.PerimeterLocation);

            bool intersects = parkingSlotGeometryToCheck.Intersects(newParkingSlotGeometry);
            if (intersects) return new ValidationResult(ErrorMessage ?? "Parking Slot area intersects with an existing parking slot.");
        }

        return ValidationResult.Success;
    }
}
