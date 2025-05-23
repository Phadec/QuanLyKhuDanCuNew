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
    public class YeuCauDichVuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public YeuCauDichVuController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: YeuCauDichVu
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> Index(string searchString, string status, int page = 1)
        {
            var query = _context.YeuCauDichVus
                .Include(y => y.DichVu)
                .Include(y => y.User)
                .Include(y => y.NguoiXuLy)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(y => 
                    y.User.HoTen.Contains(searchString) || 
                    y.DichVu.TenDichVu.Contains(searchString) ||
                    y.NoiDung.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(y => y.TrangThai == status);
            }

            int pageSize = 10;
            var yeuCauDichVus = await QuanLyKhuDanCu.Helpers.PaginatedList<YeuCauDichVu>.CreateAsync(
                query.OrderByDescending(y => y.NgayYeuCau), page, pageSize);

            ViewData["Status"] = new SelectList(new[] 
            { 
                new { Value = "", Text = "Tất cả" },
                new { Value = "ChoXuLy", Text = "Chờ xử lý" },
                new { Value = "DangXuLy", Text = "Đang xử lý" },
                new { Value = "DaHoanThanh", Text = "Đã hoàn thành" },
                new { Value = "TuChoi", Text = "Từ chối" }
            }, "Value", "Text", status);

            return View(yeuCauDichVus);
        }

        // GET: YeuCauDichVu/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var yeuCauDichVu = await _context.YeuCauDichVus
                .Include(y => y.DichVu)
                .Include(y => y.User)
                .Include(y => y.NguoiXuLy)
                .FirstOrDefaultAsync(m => m.YeuCauDichVuId == id);

            if (yeuCauDichVu == null)
            {
                return NotFound();
            }

            // Check authorization for residents
            if (User.IsInRole("Resident"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (yeuCauDichVu.UserId != currentUser.Id)
                {
                    return Forbid();
                }
            }

            return View(yeuCauDichVu);
        }

        // GET: YeuCauDichVu/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            ViewData["DichVuId"] = new SelectList(_context.DichVus.Where(d => d.TrangThai), "DichVuId", "TenDichVu");
            
            // For staff creating on behalf of a resident
            if (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff"))
            {
                ViewData["UserId"] = new SelectList(_context.Users, "Id", "HoTen");
                ViewData["IsStaff"] = true;
            }
            else
            {
                ViewData["IsStaff"] = false;
            }
            
            return View();
        }

        // POST: YeuCauDichVu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(YeuCauDichVuViewModel model)
        {
            if (ModelState.IsValid)
            {
                string userId;
                
                // For staff creating on behalf of a resident
                if (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff"))
                {
                    userId = model.UserId;
                }
                else
                {
                    // For residents creating their own requests
                    var currentUser = await _userManager.GetUserAsync(User);
                    userId = currentUser.Id;
                }
                
                var yeuCauDichVu = new YeuCauDichVu
                {
                    DichVuId = model.DichVuId,
                    UserId = userId,
                    NgayYeuCau = DateTime.Now,
                    NoiDung = model.NoiDung,
                    TrangThai = "ChoXuLy",
                    GhiChu = model.GhiChu
                };

                _context.Add(yeuCauDichVu);
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
            
            ViewData["DichVuId"] = new SelectList(_context.DichVus.Where(d => d.TrangThai), "DichVuId", "TenDichVu", model.DichVuId);
            
            // For staff creating on behalf of a resident
            if (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff"))
            {
                ViewData["UserId"] = new SelectList(_context.Users, "Id", "HoTen", model.UserId);
                ViewData["IsStaff"] = true;
            }
            else
            {
                ViewData["IsStaff"] = false;
            }
            
            return View(model);
        }

        // GET: YeuCauDichVu/MyRequests
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> MyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var requests = await _context.YeuCauDichVus
                .Include(y => y.DichVu)
                .Include(y => y.NguoiXuLy)
                .Where(y => y.UserId == user.Id)
                .OrderByDescending(y => y.NgayYeuCau)
                .ToListAsync();
                
            return View(requests);
        }

        // GET: YeuCauDichVu/ProcessRequest/5
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> ProcessRequest(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var yeuCauDichVu = await _context.YeuCauDichVus
                .Include(y => y.DichVu)
                .Include(y => y.User)
                .FirstOrDefaultAsync(m => m.YeuCauDichVuId == id);

            if (yeuCauDichVu == null)
            {
                return NotFound();
            }

            if (yeuCauDichVu.TrangThai == "DaHoanThanh" || yeuCauDichVu.TrangThai == "TuChoi")
            {
                return BadRequest("Yêu cầu này đã được xử lý.");
            }

            var viewModel = new ProcessRequestViewModel
            {
                YeuCauDichVuId = yeuCauDichVu.YeuCauDichVuId,
                TenDichVu = yeuCauDichVu.DichVu.TenDichVu,
                NguoiYeuCau = yeuCauDichVu.User.HoTen,
                NgayYeuCau = yeuCauDichVu.NgayYeuCau,
                NoiDung = yeuCauDichVu.NoiDung,
                TrangThai = yeuCauDichVu.TrangThai,
                GhiChu = yeuCauDichVu.GhiChu
            };

            return View(viewModel);
        }

        // POST: YeuCauDichVu/ProcessRequest/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> ProcessRequest(int id, ProcessRequestViewModel model)
        {
            if (id != model.YeuCauDichVuId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var yeuCauDichVu = await _context.YeuCauDichVus.FindAsync(id);
                if (yeuCauDichVu == null)
                {
                    return NotFound();
                }

                if (yeuCauDichVu.TrangThai == "DaHoanThanh" || yeuCauDichVu.TrangThai == "TuChoi")
                {
                    return BadRequest("Yêu cầu này đã được xử lý.");
                }

                var user = await _userManager.GetUserAsync(User);

                yeuCauDichVu.TrangThai = model.Action;
                yeuCauDichVu.GhiChu = model.GhiChu;
                yeuCauDichVu.NguoiXuLyId = user.Id;
                
                if (model.Action == "DangXuLy" && !yeuCauDichVu.NgayXuLy.HasValue)
                {
                    yeuCauDichVu.NgayXuLy = DateTime.Now;
                }
                else if (model.Action == "DaHoanThanh" || model.Action == "TuChoi")
                {
                    if (!yeuCauDichVu.NgayXuLy.HasValue)
                    {
                        yeuCauDichVu.NgayXuLy = DateTime.Now;
                    }
                    yeuCauDichVu.NgayHoanThanh = DateTime.Now;
                }

                _context.Update(yeuCauDichVu);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }
            
            return View(model);
        }

        // GET: YeuCauDichVu/Cancel/5
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var yeuCauDichVu = await _context.YeuCauDichVus
                .Include(y => y.DichVu)
                .FirstOrDefaultAsync(m => m.YeuCauDichVuId == id);

            if (yeuCauDichVu == null)
            {
                return NotFound();
            }

            // Check if the request belongs to the current user
            var user = await _userManager.GetUserAsync(User);
            if (yeuCauDichVu.UserId != user.Id)
            {
                return Forbid();
            }

            // Only allow cancellation of pending or in-progress requests
            if (yeuCauDichVu.TrangThai != "ChoXuLy" && yeuCauDichVu.TrangThai != "DangXuLy")
            {
                return BadRequest("Không thể hủy yêu cầu đã hoàn thành hoặc từ chối.");
            }

            return View(yeuCauDichVu);
        }

        // POST: YeuCauDichVu/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var yeuCauDichVu = await _context.YeuCauDichVus.FindAsync(id);
            
            // Check again if the request belongs to the current user
            var user = await _userManager.GetUserAsync(User);
            if (yeuCauDichVu.UserId != user.Id)
            {
                return Forbid();
            }
            
            // Only allow cancellation of pending or in-progress requests
            if (yeuCauDichVu.TrangThai != "ChoXuLy" && yeuCauDichVu.TrangThai != "DangXuLy")
            {
                return BadRequest("Không thể hủy yêu cầu đã hoàn thành hoặc từ chối.");
            }
            
            _context.YeuCauDichVus.Remove(yeuCauDichVu);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(MyRequests));
        }
    }
}
