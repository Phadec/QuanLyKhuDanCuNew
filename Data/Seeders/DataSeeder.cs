using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuanLyKhuDanCu.Models;
using System;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Data.Seeders
{
    public class DataSeeder
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            try
            {
                var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Step 1: Seed roles and admin user
                await SeedData.Initialize(userManager, roleManager);
                
                // Step 2: Seed additional users (staff and residents)
                await UserSeeder.SeedUsersAsync(userManager);
                
                // Step 3: Seed services and fee types
                await DichVuSeeder.SeedDichVuAsync(context);
                await LoaiPhiSeeder.SeedLoaiPhiAsync(context);
                
                // Step 4: Seed households and people
                await HoKhauSeeder.SeedHoKhauAsync(context, userManager);
                
                // Step 5: Seed announcements
                await ThongBaoSeeder.SeedThongBaoAsync(context, userManager);
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<DataSeeder>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
}
