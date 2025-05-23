using QuanLyKhuDanCu.Data;
using QuanLyKhuDanCu.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Data.Seeders
{
    public static class LoaiPhiSeeder
    {
        public static async Task SeedLoaiPhiAsync(ApplicationDbContext context)
        {
            if (context.LoaiPhis.Any())
            {
                return; // Database has been seeded
            }

            var loaiPhis = new[]
            {
                new LoaiPhi
                {
                    TenLoaiPhi = "Phí quản lý",
                    MucPhi = 300000,
                    BatBuoc = true,
                    ChuKy = "Hàng tháng",
                    MoTa = "Phí quản lý chung cư hàng tháng",
                    TrangThai = true
                },
                new LoaiPhi
                {
                    TenLoaiPhi = "Phí gửi xe",
                    MucPhi = 100000,
                    BatBuoc = false,
                    ChuKy = "Hàng tháng",
                    MoTa = "Phí gửi xe ô tô/xe máy tại bãi",
                    TrangThai = true
                },
                new LoaiPhi
                {
                    TenLoaiPhi = "Phí dịch vụ vệ sinh",
                    MucPhi = 150000,
                    BatBuoc = true,
                    ChuKy = "Hàng tháng",
                    MoTa = "Phí vệ sinh khu vực chung",
                    TrangThai = true
                },
                new LoaiPhi
                {
                    TenLoaiPhi = "Phí bảo trì",
                    MucPhi = 200000,
                    BatBuoc = true,
                    ChuKy = "Hàng quý",
                    MoTa = "Phí bảo trì thiết bị và cơ sở vật chất",
                    TrangThai = true
                },
                new LoaiPhi
                {
                    TenLoaiPhi = "Phí an ninh",
                    MucPhi = 100000,
                    BatBuoc = true,
                    ChuKy = "Hàng tháng",
                    MoTa = "Phí bảo vệ và dịch vụ an ninh",
                    TrangThai = true
                }
            };

            await context.LoaiPhis.AddRangeAsync(loaiPhis);
            await context.SaveChangesAsync();
        }
    }
}
