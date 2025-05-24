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
                var user = await _userManager.GetUserAsync(User);
                
                // For resident, use their own ID
                string userId = user.Id;
                
                // For staff creating request on behalf of someone else
                if (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff"))
                {
                    // If UserId is provided, use it, otherwise use the staff's ID
                    if (!string.IsNullOrEmpty(model.UserId))
                    {
                        userId = model.UserId;
                    }
                }
                
                var yeuCauDichVu = new YeuCauDichVu
                {
                    DichVuId = model.DichVuId,
                    UserId = userId,
                    NgayYeuCau = DateTime.Now,
                    NoiDung = model.NoiDung,
                    GhiChu = model.GhiChu ?? string.Empty,
                    TrangThai = "ChoXuLy",
                    // Don't set NguoiXuLyId initially
                    NguoiXuLyId = null,
                    NgayXuLy = null,
                    NgayHoanThanh = null
                };
                
                // If it's staff creating the request, automatically mark it as being processed
                if (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff"))
                {
                    yeuCauDichVu.TrangThai = "DangXuLy";
                    yeuCauDichVu.NguoiXuLyId = user.Id; // Set the staff member as the processor
                    yeuCauDichVu.NgayXuLy = DateTime.Now;
                }
                
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
            
            // If ModelState is not valid
            // Prepare the dropdown list for DichVu
            ViewBag.DichVuId = new SelectList(await _context.DichVus.Where(d => d.TrangThai).ToListAsync(), "DichVuId", "TenDichVu", model.DichVuId);
            
            // If it's staff, prepare the user dropdown
            if (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff"))
            {
                var residentsWithAccounts = await _userManager.GetUsersInRoleAsync("Resident");
                ViewBag.UserId = new SelectList(residentsWithAccounts, "Id", "HoTen", model.UserId);
                ViewData["IsStaff"] = true;
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
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Staff")]
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

            // Only allow processing if the status is either ChoXuLy or DangXuLy
            if (yeuCauDichVu.TrangThai != "ChoXuLy" && yeuCauDichVu.TrangThai != "DangXuLy")
            {
                return RedirectToAction(nameof(Details), new { id = yeuCauDichVu.YeuCauDichVuId });
            }

            var model = new ProcessRequestViewModel
            {
                YeuCauDichVuId = yeuCauDichVu.YeuCauDichVuId,
                TenDichVu = yeuCauDichVu.DichVu?.TenDichVu ?? "Không rõ",
                NguoiYeuCau = yeuCauDichVu.User?.HoTen ?? "Không rõ",
                NgayYeuCau = yeuCauDichVu.NgayYeuCau,
                NoiDung = yeuCauDichVu.NoiDung,
                TrangThai = yeuCauDichVu.TrangThai,
                GhiChu = yeuCauDichVu.GhiChu
            };

            return View(model);
        }

        // POST: YeuCauDichVu/ProcessRequest/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> ProcessRequest(ProcessRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var yeuCauDichVu = await _context.YeuCauDichVus.FindAsync(model.YeuCauDichVuId);
            if (yeuCauDichVu == null)
            {
                return NotFound();
            }

            // Get current user as the processor
            var user = await _userManager.GetUserAsync(User);

            // Update status based on the selected action
            yeuCauDichVu.TrangThai = model.Action;
            
            // Add timestamp to notes
            yeuCauDichVu.GhiChu = string.IsNullOrEmpty(yeuCauDichVu.GhiChu) 
                ? $"[{DateTime.Now:dd/MM/yyyy HH:mm}] {model.GhiChu}" 
                : $"{yeuCauDichVu.GhiChu}\n\n[{DateTime.Now:dd/MM/yyyy HH:mm}] {model.GhiChu}";
            
            // If the request is accepted for processing
            if (model.Action == "DangXuLy")
            {
                // Set the current user as processor if not already set
                if (string.IsNullOrEmpty(yeuCauDichVu.NguoiXuLyId))
                {
                    yeuCauDichVu.NguoiXuLyId = user.Id;
                }
                
                // Set processing start date if not already set
                if (!yeuCauDichVu.NgayXuLy.HasValue)
                {
                    yeuCauDichVu.NgayXuLy = DateTime.Now;
                }
            }
            // If the request is completed
            else if (model.Action == "DaHoanThanh")
            {
                // Set completion date
                yeuCauDichVu.NgayHoanThanh = DateTime.Now;
                
                // Ensure processor is set
                if (string.IsNullOrEmpty(yeuCauDichVu.NguoiXuLyId))
                {
                    yeuCauDichVu.NguoiXuLyId = user.Id;
                }
                
                // Ensure processing start date is set
                if (!yeuCauDichVu.NgayXuLy.HasValue)
                {
                    yeuCauDichVu.NgayXuLy = DateTime.Now;
                }
            }
            // If the request is rejected
            else if (model.Action == "TuChoi")
            {
                // Set processor
                if (string.IsNullOrEmpty(yeuCauDichVu.NguoiXuLyId))
                {
                    yeuCauDichVu.NguoiXuLyId = user.Id;
                }
                
                // Set processing date
                if (!yeuCauDichVu.NgayXuLy.HasValue)
                {
                    yeuCauDichVu.NgayXuLy = DateTime.Now;
                }
            }

            _context.Update(yeuCauDichVu);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = yeuCauDichVu.YeuCauDichVuId });
        }

        // GET: YeuCauDichVu/Cancel/5
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Cancel(int? id)
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

            // Only allow cancellation if:
            // 1. The user is the owner of the request and it's still pending
            // 2. Or the user is an admin/manager/staff
            var currentUserId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("Staff"))
            {
                if (yeuCauDichVu.UserId != currentUserId || yeuCauDichVu.TrangThai != "ChoXuLy")
                {
                    return Forbid();
                }
            }

            return View(yeuCauDichVu);
        }

        // POST: YeuCauDichVu/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Cancel(int id, string lyDoHuy)
        {
            var yeuCauDichVu = await _context.YeuCauDichVus
                .Include(y => y.DichVu)
                .Include(y => y.User)
                .FirstOrDefaultAsync(m => m.YeuCauDichVuId == id);
                
            if (yeuCauDichVu == null)
            {
                return NotFound();
            }

            // Only allow cancellation if:
            // 1. The user is the owner of the request and it's still pending
            // 2. Or the user is an admin/manager/staff
            var currentUserId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("Staff"))
            {
                if (yeuCauDichVu.UserId != currentUserId || yeuCauDichVu.TrangThai != "ChoXuLy")
                {
                    return Forbid();
                }
            }

            // Update status and add cancellation reason to the note
            yeuCauDichVu.TrangThai = "TuChoi";
            yeuCauDichVu.GhiChu = (string.IsNullOrEmpty(yeuCauDichVu.GhiChu) ? "" : yeuCauDichVu.GhiChu + "\n\n") + 
                                   $"[Hủy bởi {User.Identity?.Name} - {DateTime.Now:dd/MM/yyyy HH:mm}]\nLý do: {lyDoHuy}";
    
            if (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff"))
            {
                // If it's staff canceling, set them as the processor
                yeuCauDichVu.NguoiXuLyId = currentUserId;
                yeuCauDichVu.NgayXuLy = DateTime.Now;
            }

            _context.Update(yeuCauDichVu);
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
    }
}
