using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhuDanCu.Data;
using QuanLyKhuDanCu.Helpers; // Add this import
using QuanLyKhuDanCu.Models;
using QuanLyKhuDanCu.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Controllers
{
    [Authorize]
    public class HoaDonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HoaDonController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }        // GET: HoaDon
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> Index(string searchString, string status, int page = 1)
        {
            var query = _context.HoaDons
                .Include(h => h.HoKhau)
                .Include(h => h.LoaiPhi)
                .Include(h => h.NguoiThu)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(h => 
                    h.MaHoaDon.Contains(searchString) || 
                    h.HoKhau.MaHoKhau.Contains(searchString) ||
                    h.HoKhau.DiaChi.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(h => h.TrangThai == status);
            }

            int pageSize = 10;
            var hoaDons = await QuanLyKhuDanCu.Helpers.PaginatedList<HoaDon>.CreateAsync(
                query.OrderByDescending(h => h.NgayTao), page, pageSize);

            ViewData["Status"] = new SelectList(new[] 
            { 
                new { Value = "", Text = "Tất cả" },
                new { Value = "ChuaThanhToan", Text = "Chưa thanh toán" },
                new { Value = "DaThanhToan", Text = "Đã thanh toán" },
                new { Value = "QuaHan", Text = "Quá hạn" }
            }, "Value", "Text", status);

            return View(hoaDons);
        }

        // GET: HoaDon/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }            var hoaDon = await _context.HoaDons
                .Include(h => h.HoKhau)
                    .ThenInclude(hk => hk.ChuHo)
                .Include(h => h.LoaiPhi)
                .Include(h => h.NguoiThu)
                .FirstOrDefaultAsync(m => m.HoaDonId == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            // For residents, check if they own this invoice
            if (User.IsInRole("Resident"))
            {
                var user = await _userManager.GetUserAsync(User);
                var nhanKhau = await _context.NhanKhaus.FirstOrDefaultAsync(n => n.UserId == user.Id);
                
                if (nhanKhau == null)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
                
                // Kiểm tra trực tiếp HoKhauId
                if (nhanKhau.HoKhauId == null || nhanKhau.HoKhauId != hoaDon.HoKhauId)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
            }

            return View(hoaDon);
        }        // GET: HoaDon/Create
        [Authorize(Policy = "RequireStaffRole")]
        public IActionResult Create()
        {
            ViewData["HoKhauId"] = new SelectList(_context.HoKhaus, "HoKhauId", "MaHoKhau");
            ViewData["LoaiPhiId"] = new SelectList(_context.LoaiPhis, "LoaiPhiId", "TenLoaiPhi");
            return View();
        }        // POST: HoaDon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> Create(HoaDonViewModel model)
        {
            // Debug logging
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
            }
            
            if (ModelState.IsValid)
            {
                var loaiPhi = await _context.LoaiPhis.FindAsync(model.LoaiPhiId);
                if (loaiPhi == null)
                {
                    ModelState.AddModelError("LoaiPhiId", "Loại phí không hợp lệ");
                    ViewData["HoKhauId"] = new SelectList(_context.HoKhaus, "HoKhauId", "MaHoKhau");
                    ViewData["LoaiPhiId"] = new SelectList(_context.LoaiPhis, "LoaiPhiId", "TenLoaiPhi");
                    return View(model);
                }

                var hoaDon = new HoaDon
                {
                    HoKhauId = model.HoKhauId,
                    LoaiPhiId = model.LoaiPhiId,
                    MaHoaDon = GenerateInvoiceCode(),
                    TongTien = model.TongTien ?? loaiPhi.MucPhi,
                    NgayTao = DateTime.Now,
                    HanThanhToan = model.HanThanhToan,
                    TrangThai = "ChuaThanhToan",
                    GhiChu = model.GhiChu
                };                _context.Add(hoaDon);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo hóa đơn thành công.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["HoKhauId"] = new SelectList(_context.HoKhaus, "HoKhauId", "MaHoKhau");
            ViewData["LoaiPhiId"] = new SelectList(_context.LoaiPhis, "LoaiPhiId", "TenLoaiPhi");
            return View(model);
        }        // GET: HoaDon/Edit/5
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDons.FindAsync(id);
            if (hoaDon == null)
            {
                return NotFound();
            }

            // Don't allow editing of paid invoices
            if (hoaDon.TrangThai == "DaThanhToan")
            {
                return BadRequest("Không thể chỉnh sửa hóa đơn đã thanh toán");
            }

            var viewModel = new HoaDonViewModel
            {
                HoaDonId = hoaDon.HoaDonId,
                HoKhauId = hoaDon.HoKhauId,
                LoaiPhiId = hoaDon.LoaiPhiId,
                MaHoaDon = hoaDon.MaHoaDon,
                TongTien = hoaDon.TongTien,
                HanThanhToan = hoaDon.HanThanhToan,
                GhiChu = hoaDon.GhiChu
            };

            ViewData["HoKhauId"] = new SelectList(_context.HoKhaus, "HoKhauId", "MaHoKhau");
            ViewData["LoaiPhiId"] = new SelectList(_context.LoaiPhis, "LoaiPhiId", "TenLoaiPhi");
            return View(viewModel);
        }        // POST: HoaDon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> Edit(int id, HoaDonViewModel model)
        {
            if (id != model.HoaDonId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var hoaDon = await _context.HoaDons.FindAsync(id);
                    if (hoaDon == null)
                    {
                        return NotFound();
                    }

                    // Don't allow editing of paid invoices
                    if (hoaDon.TrangThai == "DaThanhToan")
                    {
                        return BadRequest("Không thể chỉnh sửa hóa đơn đã thanh toán");
                    }

                    hoaDon.HoKhauId = model.HoKhauId;
                    hoaDon.LoaiPhiId = model.LoaiPhiId;
                    hoaDon.TongTien = model.TongTien ?? hoaDon.TongTien;
                    hoaDon.HanThanhToan = model.HanThanhToan;
                    hoaDon.GhiChu = model.GhiChu;

                    // Check if overdue
                    if (hoaDon.HanThanhToan < DateTime.Now && hoaDon.TrangThai == "ChuaThanhToan")
                    {
                        hoaDon.TrangThai = "QuaHan";
                    }

                    _context.Update(hoaDon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HoaDonExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["HoKhauId"] = new SelectList(_context.HoKhaus, "HoKhauId", "MaHoKhau");
            ViewData["LoaiPhiId"] = new SelectList(_context.LoaiPhis, "LoaiPhiId", "TenLoaiPhi");
            return View(model);
        }        // GET: HoaDon/MarkAsPaid/5
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> MarkAsPaid(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDons
                .Include(h => h.HoKhau)
                .Include(h => h.LoaiPhi)
                .FirstOrDefaultAsync(m => m.HoaDonId == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            if (hoaDon.TrangThai == "DaThanhToan")
            {
                return BadRequest("Hóa đơn này đã được thanh toán");
            }

            return View(hoaDon);
        }        // POST: HoaDon/MarkAsPaid/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var hoaDon = await _context.HoaDons.FindAsync(id);
            if (hoaDon == null)
            {
                return NotFound();
            }

            if (hoaDon.TrangThai == "DaThanhToan")
            {
                return BadRequest("Hóa đơn này đã được thanh toán");
            }

            var user = await _userManager.GetUserAsync(User);

            hoaDon.TrangThai = "DaThanhToan";
            hoaDon.NgayThanhToan = DateTime.Now;
            hoaDon.NguoiThuId = user.Id;            _context.Update(hoaDon);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật hóa đơn thành công.";
            return RedirectToAction(nameof(Index));
        }        // GET: HoaDon/Delete/5
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDons
                .Include(h => h.HoKhau)
                .Include(h => h.LoaiPhi)
                .FirstOrDefaultAsync(m => m.HoaDonId == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            // Don't allow deletion of paid invoices
            if (hoaDon.TrangThai == "DaThanhToan")
            {
                return BadRequest("Không thể xóa hóa đơn đã thanh toán");
            }

            return View(hoaDon);
        }        // POST: HoaDon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hoaDon = await _context.HoaDons.FindAsync(id);
            
            // Check again if paid
            if (hoaDon.TrangThai == "DaThanhToan")
            {
                return BadRequest("Không thể xóa hóa đơn đã thanh toán");
            }
              _context.HoaDons.Remove(hoaDon);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Xóa hóa đơn thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: HoaDon/MyInvoices
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> MyInvoices(string status)
        {
            var user = await _userManager.GetUserAsync(User);
            var nhanKhau = await _context.NhanKhaus.FirstOrDefaultAsync(n => n.UserId == user.Id);
            
            if (nhanKhau == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            
            // Đảm bảo rằng nhanKhau có HoKhauId
            if (nhanKhau.HoKhauId == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            
            var query = _context.HoaDons
                .Include(h => h.HoKhau)  // Thêm Include cho HoKhau
                .Include(h => h.LoaiPhi)
                .Where(h => h.HoKhauId == nhanKhau.HoKhauId);  // Sử dụng HoKhauId trực tiếp
                
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(h => h.TrangThai == status);
            }
            
            var hoaDons = await query.OrderByDescending(h => h.NgayTao).ToListAsync();
            
            ViewData["Status"] = new SelectList(new[] 
            { 
                new { Value = "", Text = "Tất cả" },
                new { Value = "ChuaThanhToan", Text = "Chưa thanh toán" },
                new { Value = "DaThanhToan", Text = "Đã thanh toán" },
                new { Value = "QuaHan", Text = "Quá hạn" }
            }, "Value", "Text", status);
            
            return View(hoaDons);
        }

        // GET: HoaDon/CreateBatch
        [Authorize(Policy = "RequireManagerRole")]
        public IActionResult CreateBatch()
        {
            ViewData["LoaiPhiId"] = new SelectList(_context.LoaiPhis, "LoaiPhiId", "TenLoaiPhi");
            return View();
        }

        // POST: HoaDon/CreateBatch
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<IActionResult> CreateBatch(BatchInvoiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                var loaiPhi = await _context.LoaiPhis.FindAsync(model.LoaiPhiId);
                if (loaiPhi == null)
                {
                    ModelState.AddModelError("LoaiPhiId", "Loại phí không hợp lệ");
                    ViewData["LoaiPhiId"] = new SelectList(_context.LoaiPhis, "LoaiPhiId", "TenLoaiPhi");
                    return View(model);
                }

                // Get all active households
                var hoKhaus = await _context.HoKhaus
                    .Where(h => h.TrangThai)
                    .ToListAsync();

                int createdCount = 0;
                foreach (var hoKhau in hoKhaus)
                {
                    var hoaDon = new HoaDon
                    {
                        HoKhauId = hoKhau.HoKhauId,
                        LoaiPhiId = model.LoaiPhiId,
                        MaHoaDon = GenerateInvoiceCode(),
                        TongTien = model.CustomAmount ?? loaiPhi.MucPhi,
                        NgayTao = DateTime.Now,
                        HanThanhToan = model.DueDate,
                        TrangThai = "ChuaThanhToan",
                        GhiChu = model.Description
                    };

                    _context.Add(hoaDon);
                    createdCount++;
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Đã tạo thành công {createdCount} hóa đơn.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["LoaiPhiId"] = new SelectList(_context.LoaiPhis, "LoaiPhiId", "TenLoaiPhi");
            return View(model);
        }

        private bool HoaDonExists(int id)
        {
            return _context.HoaDons.Any(e => e.HoaDonId == id);
        }

        private string GenerateInvoiceCode()
        {
            // Generate a unique code for the invoice
            string prefix = "HD";
            string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            return prefix + timeStamp;
        }
    }
}
