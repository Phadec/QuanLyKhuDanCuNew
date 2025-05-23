using Microsoft.AspNetCore.Identity;
using QuanLyKhuDanCu.Models;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Data.Seeders
{
    public static class SeedData
    {
        public static async Task Initialize(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed roles
            await SeedRoles(roleManager);
            
            // Seed admin user
            await SeedAdminUser(userManager);
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Manager", "Staff", "Resident" };
            
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
        {
            // Check if admin exists
            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            
            if (adminUser == null)
            {
                // Create admin user
                adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                    HoTen = "Administrator",
                    NgaySinh = new DateTime(1990, 1, 1),
                    DiaChi = "Admin Office",
                    CMND = "000000000000",
                    SoDienThoai = "0123456789",
                    Avatar = "default-avatar.png",
                    NgayTao = DateTime.Now,
                    TrangThai = true
                };
                
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                
                if (result.Succeeded)
                {
                    // Add to admin role
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
