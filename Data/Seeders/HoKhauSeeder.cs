using Microsoft.AspNetCore.Identity;
using QuanLyKhuDanCu.Data;
using QuanLyKhuDanCu.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Data.Seeders
{
    public static class HoKhauSeeder
    {
        public static async Task SeedHoKhauAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (context.HoKhaus.Any())
            {
                return; // Database has been seeded
            }

            // Get resident users
            var residents = await userManager.GetUsersInRoleAsync("Resident");
            if (residents.Count == 0)
            {
                return; // No residents to create households for
            }

            // Create households
            for (int i = 0; i < Math.Min(residents.Count, 3); i++)
            {
                var resident = residents[i];
                
                var hoKhau = new HoKhau
                {
                    MaHoKhau = $"HK{DateTime.Now.Year}{i + 1:D3}",
                    DiaChi = $"Tòa A, Căn {100 + i}, Khu dân cư ABC",
                    NgayTao = DateTime.Now.AddDays(-30),
                    ChuHoId = resident.Id,
                    GhiChu = "Hộ khẩu demo",
                    TrangThai = true
                };

                context.HoKhaus.Add(hoKhau);
                await context.SaveChangesAsync();

                // Create people for each household
                var nhanKhau = new NhanKhau
                {
                    HoTen = resident.HoTen,
                    NgaySinh = resident.NgaySinh,
                    GioiTinh = "Nam",
                    CMND = resident.CMND,
                    QuocTich = "Việt Nam",
                    NgheNghiep = "Nhân viên văn phòng",
                    NoiLamViec = "Công ty ABC",
                    QuanHeVoiChuHo = "Chủ hộ",
                    SoDienThoai = resident.SoDienThoai,
                    Email = resident.Email,
                    HoKhauId = hoKhau.HoKhauId,
                    TrangThai = true,
                    UserId = resident.Id,
                    NgayDangKy = DateTime.Now.AddDays(-30)
                };

                context.NhanKhaus.Add(nhanKhau);

                // Add family members
                var familyMember1 = new NhanKhau
                {
                    HoTen = $"Người thân 1 của {resident.HoTen}",
                    NgaySinh = DateTime.Now.AddYears(-30),
                    GioiTinh = "Nữ",
                    CMND = new Random().Next(100000000, 999999999).ToString(),
                    QuocTich = "Việt Nam",
                    NgheNghiep = "Giáo viên",
                    NoiLamViec = "Trường học XYZ",
                    QuanHeVoiChuHo = "Vợ",
                    SoDienThoai = "0" + new Random().Next(100000000, 999999999).ToString(),
                    Email = $"family1_{i}@example.com",
                    HoKhauId = hoKhau.HoKhauId,
                    TrangThai = true,
                    NgayDangKy = DateTime.Now.AddDays(-30)
                };

                context.NhanKhaus.Add(familyMember1);

                var familyMember2 = new NhanKhau
                {
                    HoTen = $"Người thân 2 của {resident.HoTen}",
                    NgaySinh = DateTime.Now.AddYears(-10),
                    GioiTinh = "Nam",
                    CMND = "",
                    QuocTich = "Việt Nam",
                    NgheNghiep = "Học sinh",
                    NoiLamViec = "Trường học ABC",
                    QuanHeVoiChuHo = "Con",
                    SoDienThoai = "",
                    Email = "",
                    HoKhauId = hoKhau.HoKhauId,
                    TrangThai = true,
                    NgayDangKy = DateTime.Now.AddDays(-30)
                };

                context.NhanKhaus.Add(familyMember2);
            }

            await context.SaveChangesAsync();
        }
    }
}
