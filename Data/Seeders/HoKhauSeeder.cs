using QuanLyKhuDanCu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace QuanLyKhuDanCu.Data.Seeders
{
    public static class HoKhauSeeder
    {
        public static async Task SeedHoKhauAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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
            }
        }
    }
}
