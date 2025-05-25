using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhuDanCu.Data;
using QuanLyKhuDanCu.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Text;
using iText = iTextSharp.text;

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
                    .Include(n => n.HoKhau)
                    .OrderByDescending(n => n.NgayDangKy)
                    .Take(10)
                    .Select(n => new BiendongDanCuItemViewModel
                    {
                        HoTen = n.HoTen,
                        NgaySinh = n.NgaySinh,
                        GioiTinh = n.GioiTinh,
                        DiaChi = n.HoKhau != null ? n.HoKhau.DiaChi : "Không có thông tin",
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
            var endDate = new DateTime(year.Value, 12, 31);            // Get monthly revenue for the selected year
            var monthlyRevenue = await _context.HoaDons
                .Where(h => h.TrangThai == "DaThanhToan" && h.NgayThanhToan.HasValue && 
                       h.NgayThanhToan.Value >= startDate && h.NgayThanhToan.Value <= endDate)
                .GroupBy(h => h.NgayThanhToan!.Value.Month)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(h => h.TongTien) })
                .ToDictionaryAsync(x => x.Month, x => x.Revenue);            // Get revenue by fee type
            var revenueByType = await _context.HoaDons
                .Include(h => h.LoaiPhi)
                .Where(h => h.TrangThai == "DaThanhToan" && h.NgayThanhToan.HasValue && 
                       h.NgayThanhToan.Value >= startDate && h.NgayThanhToan.Value <= endDate)
                .GroupBy(h => h.LoaiPhi!.TenLoaiPhi)
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
                .ToDictionaryAsync(kvp => kvp.Key, kvp => kvp.Value);            // Get service requests by type
            var requestsByType = await _context.YeuCauDichVus
                .Include(y => y.DichVu)
                .Where(y => y.NgayYeuCau >= startDate && y.NgayYeuCau <= endDate)
                .GroupBy(y => y.DichVu!.TenDichVu)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .ToDictionaryAsync(kvp => kvp.Key, kvp => kvp.Value);
                
            // Get average processing time (in days)
            var completedRequests = await _context.YeuCauDichVus
                .Where(y => y.TrangThai == "DaHoanThanh" && y.NgayHoanThanh.HasValue &&
                       y.NgayYeuCau >= startDate && y.NgayYeuCau <= endDate)
                .Select(y => new { NgayYeuCau = y.NgayYeuCau, NgayHoanThanh = y.NgayHoanThanh!.Value })
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
        }        // GET: ThongKe/ExportData
        public async Task<IActionResult> ExportData(string type)
        {
            try
            {
                // Get dashboard data
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
                };                switch (type?.ToLower())
                {
                    case "excel":
                        return ExportToExcel(dashboard);
                    case "pdf":
                        return ExportToPdf(dashboard);
                    default:
                        TempData["Error"] = "Định dạng xuất không được hỗ trợ.";
                        return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra khi xuất dữ liệu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private IActionResult ExportToExcel(DashboardViewModel dashboard)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            
            // Sheet 1: Tổng quan
            var worksheetOverview = package.Workbook.Worksheets.Add("Tổng quan");
            
            // Title
            worksheetOverview.Cells[1, 1].Value = "BÁO CÁO THỐNG KÊ TỔNG QUAN KHU DÂN CƯ";
            worksheetOverview.Cells[1, 1, 1, 6].Merge = true;
            worksheetOverview.Cells[1, 1].Style.Font.Size = 16;
            worksheetOverview.Cells[1, 1].Style.Font.Bold = true;
            worksheetOverview.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            // Date
            worksheetOverview.Cells[2, 1].Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            worksheetOverview.Cells[2, 1, 2, 6].Merge = true;
            worksheetOverview.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            // Statistics data
            int row = 4;
            worksheetOverview.Cells[row, 1].Value = "Chỉ số";
            worksheetOverview.Cells[row, 2].Value = "Giá trị";            worksheetOverview.Cells[row, 1, row, 2].Style.Font.Bold = true;
            worksheetOverview.Cells[row, 1, row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheetOverview.Cells[row, 1, row, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            
            row++;
            worksheetOverview.Cells[row, 1].Value = "Tổng số hộ khẩu";
            worksheetOverview.Cells[row, 2].Value = dashboard.TongSoHoKhau;
            row++;
            worksheetOverview.Cells[row, 1].Value = "Tổng số nhân khẩu";
            worksheetOverview.Cells[row, 2].Value = dashboard.TongSoNhanKhau;
            row++;
            worksheetOverview.Cells[row, 1].Value = "Đơn tạm trú/tạm vắng";
            worksheetOverview.Cells[row, 2].Value = dashboard.TongSoDonTamTruTamVang;
            row++;
            worksheetOverview.Cells[row, 1].Value = "Hóa đơn chưa thanh toán";
            worksheetOverview.Cells[row, 2].Value = dashboard.HoaDonChuaThanhToan;
            row++;
            worksheetOverview.Cells[row, 1].Value = "Yêu cầu dịch vụ";
            worksheetOverview.Cells[row, 2].Value = dashboard.TongSoYeuCauDichVu;
            row++;
            worksheetOverview.Cells[row, 1].Value = "Phản ánh & góp ý";
            worksheetOverview.Cells[row, 2].Value = dashboard.TongSoPhanAnh;
            row++;
            worksheetOverview.Cells[row, 1].Value = "Thu nhập tháng này";
            worksheetOverview.Cells[row, 2].Value = dashboard.TongThuTheoThang;
            worksheetOverview.Cells[row, 2].Style.Numberformat.Format = "#,##0 ₫";
            row++;
            worksheetOverview.Cells[row, 1].Value = "Thu nhập năm nay";
            worksheetOverview.Cells[row, 2].Value = dashboard.TongThuTheoNam;
            worksheetOverview.Cells[row, 2].Style.Numberformat.Format = "#,##0 ₫";
            
            // Sheet 2: Thống kê theo giới tính
            var worksheetGender = package.Workbook.Worksheets.Add("Thống kê giới tính");
            worksheetGender.Cells[1, 1].Value = "THỐNG KÊ THEO GIỚI TÍNH";
            worksheetGender.Cells[1, 1, 1, 3].Merge = true;
            worksheetGender.Cells[1, 1].Style.Font.Size = 14;
            worksheetGender.Cells[1, 1].Style.Font.Bold = true;
            worksheetGender.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            row = 3;
            worksheetGender.Cells[row, 1].Value = "Giới tính";
            worksheetGender.Cells[row, 2].Value = "Số lượng";
            worksheetGender.Cells[row, 3].Value = "Tỷ lệ %";            worksheetGender.Cells[row, 1, row, 3].Style.Font.Bold = true;
            worksheetGender.Cells[row, 1, row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheetGender.Cells[row, 1, row, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
            
            var totalPeople = dashboard.ThongKeGioiTinh.Values.Sum();
            foreach (var item in dashboard.ThongKeGioiTinh)
            {
                row++;
                worksheetGender.Cells[row, 1].Value = item.Key;
                worksheetGender.Cells[row, 2].Value = item.Value;
                worksheetGender.Cells[row, 3].Value = totalPeople > 0 ? (double)item.Value / totalPeople : 0;
                worksheetGender.Cells[row, 3].Style.Numberformat.Format = "0.00%";
            }
            
            // Sheet 3: Thống kê tạm trú/tạm vắng
            var worksheetResident = package.Workbook.Worksheets.Add("Tạm trú - Tạm vắng");
            worksheetResident.Cells[1, 1].Value = "THỐNG KÊ TẠM TRÚ - TẠM VẮNG";
            worksheetResident.Cells[1, 1, 1, 3].Merge = true;
            worksheetResident.Cells[1, 1].Style.Font.Size = 14;
            worksheetResident.Cells[1, 1].Style.Font.Bold = true;
            worksheetResident.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            row = 3;
            worksheetResident.Cells[row, 1].Value = "Loại đơn";
            worksheetResident.Cells[row, 2].Value = "Số lượng";
            worksheetResident.Cells[row, 3].Value = "Tỷ lệ %";            worksheetResident.Cells[row, 1, row, 3].Style.Font.Bold = true;
            worksheetResident.Cells[row, 1, row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheetResident.Cells[row, 1, row, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
            
            var totalResidents = dashboard.ThongKeTamTruTamVang.Values.Sum();
            foreach (var item in dashboard.ThongKeTamTruTamVang)
            {
                row++;
                worksheetResident.Cells[row, 1].Value = item.Key;
                worksheetResident.Cells[row, 2].Value = item.Value;
                worksheetResident.Cells[row, 3].Value = totalResidents > 0 ? (double)item.Value / totalResidents : 0;
                worksheetResident.Cells[row, 3].Style.Numberformat.Format = "0.00%";
            }
            
            // Auto-fit columns
            foreach (var worksheet in package.Workbook.Worksheets)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }
            
            var fileName = $"ThongKe_KhuDanCu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var fileContent = package.GetAsByteArray();
            
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private IActionResult ExportToPdf(DashboardViewModel dashboard)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 20, 20, 40, 40);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();
            
            // Create fonts
            var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
            var headerFont = FontFactory.GetFont("Arial", 14, Font.BOLD);
            var normalFont = FontFactory.GetFont("Arial", 11);
            var tableHeaderFont = FontFactory.GetFont("Arial", 10, Font.BOLD);
            var tableDataFont = FontFactory.GetFont("Arial", 9);
            
            // Title
            var titleParagraph = new Paragraph("BÁO CÁO THỐNG KÊ TỔNG QUAN KHU DÂN CƯ", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10
            };
            document.Add(titleParagraph);
            
            // Date
            var dateParagraph = new Paragraph($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", normalFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(dateParagraph);
            
            // Overview statistics table
            var overviewHeader = new Paragraph("I. THỐNG KÊ TỔNG QUAN", headerFont)
            {
                SpacingAfter = 10
            };
            document.Add(overviewHeader);
            
            var overviewTable = new PdfPTable(2) { WidthPercentage = 100 };
            overviewTable.SetWidths(new float[] { 3, 1 });
              // Table headers
            var headerCell1 = new PdfPCell(new Phrase("Chỉ số", tableHeaderFont))
            {
                BackgroundColor = new BaseColor(211, 211, 211),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 8
            };
            var headerCell2 = new PdfPCell(new Phrase("Giá trị", tableHeaderFont))
            {
                BackgroundColor = new BaseColor(211, 211, 211),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 8
            };
            overviewTable.AddCell(headerCell1);
            overviewTable.AddCell(headerCell2);
            
            // Data rows
            var overviewData = new List<(string Label, string Value)>
            {
                ("Tổng số hộ khẩu", dashboard.TongSoHoKhau.ToString()),
                ("Tổng số nhân khẩu", dashboard.TongSoNhanKhau.ToString()),
                ("Đơn tạm trú/tạm vắng", dashboard.TongSoDonTamTruTamVang.ToString()),
                ("Hóa đơn chưa thanh toán", dashboard.HoaDonChuaThanhToan.ToString()),
                ("Yêu cầu dịch vụ", dashboard.TongSoYeuCauDichVu.ToString()),
                ("Phản ánh & góp ý", dashboard.TongSoPhanAnh.ToString()),
                ("Thu nhập tháng này", dashboard.TongThuTheoThang.ToString("N0") + " VNĐ"),
                ("Thu nhập năm nay", dashboard.TongThuTheoNam.ToString("N0") + " VNĐ")
            };
            
            foreach (var (label, value) in overviewData)
            {
                overviewTable.AddCell(new PdfPCell(new Phrase(label, tableDataFont)) { Padding = 6 });
                overviewTable.AddCell(new PdfPCell(new Phrase(value, tableDataFont)) 
                { 
                    Padding = 6, 
                    HorizontalAlignment = Element.ALIGN_RIGHT 
                });
            }
            
            document.Add(overviewTable);
            document.Add(new Paragraph("\n"));
            
            // Gender statistics
            var genderHeader = new Paragraph("II. THỐNG KÊ THEO GIỚI TÍNH", headerFont)
            {
                SpacingAfter = 10
            };
            document.Add(genderHeader);
            
            var genderTable = new PdfPTable(3) { WidthPercentage = 100 };
            genderTable.SetWidths(new float[] { 2, 1, 1 });
              genderTable.AddCell(new PdfPCell(new Phrase("Giới tính", tableHeaderFont))
            {
                BackgroundColor = new BaseColor(211, 211, 211),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 8
            });
            genderTable.AddCell(new PdfPCell(new Phrase("Số lượng", tableHeaderFont))
            {
                BackgroundColor = new BaseColor(211, 211, 211),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 8
            });
            genderTable.AddCell(new PdfPCell(new Phrase("Tỷ lệ %", tableHeaderFont))
            {
                BackgroundColor = new BaseColor(211, 211, 211),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 8
            });
            
            var totalPeople = dashboard.ThongKeGioiTinh.Values.Sum();
            foreach (var item in dashboard.ThongKeGioiTinh)
            {
                genderTable.AddCell(new PdfPCell(new Phrase(item.Key, tableDataFont)) { Padding = 6 });
                genderTable.AddCell(new PdfPCell(new Phrase(item.Value.ToString(), tableDataFont)) 
                { 
                    Padding = 6, 
                    HorizontalAlignment = Element.ALIGN_RIGHT 
                });
                var percentage = totalPeople > 0 ? ((double)item.Value / totalPeople * 100).ToString("F1") + "%" : "0%";
                genderTable.AddCell(new PdfPCell(new Phrase(percentage, tableDataFont)) 
                { 
                    Padding = 6, 
                    HorizontalAlignment = Element.ALIGN_RIGHT 
                });
            }
            
            document.Add(genderTable);
            document.Add(new Paragraph("\n"));
            
            // Temporary residence statistics
            var residentHeader = new Paragraph("III. THỐNG KÊ TẠM TRÚ - TẠM VẮNG", headerFont)
            {
                SpacingAfter = 10
            };
            document.Add(residentHeader);
            
            var residentTable = new PdfPTable(3) { WidthPercentage = 100 };
            residentTable.SetWidths(new float[] { 2, 1, 1 });
              residentTable.AddCell(new PdfPCell(new Phrase("Loại đơn", tableHeaderFont))
            {
                BackgroundColor = new BaseColor(211, 211, 211),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 8
            });
            residentTable.AddCell(new PdfPCell(new Phrase("Số lượng", tableHeaderFont))
            {
                BackgroundColor = new BaseColor(211, 211, 211),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 8
            });
            residentTable.AddCell(new PdfPCell(new Phrase("Tỷ lệ %", tableHeaderFont))
            {
                BackgroundColor = new BaseColor(211, 211, 211),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 8
            });
            
            var totalResidents = dashboard.ThongKeTamTruTamVang.Values.Sum();
            foreach (var item in dashboard.ThongKeTamTruTamVang)
            {
                residentTable.AddCell(new PdfPCell(new Phrase(item.Key, tableDataFont)) { Padding = 6 });
                residentTable.AddCell(new PdfPCell(new Phrase(item.Value.ToString(), tableDataFont)) 
                { 
                    Padding = 6, 
                    HorizontalAlignment = Element.ALIGN_RIGHT 
                });
                var percentage = totalResidents > 0 ? ((double)item.Value / totalResidents * 100).ToString("F1") + "%" : "0%";
                residentTable.AddCell(new PdfPCell(new Phrase(percentage, tableDataFont)) 
                { 
                    Padding = 6, 
                    HorizontalAlignment = Element.ALIGN_RIGHT 
                });
            }
            
            document.Add(residentTable);
            
            // Footer
            document.Add(new Paragraph("\n"));
            var footerParagraph = new Paragraph("--- Hết báo cáo ---", normalFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingBefore = 20
            };
            document.Add(footerParagraph);
            
            document.Close();
            
            var fileName = $"ThongKe_KhuDanCu_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(memoryStream.ToArray(), "application/pdf", fileName);
        }
    }
}
