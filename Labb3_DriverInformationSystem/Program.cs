using Labb3_DriverInformationSystem.Data;
using Labb3_DriverInformationSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Labb3_DriverInformationSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionToDb")));

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4; // Tillåter kortare lösenord
                options.Password.RequireNonAlphanumeric = false; // Kräver inte specialtecken
                options.Password.RequireUppercase = false; // Kräver inte versaler
                options.Password.RequireLowercase = false; // Kräver inte gemener
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<ChangeLogService>();
            builder.Services.AddScoped<NotificationsService>();

            var app = builder.Build();

            // Seedar roller, användare och förare
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await SeedData.InitalizeSeedingData(services);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Använd autentisering och auktorisering
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
                        
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
