using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhuDanCu.Data;
using QuanLyKhuDanCu.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Controllers
{
    [Authorize(Policy = "RequireManagerRole")]
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongKeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ThongKe
        public async Task<IActionResult> Index()
        {
            var dashboard = new DashboardViewModel
            {
                TongSoHoKhau = await _context.HoKhaus.CountAsync(),
                TongSoNhanKhau = await _context.NhanKhaus.CountAsync(),
                TongSoDonTamTruTamVang = await _context.TamTruTamVangs.CountAsync(),
                HoaDonChuaThanhToan = await _context.HoaDons.CountAsync(h => h.TrangThai == "ChuaThanhToan" || h.TrangThai == "QuaHan"),
                TongSoYeuCauDichVu = await _context.YeuCauDichVus.CountAsync(),
                TongSoPhanAnh = await _context.PhanAnhs.CountAsync(),
                
                // Residents by gender
                ThongKeGioiTinh = new Dictionary<string, int>
                {
                    ["Nam"] = await _context.NhanKhaus.CountAsync(n => n.GioiTinh == "Nam"),
                    ["Nữ"] = await _context.NhanKhaus.CountAsync(n => n.GioiTinh == "Nữ"),
                    ["Khác"] = await _context.NhanKhaus.CountAsync(n => n.GioiTinh != "Nam" && n.GioiTinh != "Nữ")
                },
                
                // Temporary residence status
                ThongKeTamTruTamVang = new Dictionary<string, int>
                {
                    ["Tạm trú"] = await _context.TamTruTamVangs.CountAsync(t => t.LoaiDon == "TamTru" && t.TrangThai == "DaDuyet" && t.NgayKetThuc > DateTime.Now),
                    ["Tạm vắng"] = await _context.TamTruTamVangs.CountAsync(t => t.LoaiDon == "TamVang" && t.TrangThai == "DaDuyet" && t.NgayKetThuc > DateTime.Now)
                },
                
                // Revenue summary
                TongThuTheoThang = await _context.HoaDons
                    .Where(h => h.TrangThai == "DaThanhToan" && h.NgayThanhToan.HasValue && h.NgayThanhToan.Value.Month == DateTime.Now.Month && h.NgayThanhToan.Value.Year == DateTime.Now.Year)
                    .SumAsync(h => h.TongTien),
                    
                TongThuTheoNam = await _context.HoaDons
                    .Where(h => h.TrangThai == "DaThanhToan" && h.NgayThanhToan.HasValue && h.NgayThanhToan.Value.Year == DateTime.Now.Year)
                    .SumAsync(h => h.TongTien)
            };

            return View(dashboard);
        }

        // GET: ThongKe/DanCu
        public async Task<IActionResult> DanCu()
        {
            var thongKeDanCu = new ThongKeDanCuViewModel
            {
                TongSoHoKhau = await _context.HoKhaus.CountAsync(),
                TongSoNhanKhau = await _context.NhanKhaus.CountAsync(),
                
                // Age groups
                ThongKeDoTuoi = new Dictionary<string, int>
                {
                    ["Dưới 18 tuổi"] = await _context.NhanKhaus.CountAsync(n => n.NgaySinh.AddYears(18) > DateTime.Now),
                    ["18-30 tuổi"] = await _context.NhanKhaus.CountAsync(n => n.NgaySinh.AddYears(18) <= DateTime.Now && n.NgaySinh.AddYears(30) > DateTime.Now),
                    ["31-45 tuổi"] = await _context.NhanKhaus.CountAsync(n => n.NgaySinh.AddYears(30) <= DateTime.Now && n.NgaySinh.AddYears(45) > DateTime.Now),
                    ["46-60 tuổi"] = await _context.NhanKhaus.CountAsync(n => n.NgaySinh.AddYears(45) <= DateTime.Now && n.NgaySinh.AddYears(60) > DateTime.Now),
                    ["Trên 60 tuổi"] = await _context.NhanKhaus.CountAsync(n => n.NgaySinh.AddYears(60) <= DateTime.Now)
                },
                
                // Household size distribution
                PhanBoQuyMoHo = await _context.HoKhaus
                    .Select(h => new { HoKhauId = h.HoKhauId, SoNhanKhau = h.NhanKhaus.Count })
                    .GroupBy(h => h.SoNhanKhau)
                    .Select(g => new KeyValuePair<string, int>(g.Key.ToString() + " người", g.Count()))
                    .ToDictionaryAsync(kvp => kvp.Key, kvp => kvp.Value),
                    
                // Nationality statistics
                ThongKeQuocTich = await _context.NhanKhaus
                    .GroupBy(n => n.QuocTich)
                    .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                    .ToDictionaryAsync(kvp => kvp.Key, kvp => kvp.Value),
                
                // Recent resident changes
                BiendongDanCuGanDay = await _context.NhanKhaus
                    .OrderByDescending(n => n.NgayDangKy)
                    .Take(10)
                    .Select(n => new BiendongDanCuItemViewModel
                    {
                        HoTen = n.HoTen,
                        NgaySinh = n.NgaySinh,
                        GioiTinh = n.GioiTinh,
                        DiaChi = n.HoKhau.DiaChi,
                        NgayDangKy = n.NgayDangKy
                    })
                    .ToListAsync()
            };

            return View(thongKeDanCu);
        }

        // GET: ThongKe/ThuPhi
        public async Task<IActionResult> ThuPhi(int? year)
        {
            if (!year.HasValue)
            {
                year = DateTime.Now.Year;
            }

            var startDate = new DateTime(year.Value, 1, 1);
            var endDate = new DateTime(year.Value, 12, 31);

            // Get monthly revenue for the selected year
            var monthlyRevenue = await _context.HoaDons
                .Where(h => h.TrangThai == "DaThanhToan" && h.NgayThanhToan.HasValue && 
                       h.NgayThanhToan.Value >= startDate && h.NgayThanhToan.Value <= endDate)
                .GroupBy(h => h.NgayThanhToan.Value.Month)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(h => h.TongTien) })
                .ToDictionaryAsync(x => x.Month, x => x.Revenue);

            // Get revenue by fee type
            var revenueByType = await _context.HoaDons
                .Where(h => h.TrangThai == "DaThanhToan" && h.NgayThanhToan.HasValue && 
                       h.NgayThanhToan.Value >= startDate && h.NgayThanhToan.Value <= endDate)
                .GroupBy(h => h.LoaiPhi.TenLoaiPhi)
                .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Sum(h => h.TongTien)))
                .ToDictionaryAsync(kvp => kvp.Key, kvp => kvp.Value);

            // Get payment status statistics
            var paymentStatusStats = new Dictionary<string, int>
            {
                ["Đã thanh toán"] = await _context.HoaDons
                    .CountAsync(h => h.TrangThai == "DaThanhToan" && h.NgayTao >= startDate && h.NgayTao <= endDate),
                ["Chưa thanh toán"] = await _context.HoaDons
                    .CountAsync(h => h.TrangThai == "ChuaThanhToan" && h.NgayTao >= startDate && h.NgayTao <= endDate),
                ["Quá hạn"] = await _context.HoaDons
                    .CountAsync(h => h.TrangThai == "QuaHan" && h.NgayTao >= startDate && h.NgayTao <= endDate)
            };

            // Get recent payments
            var recentPayments = await _context.HoaDons
                .Include(h => h.HoKhau)
                .Include(h => h.LoaiPhi)
                .Include(h => h.NguoiThu)
                .Where(h => h.TrangThai == "DaThanhToan")
                .OrderByDescending(h => h.NgayThanhToan)
                .Take(10)
                .ToListAsync();

            var viewModel = new ThongKeThuPhiViewModel
            {
                Year = year.Value,
                TongThuNam = await _context.HoaDons
                    .Where(h => h.TrangThai == "DaThanhToan" && h.NgayThanhToan.HasValue && 
                           h.NgayThanhToan.Value >= startDate && h.NgayThanhToan.Value <= endDate)
                    .SumAsync(h => h.TongTien),
                DoanhThuTheoThang = new Dictionary<string, decimal>(),
                DoanhThuTheoLoaiPhi = revenueByType,
                ThongKeTrangThaiThanhToan = paymentStatusStats,
                ThanhToanGanDay = recentPayments
            };

            // Fill in all months
            for (int i = 1; i <= 12; i++)
            {
                string monthName = new DateTime(2000, i, 1).ToString("MMMM");
                viewModel.DoanhThuTheoThang[monthName] = monthlyRevenue.ContainsKey(i) ? monthlyRevenue[i] : 0;
            }

            return View(viewModel);
        }

        // GET: ThongKe/YeuCauDichVu
        public async Task<IActionResult> YeuCauDichVu(int? year)
        {
            if (!year.HasValue)
            {
                year = DateTime.Now.Year;
            }

            var startDate = new DateTime(year.Value, 1, 1);
            var endDate = new DateTime(year.Value, 12, 31);

            // Get service requests by month
            var requestsByMonth = await _context.YeuCauDichVus
                .Where(y => y.NgayYeuCau >= startDate && y.NgayYeuCau <= endDate)
                .GroupBy(y => y.NgayYeuCau.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Month, x => x.Count);

            // Get service requests by status
            var requestsByStatus = await _context.YeuCauDichVus
                .Where(y => y.NgayYeuCau >= startDate && y.NgayYeuCau <= endDate)
                .GroupBy(y => y.TrangThai)
                .Select(g => new KeyValuePair<string, int>(
                    g.Key == "ChoXuLy" ? "Chờ xử lý" :
                    g.Key == "DangXuLy" ? "Đang xử lý" :
                    g.Key == "DaHoanThanh" ? "Đã hoàn thành" :
                    g.Key == "TuChoi" ? "Từ chối" : g.Key,
                    g.Count()))
                .ToDictionaryAsync(kvp => kvp.Key, kvp => kvp.Value);

            // Get service requests by type
            var requestsByType = await _context.YeuCauDichVus
                .Where(y => y.NgayYeuCau >= startDate && y.NgayYeuCau <= endDate)
                .GroupBy(y => y.DichVu.TenDichVu)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .ToDictionaryAsync(kvp => kvp.Key, kvp => kvp.Value);            // Get average processing time (in days)
            var completedRequests = await _context.YeuCauDichVus
                .Where(y => y.TrangThai == "DaHoanThanh" && y.NgayHoanThanh.HasValue &&
                       y.NgayYeuCau >= startDate && y.NgayYeuCau <= endDate)
                .Select(y => new { NgayYeuCau = y.NgayYeuCau, NgayHoanThanh = y.NgayHoanThanh.Value })
                .ToListAsync();
            
            var avgProcessingTime = completedRequests.Any() 
                ? completedRequests.Average(r => (r.NgayHoanThanh - r.NgayYeuCau).Days)
                : 0;

            // Get recent service requests
            var recentRequests = await _context.YeuCauDichVus
                .Include(y => y.DichVu)
                .Include(y => y.User)
                .Include(y => y.NguoiXuLy)
                .OrderByDescending(y => y.NgayYeuCau)
                .Take(10)
                .ToListAsync();

            var viewModel = new ThongKeYeuCauDichVuViewModel
            {
                Year = year.Value,
                TongSoYeuCau = await _context.YeuCauDichVus
                    .CountAsync(y => y.NgayYeuCau >= startDate && y.NgayYeuCau <= endDate),
                YeuCauTheoThang = new Dictionary<string, int>(),
                YeuCauTheoTrangThai = requestsByStatus,
                YeuCauTheoDichVu = requestsByType,
                ThoiGianXuLyTrungBinh = avgProcessingTime,
                YeuCauGanDay = recentRequests
            };

            // Fill in all months
            for (int i = 1; i <= 12; i++)
            {
                string monthName = new DateTime(2000, i, 1).ToString("MMMM");
                viewModel.YeuCauTheoThang[monthName] = requestsByMonth.ContainsKey(i) ? requestsByMonth[i] : 0;
            }

            return View(viewModel);
        }

        // GET: ThongKe/ExportData
        public async Task<IActionResult> ExportData(string type)
        {
            // This is a placeholder for the data export functionality
            // In a real application, you would implement different export formats (CSV, Excel, PDF)
            // based on the 'type' parameter and the specific data requested
            
            // For now, we'll just redirect back to the index with a message
            TempData["Message"] = $"Tính năng xuất dữ liệu {type} đang được phát triển.";
            return RedirectToAction(nameof(Index));
        }
    }
}
