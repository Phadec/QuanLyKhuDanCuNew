using QuanLyKhuDanCu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace QuanLyKhuDanCu.Data.Seeders
{
    public static class NhanKhauSeeder
    {
        public static async Task SeedNhanKhauAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (await context.NhanKhaus.AnyAsync())
            {
                return; // Already seeded
            }

            var hoKhau1 = await context.HoKhaus.FirstOrDefaultAsync(h => h.MaHoKhau == "HK001");
            var hoKhau2 = await context.HoKhaus.FirstOrDefaultAsync(h => h.MaHoKhau == "HK002");

            var user1 = await userManager.FindByEmailAsync("resident1@example.com");
            var user2 = await userManager.FindByEmailAsync("resident2@example.com");

            if (hoKhau1 != null)
            {
                var nhanKhaus = new List<NhanKhau>
                {
                    new NhanKhau
                    {
                        HoTen = user1?.HoTen ?? "Nguyễn Văn A",
                        NgaySinh = new DateTime(1980, 1, 15),
                        GioiTinh = "Nam",
                        CMND = "012345678901",
                        SoDienThoai = user1?.SoDienThoai ?? "0901234567",
                        Email = user1?.Email ?? "nguyenvana@example.com",
                        QuocTich = "Việt Nam",
                        NgheNghiep = "Kỹ sư",
                        NoiLamViec = "Công ty ABC",
                        QuanHeVoiChuHo = "Chủ hộ",
                        HoKhauId = hoKhau1.HoKhauId,
                        UserId = user1?.Id,
                        NgayDangKy = DateTime.Now.AddDays(-30),
                        TrangThai = true
                    },
                    new NhanKhau
                    {
                        HoTen = "Nguyễn Thị B",
                        NgaySinh = new DateTime(1982, 5, 20),
                        GioiTinh = "Nữ",
                        CMND = "012345678902",
                        SoDienThoai = "0901234568",
                        Email = "nguyenthib@example.com",
                        QuocTich = "Việt Nam",
                        NgheNghiep = "Giáo viên",
                        NoiLamViec = "Trường tiểu học XYZ",
                        QuanHeVoiChuHo = "Vợ",
                        HoKhauId = hoKhau1.HoKhauId,
                        UserId = null,
                        NgayDangKy = DateTime.Now.AddDays(-30),
                        TrangThai = true
                    }
                };
                await context.NhanKhaus.AddRangeAsync(nhanKhaus);
            }

            if (hoKhau2 != null && user2 != null)
            {
                var nhanKhau2 = new NhanKhau
                {
                    HoTen = user2.HoTen,
                    NgaySinh = new DateTime(1975, 8, 10),
                    GioiTinh = "Nữ",
                    CMND = "012345678903",
                    SoDienThoai = user2.SoDienThoai,
                    Email = user2.Email,
                    QuocTich = "Việt Nam",
                    NgheNghiep = "Bác sĩ",
                    NoiLamViec = "Bệnh viện Đa Khoa",
                    QuanHeVoiChuHo = "Chủ hộ",
                    HoKhauId = hoKhau2.HoKhauId,
                    UserId = user2.Id,
                    NgayDangKy = DateTime.Now.AddDays(-25),
                    TrangThai = true
                };
                await context.NhanKhaus.AddAsync(nhanKhau2);
            }

            await context.SaveChangesAsync();
        }
    }
}
