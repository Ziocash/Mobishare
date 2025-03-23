using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mobishare.Core.Models.User;

namespace Mobishare.Core.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Balances> Balances { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
    }
}