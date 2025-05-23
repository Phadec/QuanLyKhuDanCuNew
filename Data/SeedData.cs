using Microsoft.AspNetCore.Identity;
using QuanLyKhuDanCu.Models;
using System;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Data
{
    public static class SeedData
    {
        public static async Task Initialize(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Create roles if they don't exist
            string[] roleNames = { "Admin", "Manager", "Staff", "Resident" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create admin user if it doesn't exist
            string adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    HoTen = "Administrator",
                    NgaySinh = new DateTime(1990, 1, 1),
                    DiaChi = "Địa chỉ quản trị viên",
                    CMND = "000000000000",
                    SoDienThoai = "0123456789",
                    NgayTao = DateTime.Now,
                    TrangThai = true,
                    EmailConfirmed = true,
                    Avatar = string.Empty // Initialize empty string for Avatar
                };

                var result = await userManager.CreateAsync(user, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
