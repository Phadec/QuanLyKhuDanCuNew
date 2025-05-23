using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using QuanLyKhuDanCu.Data.Seeders;
using QuanLyKhuDanCu.Models;
using System;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Data
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                // Seed basic data
                await SeedData.Initialize(userManager, roleManager);
                
                // Seed additional data
                await DichVuSeeder.SeedDichVuAsync(context);
                await LoaiPhiSeeder.SeedLoaiPhiAsync(context);
                await ThongBaoSeeder.SeedThongBaoAsync(context, userManager);
                
                // You can add more seeders here
            }
            catch (Exception ex)
            {
                // Get logger and log the error
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
}

// If this file exists, it should be deleted or renamed to avoid ambiguity with the new DataSeeder class in QuanLyKhuDanCu.Data.Seeders namespace
