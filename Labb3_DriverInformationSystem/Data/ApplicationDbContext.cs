using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Labb3_DriverInformationSystem.Models;

namespace Labb3_DriverInformationSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<ChangeLog> ChangeLogs { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Definiera relation mellan Employee och Driver (en till många)
            modelBuilder.Entity<Driver>()
                .HasOne(d => d.Employee)
                .WithMany(e => e.Drivers)
                .HasForeignKey(d => d.EmployeeId)
                // Om en anställd tas bort, förhindra att förare tas bort
                .OnDelete(DeleteBehavior.Restrict);

            // Seed-data för roller
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Name = "Employee", NormalizedName = "EMPLOYEE" }
            );
        }
    }
}
