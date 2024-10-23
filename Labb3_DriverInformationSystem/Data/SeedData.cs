using Labb3_DriverInformationSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace Labb3_DriverInformationSystem.Data
{
    public static class SeedData
    {

        // Metod för att seeda data
        public static async Task InitalizeSeedingData(IServiceProvider serviceProvider)
        {
            // Skapa ett scope för att kunna använda serviceprovider
            using (var scope = serviceProvider.CreateScope())
            {
                // Hämta serviceprovider från scopet
                var scopedProvider = scope.ServiceProvider;
                await CreateRoles(scopedProvider);
                await CreateTestUsers(scopedProvider);
                await CreateDriversAndEvents(scopedProvider);
            }
        }

        // Metod för att skapa roller
        private static async Task CreateRoles(IServiceProvider serviceProvider)
        {
            // Hämta RoleManager från serviceprovider och skapa roller om de inte finns
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roleNames = { "Admin", "Employee" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        // Metod för att skapa testanvändare och koppla dem till roller
        private static async Task CreateTestUsers(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            var adminUser = new IdentityUser { UserName = "admin@test.com", Email = "admin@test.com" };

            // Om adminanvändaren inte finns, skapa den och koppla till Admin-rollen
            if (userManager.Users.All(u => u.UserName != adminUser.UserName))
            {
                var result = await userManager.CreateAsync(adminUser, "admin");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Skapa två testanvändare och koppla till Employee-rollen
            var employeeUserPetter = new IdentityUser { UserName = "petter@emp.com", Email = "petter@emp.com" };
            if (userManager.Users.All(u => u.UserName != employeeUserPetter.UserName))
            {
                var result = await userManager.CreateAsync(employeeUserPetter, "test");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(employeeUserPetter, "Employee");

                    var employeePetter = new Employee
                    {
                        Name = "Petter Olsson",
                        Phonenumber = "0709162764",
                        DateOfHire = DateTime.Now,
                        IdentityUserId = employeeUserPetter.Id
                    };

                    dbContext.Employees.Add(employeePetter);
                }
            }

            var employeeUserElin = new IdentityUser { UserName = "elin@emp.com", Email = "elin@emp.com" };
            if (userManager.Users.All(u => u.UserName != employeeUserElin.UserName))
            {
                var result = await userManager.CreateAsync(employeeUserElin, "test");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(employeeUserElin, "Employee");

                    var employeeElin = new Employee
                    {
                        Name = "Elin Jönsson",
                        Phonenumber = "0708123456",
                        DateOfHire = DateTime.Now,
                        IdentityUserId = employeeUserElin.Id
                    };

                    dbContext.Employees.Add(employeeElin);
                }
            }

            await dbContext.SaveChangesAsync();
        }


        // Metod för att skapa testförare och händelser
        private static async Task CreateDriversAndEvents(IServiceProvider serviceProvider)
        {
            // Hämta DbContext från serviceprovider och skapa testförare och händelser om de inte finns
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (!dbContext.Drivers.Any())
            {
                var drivers = new List<Driver>
                {
                    new Driver { Name = "Maria Andersson", LicenseNumber = "XKJ123", PhoneNumber = "0701234567", Salary = 28000, EmployeeId = 1 },
                    new Driver { Name = "Johan Svensson", LicenseNumber = "LMN456", PhoneNumber = "0707654321", Salary = 29000, EmployeeId = 2 },
                    new Driver { Name = "Anna Bergström", LicenseNumber = "PLR789", PhoneNumber = "0703334444", Salary = 30000, EmployeeId = 1 },
                    new Driver { Name = "Erik Lund", LicenseNumber = "TRE456", PhoneNumber = "0705556666", Salary = 31000, EmployeeId = 2 },
                    new Driver { Name = "Sofia Karlsson", LicenseNumber = "YUI678", PhoneNumber = "0707778888", Salary = 32000, EmployeeId = 1 },
                    new Driver { Name = "Daniel Norén", LicenseNumber = "VBN123", PhoneNumber = "0709990000", Salary = 33000, EmployeeId = 2 },
                    new Driver { Name = "Linda Olsson", LicenseNumber = "QWE345", PhoneNumber = "0701112222", Salary = 34000, EmployeeId = 1 },
                    new Driver { Name = "Oskar Persson", LicenseNumber = "ASD678", PhoneNumber = "0702223333", Salary = 35000, EmployeeId = 2 }
                };

                dbContext.Drivers.AddRange(drivers);
                await dbContext.SaveChangesAsync();
            }

            if (!dbContext.Events.Any())
            {
                var events = new List<Event>
                {
                    new Event { Title = "Tankning", Description = "Tanka bilen", EventDate = DateTime.Now, Expense = 500, DriverId = 1 },
                    new Event { Title = "Däckbyte", Description = "Byte av däck", EventDate = DateTime.Now, Expense = 1200, DriverId = 2 },
                    new Event { Title = "Service", Description = "Årlig service", EventDate = DateTime.Now.AddMonths(-3), Expense = 3000, DriverId = 3 },
                    new Event { Title = "Reparation", Description = "Motorproblem", EventDate = DateTime.Now.AddMonths(-2), Expense = 5000, DriverId = 4 },
                    new Event { Title = "Tvätt", Description = "Tvätta bilen", EventDate = DateTime.Now.AddMonths(-1), Expense = 100, DriverId = 5 },
                    new Event { Title = "Parkering", Description = "Betala för parkering", EventDate = DateTime.Now.AddDays(-10), Expense = 200, DriverId = 6 },
                    new Event { Title = "Försäkring", Description = "Årsförsäkring", EventDate = DateTime.Now.AddMonths(-6), Expense = 8000, DriverId = 7 },
                    new Event { Title = "Olje byte", Description = "Byta olja", EventDate = DateTime.Now.AddMonths(-2), Expense = 600, DriverId = 8 },
                    new Event { Title = "Vindrutetorkare", Description = "Byta vindrutetorkare", EventDate = DateTime.Now.AddDays(-5), Expense = 300, DriverId = 1 },
                    new Event { Title = "Stenskott", Description = "Laga stenskott", EventDate = DateTime.Now.AddMonths(-1), Expense = 1500, DriverId = 2 },
                    new Event { Title = "Byta batteri", Description = "Nytt bilbatteri", EventDate = DateTime.Now.AddMonths(-3), Expense = 2000, DriverId = 3 },
                    new Event { Title = "Bilbesiktning", Description = "Årlig bilbesiktning", EventDate = DateTime.Now.AddDays(-20), Expense = 500, DriverId = 4 },

                    new Event { Title = "Transportuppdrag", Description = "Körning för kund A", EventDate = DateTime.Now, Income = 2000, DriverId = 1 },
                    new Event { Title = "Privat körning", Description = "Chaufför för bröllop", EventDate = DateTime.Now, Income = 1500, DriverId = 2 },
                    new Event { Title = "Leverans", Description = "Leverans till företag B", EventDate = DateTime.Now.AddMonths(-1), Income = 3000, DriverId = 3 },
                    new Event { Title = "Specialkörning", Description = "Transport av känslig utrustning", EventDate = DateTime.Now.AddDays(-5), Income = 2500, DriverId = 4 },
                    new Event { Title = "VIP-kund", Description = "Körning för VIP-kund", EventDate = DateTime.Now.AddDays(-3), Income = 4000, DriverId = 5 },
                    new Event { Title = "Storkontrakt", Description = "Transport för företag C", EventDate = DateTime.Now.AddDays(-15), Income = 5000, DriverId = 6 },
                    new Event { Title = "Långdistanskörning", Description = "Transport till annan stad", EventDate = DateTime.Now.AddMonths(-2),Income = 3500, DriverId = 7 },
                    new Event { Title = "Returkörning", Description = "Transport för återvändande gods", EventDate = DateTime.Now.AddMonths(-3), Expense = 0, Income = 1800, DriverId = 8 }
                };

                dbContext.Events.AddRange(events);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
