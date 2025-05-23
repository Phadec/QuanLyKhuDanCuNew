using QuanLyKhuDanCu.Data;
using QuanLyKhuDanCu.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Data.Seeders
{
    public static class DichVuSeeder
    {
        public static async Task SeedDichVuAsync(ApplicationDbContext context)
        {
            if (context.DichVus.Any())
            {
                return; // Database has been seeded
            }

            var dichVus = new[]
            {
                new DichVu
                {
                    TenDichVu = "Sửa chữa điện",
                    MoTa = "Dịch vụ sửa chữa, bảo trì hệ thống điện trong căn hộ",
                    PhiDichVu = 150000,
                    TrangThai = true
                },
                new DichVu
                {
                    TenDichVu = "Sửa chữa nước",
                    MoTa = "Dịch vụ sửa chữa, bảo trì hệ thống nước trong căn hộ",
                    PhiDichVu = 200000,
                    TrangThai = true
                },
                new DichVu
                {
                    TenDichVu = "Vệ sinh căn hộ",
                    MoTa = "Dịch vụ vệ sinh, dọn dẹp căn hộ",
                    PhiDichVu = 300000,
                    TrangThai = true
                },
                new DichVu
                {
                    TenDichVu = "Sửa chữa thiết bị",
                    MoTa = "Dịch vụ sửa chữa các thiết bị trong căn hộ",
                    PhiDichVu = 250000,
                    TrangThai = true
                },
                new DichVu
                {
                    TenDichVu = "Đổi khóa, làm chìa khóa",
                    MoTa = "Dịch vụ thay khóa, làm thêm chìa khóa",
                    PhiDichVu = 100000,
                    TrangThai = true
                }
            };

            await context.DichVus.AddRangeAsync(dichVus);
            await context.SaveChangesAsync();
        }
    }
}
