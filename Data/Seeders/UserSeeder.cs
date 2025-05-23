using Microsoft.AspNetCore.Identity;
using QuanLyKhuDanCu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Data.Seeders
{
    public static class UserSeeder
    {
        public static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            if (userManager.Users.Count() > 1) // Admin already exists
            {
                return; // Already seeded
            }

            // Create demo staff users
            var staffUsers = new List<(string email, string name, string role)>
            {
                ("manager@example.com", "Quản Lý", "Manager"),
                ("staff1@example.com", "Nhân Viên 1", "Staff"),
                ("staff2@example.com", "Nhân Viên 2", "Staff")
            };

            foreach (var (email, name, role) in staffUsers)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        HoTen = name,
                        NgaySinh = new DateTime(1985, 1, 1).AddDays(new Random().Next(3650)),
                        DiaChi = "Địa chỉ " + name,
                        CMND = new Random().Next(100000000, 999999999).ToString(),
                        SoDienThoai = "0" + new Random().Next(100000000, 999999999).ToString(),
                        NgayTao = DateTime.Now.AddDays(-new Random().Next(10, 100)),
                        TrangThai = true,
                        EmailConfirmed = true,
                        Avatar = string.Empty
                    };

                    var result = await userManager.CreateAsync(user, "Password@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, role);
                    }
                }
            }

            // Create demo resident users
            var residentUsers = new List<(string email, string name)>
            {
                ("resident1@example.com", "Cư Dân 1"),
                ("resident2@example.com", "Cư Dân 2"),
                ("resident3@example.com", "Cư Dân 3"),
                ("resident4@example.com", "Cư Dân 4"),
                ("resident5@example.com", "Cư Dân 5")
            };

            foreach (var (email, name) in residentUsers)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        HoTen = name,
                        NgaySinh = new DateTime(1970, 1, 1).AddDays(new Random().Next(10950)),
                        DiaChi = "Căn hộ " + new Random().Next(100, 999),
                        CMND = new Random().Next(100000000, 999999999).ToString(),
                        SoDienThoai = "0" + new Random().Next(100000000, 999999999).ToString(),
                        NgayTao = DateTime.Now.AddDays(-new Random().Next(10, 100)),
                        TrangThai = true,
                        EmailConfirmed = true,
                        Avatar = string.Empty
                    };

                    var result = await userManager.CreateAsync(user, "Password@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Resident");
                    }
                }
            }
        }
    }
}
