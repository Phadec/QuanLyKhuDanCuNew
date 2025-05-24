using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

       
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                // Seed basic data (Roles, Admin User)
                await SeedData.Initialize(userManager, roleManager);

                // Seed Users (Manager, Staff, Residents) - Create UserSeeder first
                await UserSeeder.SeedUsersAsync(userManager);

                // Seed additional data
                await DichVuSeeder.SeedDichVuAsync(context);
                await LoaiPhiSeeder.SeedLoaiPhiAsync(context);
                await ThongBaoSeeder.SeedThongBaoAsync(context, userManager);

                // Seed HoKhau first, then NhanKhau
                await HoKhauSeeder.SeedHoKhauAsync(context, userManager);
                await NhanKhauSeeder.SeedNhanKhauAsync(context, userManager);

                // You can add more seeders here
            } 
        }
    }

