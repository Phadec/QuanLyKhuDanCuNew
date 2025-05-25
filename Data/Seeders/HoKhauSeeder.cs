using QuanLyKhuDanCu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace QuanLyKhuDanCu.Data.Seeders
{
    public static class HoKhauSeeder
    {        public static async Task SeedHoKhauAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (await context.HoKhaus.AnyAsync())
            {
                return; // Already seeded
            }

            var resident1 = await userManager.FindByEmailAsync("resident1@example.com");
            var resident2 = await userManager.FindByEmailAsync("resident2@example.com");
            var admin = await userManager.FindByEmailAsync("admin@example.com");

            if (resident1 == null) resident1 = admin;
            if (resident2 == null) resident2 = admin;

            if (resident1 != null)
            {
                var hoKhaus = new List<HoKhau>
                {
                    new HoKhau
                    {
                        MaHoKhau = "HK001",
                        ChuHoId = resident1.Id,
                        DiaChi = "Căn hộ A101, Tòa nhà CT1",
                        NgayTao = DateTime.Now.AddDays(-30),
                        TrangThai = true,
                        GhiChu = "Hộ gia đình đầy đủ"
                    }
                };

                if (resident2 != null)
                {
                    hoKhaus.Add(new HoKhau
                    {
                        MaHoKhau = "HK002", 
                        ChuHoId = resident2.Id,
                        DiaChi = "Căn hộ B205, Tòa nhà CT2",
                        NgayTao = DateTime.Now.AddDays(-25),
                        TrangThai = true,
                        GhiChu = "Hộ gia đình mới"
                    });
                }

                await context.HoKhaus.AddRangeAsync(hoKhaus);
                await context.SaveChangesAsync();

                // Automatically add household heads as residents (nhân khẩu)
                await AddHouseholdHeadsAsResidents(context, hoKhaus, userManager);
            }
        }

        private static async Task AddHouseholdHeadsAsResidents(ApplicationDbContext context, List<HoKhau> hoKhaus, UserManager<ApplicationUser> userManager)
        {
            var nhanKhaus = new List<NhanKhau>();

            foreach (var hoKhau in hoKhaus)
            {
                var chuHo = await userManager.FindByIdAsync(hoKhau.ChuHoId);
                if (chuHo != null)
                {                    var nhanKhau = new NhanKhau
                    {
                        HoTen = chuHo.HoTen,
                        NgaySinh = chuHo.NgaySinh,
                        GioiTinh = "Nam", // Default value, can be updated later
                        CMND = chuHo.CMND,
                        SoDienThoai = chuHo.SoDienThoai,
                        Email = chuHo.Email ?? "",
                        QuocTich = "Việt Nam",
                        NgheNghiep = "Chưa cập nhật", // Default value
                        NoiLamViec = "Chưa cập nhật", // Default value
                        QuanHeVoiChuHo = "Chủ hộ",
                        HoKhauId = hoKhau.HoKhauId,
                        UserId = chuHo.Id,
                        NgayDangKy = hoKhau.NgayTao,
                        TrangThai = true
                    };
                    nhanKhaus.Add(nhanKhau);
                }
            }

            if (nhanKhaus.Any())
            {
                await context.NhanKhaus.AddRangeAsync(nhanKhaus);
                await context.SaveChangesAsync();
            }
        }
    }
}
