using Microsoft.AspNetCore.Identity;
using QuanLyKhuDanCu.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Data.Seeders
{
    public static class UserSeeder
    {
        public static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            // Create resident users if they don't exist
            var residents = new[]
            {
                new { Email = "resident1@example.com", HoTen = "Nguyễn Văn A", Phone = "0901234567" },
                new { Email = "resident2@example.com", HoTen = "Trần Thị B", Phone = "0901234568" },
                new { Email = "resident3@example.com", HoTen = "Lê Văn C", Phone = "0901234569" }
            };

            foreach (var residentInfo in residents)
            {
                var existingUser = await userManager.FindByEmailAsync(residentInfo.Email);
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = residentInfo.Email,
                        Email = residentInfo.Email,
                        HoTen = residentInfo.HoTen,
                        NgaySinh = new DateTime(1990, 1, 1),
                        DiaChi = "Địa chỉ tạm thời",
                        CMND = DateTime.Now.Ticks.ToString().Substring(0, 12),
                        SoDienThoai = residentInfo.Phone,
                        NgayTao = DateTime.Now,
                        TrangThai = true,
                        EmailConfirmed = true,
                        Avatar = string.Empty
                    };

                    var result = await userManager.CreateAsync(user, "Resident@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Resident");
                    }
                }
            }
        }
    }
}
