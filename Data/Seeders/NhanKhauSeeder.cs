using QuanLyKhuDanCu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace QuanLyKhuDanCu.Data.Seeders
{
    public static class NhanKhauSeeder
    {        public static async Task SeedNhanKhauAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Check if there are already residents (household heads are now automatically created by HoKhauSeeder)
            var hoKhau1 = await context.HoKhaus.FirstOrDefaultAsync(h => h.MaHoKhau == "HK001");
            var hoKhau2 = await context.HoKhaus.FirstOrDefaultAsync(h => h.MaHoKhau == "HK002");

            // Add additional family members for HK001
            if (hoKhau1 != null)
            {
                // Check if there are already residents for this household
                var existingResidents = await context.NhanKhaus
                    .Where(n => n.HoKhauId == hoKhau1.HoKhauId)
                    .ToListAsync();

                // Only add additional family members if not already seeded
                if (existingResidents.Count <= 1) // Only household head exists
                {
                    var additionalMembers = new List<NhanKhau>
                    {
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
                        },
                        new NhanKhau
                        {
                            HoTen = "Nguyễn Văn C",
                            NgaySinh = new DateTime(2010, 3, 15),
                            GioiTinh = "Nam",
                            CMND = "012345678903",
                            SoDienThoai = "",
                            Email = "",
                            QuocTich = "Việt Nam",
                            NgheNghiep = "Học sinh",
                            NoiLamViec = "Trường tiểu học ABC",
                            QuanHeVoiChuHo = "Con",
                            HoKhauId = hoKhau1.HoKhauId,
                            UserId = null,
                            NgayDangKy = DateTime.Now.AddDays(-30),
                            TrangThai = true
                        }
                    };
                    await context.NhanKhaus.AddRangeAsync(additionalMembers);
                }
            }

            // For HK002, household head is already added automatically by HoKhauSeeder
            // We can add additional family members if needed
            if (hoKhau2 != null)
            {
                var existingResidents = await context.NhanKhaus
                    .Where(n => n.HoKhauId == hoKhau2.HoKhauId)
                    .ToListAsync();

                // Only add additional family members if not already seeded
                if (existingResidents.Count <= 1) // Only household head exists
                {
                    var additionalMember = new NhanKhau
                    {
                        HoTen = "Lê Văn D",
                        NgaySinh = new DateTime(2012, 7, 8),
                        GioiTinh = "Nam",
                        CMND = "012345678904",
                        SoDienThoai = "",
                        Email = "",
                        QuocTich = "Việt Nam",
                        NgheNghiep = "Học sinh",
                        NoiLamViec = "Trường tiểu học DEF",
                        QuanHeVoiChuHo = "Con",
                        HoKhauId = hoKhau2.HoKhauId,
                        UserId = null,
                        NgayDangKy = DateTime.Now.AddDays(-25),
                        TrangThai = true
                    };
                    await context.NhanKhaus.AddAsync(additionalMember);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
