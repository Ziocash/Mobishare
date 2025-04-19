using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.Models.Users;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Balance> Balances { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<ParkingSlot> ParkingSlots { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Repair> Repairs { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Ride> Rides { get; set; }
        public DbSet<RepairAssignment> RepairAssignments { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
    }
}