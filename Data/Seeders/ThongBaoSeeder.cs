using Microsoft.AspNetCore.Identity;
using QuanLyKhuDanCu.Data;
using QuanLyKhuDanCu.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Data.Seeders
{
    public static class ThongBaoSeeder
    {
        public static async Task SeedThongBaoAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (context.ThongBaos.Any())
            {
                return; // Database has been seeded
            }

            // Get admin user for creating announcements
            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            if (adminUser == null)
            {
                return; // No admin user to create announcements
            }

            var thongBaos = new[]
            {
                new ThongBao
                {
                    TieuDe = "Thông báo bảo trì hệ thống nước",
                    NoiDung = "Kính gửi quý cư dân,\n\nTrung tâm quản lý chung cư xin thông báo sẽ tiến hành bảo trì hệ thống nước vào ngày 15/01/2024 từ 8:00 đến 12:00. Trong thời gian này, hệ thống nước có thể bị gián đoạn.\n\nXin cảm ơn sự hợp tác của quý cư dân.",
                    NgayTao = DateTime.Now.AddDays(-2),
                    NgayHetHan = DateTime.Now.AddDays(15),
                    DoiTuong = "TatCa",
                    TrangThai = true,
                    NguoiTaoId = adminUser.Id,
                    FileDinhKem = null // Use null if no file
                },
                new ThongBao
                {
                    TieuDe = "Thông báo họp cư dân quý I/2024",
                    NoiDung = "Kính gửi quý cư dân,\n\nBan quản lý chung cư trân trọng kính mời quý cư dân tham dự cuộc họp định kỳ quý I năm 2024 được tổ chức vào ngày 20/01/2024 lúc 19:30 tại sảnh chính tòa nhà.\n\nNội dung cuộc họp:\n1. Báo cáo tình hình an ninh, vệ sinh\n2. Thảo luận các vấn đề cư dân quan tâm\n3. Kế hoạch hoạt động quý tới\n\nRất mong sự có mặt đông đủ của quý cư dân.",
                    NgayTao = DateTime.Now.AddDays(-1),
                    NgayHetHan = DateTime.Now.AddDays(20),
                    DoiTuong = "TatCa",
                    TrangThai = true,
                    NguoiTaoId = adminUser.Id,
                    FileDinhKem = null // Use null if no file
                },
                new ThongBao
                {
                    TieuDe = "Hướng dẫn thanh toán phí trực tuyến",
                    NoiDung = "Kính gửi quý cư dân,\n\nNhằm tạo điều kiện thuận lợi cho việc thanh toán các khoản phí, ban quản lý đã triển khai hình thức thanh toán trực tuyến qua ứng dụng quản lý chung cư. Hướng dẫn chi tiết vui lòng xem trong file đính kèm.\n\nMọi thắc mắc xin liên hệ quầy lễ tân hoặc số hotline: 0123.456.789.",
                    NgayTao = DateTime.Now.AddDays(-5),
                    NgayHetHan = null,
                    DoiTuong = "TatCa",
                    TrangThai = true,
                    NguoiTaoId = adminUser.Id,
                    FileDinhKem = null // Use null if no file
                },
                new ThongBao
                {
                    TieuDe = "Thông báo nghỉ Tết Nguyên đán 2024",
                    NoiDung = "Kính gửi quý cư dân,\n\nBan quản lý chung cư xin thông báo lịch nghỉ Tết Nguyên đán 2024 như sau:\n- Thời gian nghỉ: Từ ngày 08/02/2024 đến hết ngày 14/02/2024\n- Quầy lễ tân vẫn hoạt động 24/7 để hỗ trợ các vấn đề khẩn cấp\n- Đội kỹ thuật trực sửa chữa: 01 nhân viên/ca\n\nKính chúc quý cư dân năm mới An khang, Thịnh vượng!",
                    NgayTao = DateTime.Now,
                    NgayHetHan = DateTime.Now.AddDays(45),
                    DoiTuong = "TatCa",
                    TrangThai = true,
                    NguoiTaoId = adminUser.Id,
                    FileDinhKem = null // Use null if no file
                },
                new ThongBao
                {
                    TieuDe = "Thông báo nội bộ - Chỉ dành cho nhân viên",
                    NoiDung = "Kính gửi các bộ phận,\n\nVề việc chuẩn bị báo cáo tổng kết cuối năm 2023, đề nghị các bộ phận chuẩn bị nội dung báo cáo theo mẫu đã gửi và nộp trước ngày 05/01/2024.\n\nTrân trọng.",
                    NgayTao = DateTime.Now.AddDays(-3),
                    NgayHetHan = DateTime.Now.AddDays(5),
                    DoiTuong = "Staff",
                    TrangThai = true,
                    NguoiTaoId = adminUser.Id,
                    FileDinhKem = null // Use null if no file
                }
            };

            await context.ThongBaos.AddRangeAsync(thongBaos);
            await context.SaveChangesAsync();
        }
    }
}
