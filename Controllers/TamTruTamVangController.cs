using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhuDanCu.Data;
using QuanLyKhuDanCu.Helpers;
using QuanLyKhuDanCu.Models;
using QuanLyKhuDanCu.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Controllers
{
    public class TamTruTamVangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TamTruTamVangController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: TamTruTamVang
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> Index(string searchString, string status, int page = 1)
        {
            var query = _context.TamTruTamVangs
                .Include(t => t.NhanKhau)
                .Include(t => t.NguoiDuyet)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => 
                    t.NhanKhau.HoTen.Contains(searchString) || 
                    t.LyDo.Contains(searchString) ||
                    t.DiaChiTamTruTamVang.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.TrangThai == status);
            }

            int pageSize = 10;
            var tamTruTamVangs = await QuanLyKhuDanCu.Helpers.PaginatedList<TamTruTamVang>.CreateAsync(
                query.OrderByDescending(t => t.NgayTao), page, pageSize);

            ViewBag.Status = new SelectList(new[]
            {
                new { Value = "", Text = "Tất cả" },
                new { Value = "ChoXuLy", Text = "Chờ xử lý" },
                new { Value = "DaXuLy", Text = "Đã xử lý" },
                new { Value = "TuChoi", Text = "Từ chối" }
            }, "Value", "Text", status);

            return View(tamTruTamVangs);
        }

        // GET: TamTruTamVang/MyRequests
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> MyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            
            // Get all NhanKhau associated with this user
            var nhanKhaus = await _context.NhanKhaus
                .Where(n => n.UserId == user.Id)
                .ToListAsync();
                
            var nhanKhauIds = nhanKhaus.Select(n => n.NhanKhauId).ToList();
            
            var requests = await _context.TamTruTamVangs
                .Include(t => t.NhanKhau)
                .Include(t => t.NguoiDuyet)
                .Where(t => nhanKhauIds.Contains(t.NhanKhauId))
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();
                
            return View(requests);
        }

        // GET: TamTruTamVang/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tamTruTamVang = await _context.TamTruTamVangs
                .Include(t => t.NhanKhau)
                .Include(t => t.NguoiDuyet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tamTruTamVang == null)
            {
                return NotFound();
            }

            // Check if user has permission to view the details
            if (User.IsInRole("Resident"))
            {
                var user = await _userManager.GetUserAsync(User);
                var nhanKhau = await _context.NhanKhaus
                    .FirstOrDefaultAsync(n => n.NhanKhauId == tamTruTamVang.NhanKhauId);
                    
                if (nhanKhau.UserId != user.Id)
                {
                    return Forbid();
                }
            }

            return View(tamTruTamVang);
        }

        // GET: TamTruTamVang/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Resident"))
            {
                var user = await _userManager.GetUserAsync(User);
                
                // Get only NhanKhau associated with this user
                var nhanKhaus = await _context.NhanKhaus
                    .Where(n => n.UserId == user.Id && n.TrangThai)
                    .ToListAsync();
                    
                ViewBag.NhanKhauId = new SelectList(nhanKhaus, "NhanKhauId", "HoTen");
            }
            else
            {
                // For staff, show all active NhanKhau
                ViewBag.NhanKhauId = new SelectList(
                    await _context.NhanKhaus
                        .Where(n => n.TrangThai)
                        .OrderBy(n => n.HoTen)
                        .ToListAsync(), 
                    "NhanKhauId", "HoTen");
            }
            
            return View();
        }

        // POST: TamTruTamVang/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(TamTruTamVangViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate that the user has the right to create for this NhanKhau
                if (User.IsInRole("Resident"))
                {
                    var user = await _userManager.GetUserAsync(User);
                    var nhanKhau = await _context.NhanKhaus
                        .FirstOrDefaultAsync(n => n.NhanKhauId == model.NhanKhauId);
                        
                    if (nhanKhau == null || nhanKhau.UserId != user.Id)
                    {
                        ModelState.AddModelError("NhanKhauId", "Bạn không có quyền đăng ký cho nhân khẩu này.");
                        
                        // Refresh the dropdown
                        var nhanKhaus = await _context.NhanKhaus
                            .Where(n => n.UserId == user.Id && n.TrangThai)
                            .ToListAsync();
                            
                        ViewBag.NhanKhauId = new SelectList(nhanKhaus, "NhanKhauId", "HoTen", model.NhanKhauId);
                        
                        return View(model);
                    }
                }
                
                var tamTruTamVang = new TamTruTamVang
                {
                    NhanKhauId = model.NhanKhauId,
                    LoaiDon = model.LoaiDon,
                    NgayBatDau = model.NgayBatDau,
                    NgayKetThuc = model.NgayKetThuc,
                    LyDo = model.LyDo,
                    DiaChiTamTruTamVang = model.DiaChiTamTruTamVang,
                    GhiChu = model.GhiChu,
                    NgayTao = DateTime.Now,
                    TrangThai = "ChoXuLy",
                    // We don't set NguoiDuyetId here - it remains null until approval
                };
                
                // If it's a staff member creating, set it to approved automatically
                if (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff"))
                {
                    var user = await _userManager.GetUserAsync(User);
                    tamTruTamVang.TrangThai = "DaXuLy";
                    tamTruTamVang.NgayDuyet = DateTime.Now;
                    tamTruTamVang.NguoiDuyetId = user.Id;
                }

                _context.Add(tamTruTamVang);
                await _context.SaveChangesAsync();
                
                if (User.IsInRole("Resident"))
                {
                    return RedirectToAction(nameof(MyRequests));
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            
            // If we got this far, something failed, redisplay form
            if (User.IsInRole("Resident"))
            {
                var user = await _userManager.GetUserAsync(User);
                var nhanKhaus = await _context.NhanKhaus
                    .Where(n => n.UserId == user.Id && n.TrangThai)
                    .ToListAsync();
                    
                ViewBag.NhanKhauId = new SelectList(nhanKhaus, "NhanKhauId", "HoTen", model.NhanKhauId);
            }
            else
            {
                ViewBag.NhanKhauId = new SelectList(
                    await _context.NhanKhaus
                        .Where(n => n.TrangThai)
                        .OrderBy(n => n.HoTen)
                        .ToListAsync(), 
                    "NhanKhauId", "HoTen", model.NhanKhauId);
            }
            
            return View(model);
        }

        // GET: TamTruTamVang/Approve/5
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tamTruTamVang = await _context.TamTruTamVangs
                .Include(t => t.NhanKhau)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tamTruTamVang == null)
            {
                return NotFound();
            }

            if (tamTruTamVang.TrangThai != "ChoDuyet")
            {
                return BadRequest("Đơn này đã được xử lý.");
            }

            return View(tamTruTamVang);
        }

        // POST: TamTruTamVang/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> Approve(int id, string decision, string ghiChu)
        {
            var tamTruTamVang = await _context.TamTruTamVangs.FindAsync(id);
            if (tamTruTamVang == null)
            {
                return NotFound();
            }

            if (tamTruTamVang.TrangThai != "ChoDuyet")
            {
                return BadRequest("Đơn này đã được xử lý.");
            }

            var user = await _userManager.GetUserAsync(User);

            tamTruTamVang.TrangThai = decision == "approve" ? "DaDuyet" : "TuChoi";
            tamTruTamVang.NguoiDuyetId = user.Id;
            tamTruTamVang.NgayDuyet = DateTime.Now;
            tamTruTamVang.GhiChu = ghiChu;

            _context.Update(tamTruTamVang);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: TamTruTamVang/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tamTruTamVang = await _context.TamTruTamVangs
                .Include(t => t.NhanKhau)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tamTruTamVang == null)
            {
                return NotFound();
            }

            // Only allow deletion of pending requests
            if (tamTruTamVang.TrangThai != "ChoDuyet")
            {
                return BadRequest("Không thể xóa đơn đã được xử lý.");
            }

            // For residents, check if it's their own request
            if (User.IsInRole("Resident"))
            {
                var user = await _userManager.GetUserAsync(User);
                var nhanKhau = await _context.NhanKhaus.FirstOrDefaultAsync(n => n.UserId == user.Id);
                
                if (nhanKhau == null || tamTruTamVang.NhanKhauId != nhanKhau.NhanKhauId)
                {
                    return Forbid();
                }
            }

            return View(tamTruTamVang);
        }

        // POST: TamTruTamVang/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tamTruTamVang = await _context.TamTruTamVangs.FindAsync(id);
            
            // Security check again
            if (tamTruTamVang.TrangThai != "ChoDuyet")
            {
                return BadRequest("Không thể xóa đơn đã được xử lý.");
            }
            
            if (User.IsInRole("Resident"))
            {
                var user = await _userManager.GetUserAsync(User);
                var nhanKhau = await _context.NhanKhaus.FirstOrDefaultAsync(n => n.UserId == user.Id);
                
                if (nhanKhau == null || tamTruTamVang.NhanKhauId != nhanKhau.NhanKhauId)
                {
                    return Forbid();
                }
            }
            
            _context.TamTruTamVangs.Remove(tamTruTamVang);
            await _context.SaveChangesAsync();
            
            if (User.IsInRole("Resident"))
            {
                return RedirectToAction("MyRequests");
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
