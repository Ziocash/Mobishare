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
        public DbSet<Balance> Balance { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<ParkingSlot> ParkingSlot { get; set; }
        public DbSet<Vehicle> Vehicle { get; set; }
        public DbSet<Repair> Repair { get; set; }
        public DbSet<Report> Report { get; set; }
        public DbSet<Ride> Ride { get; set; }
        public DbSet<RepairAssignment> RepairAssignment { get; set; }

    }
}