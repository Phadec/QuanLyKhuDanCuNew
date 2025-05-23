using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Add this for SelectList
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
    public class PhanAnhController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PhanAnhController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: PhanAnh
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> Index(string searchString, string status, int page = 1)
        {
            var query = _context.PhanAnhs
                .Include(p => p.User)
                .Include(p => p.NguoiXuLy)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => 
                    p.TieuDe.Contains(searchString) || 
                    p.NoiDung.Contains(searchString) ||
                    p.User.HoTen.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.TrangThai == status);
            }

            int pageSize = 10;
            var phanAnhs = await QuanLyKhuDanCu.Helpers.PaginatedList<PhanAnh>.CreateAsync(
                query.OrderByDescending(p => p.NgayTao), page, pageSize);

            ViewData["Status"] = new SelectList(new[] 
            { 
                new { Value = "", Text = "Tất cả" },
                new { Value = "ChoXuLy", Text = "Chờ xử lý" },
                new { Value = "DangXuLy", Text = "Đang xử lý" },
                new { Value = "DaXuLy", Text = "Đã xử lý" }
            }, "Value", "Text", status);

            return View(phanAnhs);
        }

        // GET: PhanAnh/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phanAnh = await _context.PhanAnhs
                .Include(p => p.User)
                .Include(p => p.NguoiXuLy)
                .FirstOrDefaultAsync(m => m.PhanAnhId == id);

            if (phanAnh == null)
            {
                return NotFound();
            }

            // Check authorization for residents
            if (User.IsInRole("Resident"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (phanAnh.UserId != currentUser.Id)
                {
                    return Forbid();
                }
            }

            return View(phanAnh);
        }

        // GET: PhanAnh/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: PhanAnh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(PhanAnhViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                
                var phanAnh = new PhanAnh
                {
                    UserId = user.Id,
                    TieuDe = model.TieuDe,
                    NoiDung = model.NoiDung,
                    NgayTao = DateTime.Now,
                    TrangThai = "ChoXuLy",
                    PhanHoi = string.Empty, // Initialize PhanHoi with empty string to avoid null
                    FileDinhKem = string.Empty // Default empty if no file
                };
                
                // Handle file upload if provided
                if (model.FileDinhKem != null && model.FileDinhKem.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "phananh");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.FileDinhKem.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.FileDinhKem.CopyToAsync(fileStream);
                    }

                    phanAnh.FileDinhKem = uniqueFileName;
                }

                _context.PhanAnhs.Add(phanAnh);
                await _context.SaveChangesAsync();
                
                if (User.IsInRole("Resident"))
                {
                    return RedirectToAction(nameof(MyFeedback));
                }
                return RedirectToAction(nameof(Index));
            }
            
            return View(model);
        }

        // GET: PhanAnh/MyFeedback
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> MyFeedback()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var feedback = await _context.PhanAnhs
                .Include(p => p.NguoiXuLy)
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.NgayTao)
                .ToListAsync();
                
            return View(feedback);
        }

        // GET: PhanAnh/ProcessFeedback/5
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> ProcessFeedback(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phanAnh = await _context.PhanAnhs
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PhanAnhId == id);

            if (phanAnh == null)
            {
                return NotFound();
            }

            if (phanAnh.TrangThai == "DaXuLy")
            {
                return BadRequest("Phản ánh này đã được xử lý.");
            }

            var viewModel = new ProcessFeedbackViewModel
            {
                PhanAnhId = phanAnh.PhanAnhId,
                TieuDe = phanAnh.TieuDe,
                NoiDung = phanAnh.NoiDung,
                NgayTao = phanAnh.NgayTao,
                TrangThai = phanAnh.TrangThai,
                NguoiGui = phanAnh.User.HoTen,
                PhanHoi = phanAnh.PhanHoi
            };

            return View(viewModel);
        }

        // POST: PhanAnh/ProcessFeedback/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> ProcessFeedback(int id, ProcessFeedbackViewModel model)
        {
            if (id != model.PhanAnhId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var phanAnh = await _context.PhanAnhs.FindAsync(id);
                if (phanAnh == null)
                {
                    return NotFound();
                }

                if (phanAnh.TrangThai == "DaXuLy")
                {
                    return BadRequest("Phản ánh này đã được xử lý.");
                }

                var user = await _userManager.GetUserAsync(User);

                phanAnh.TrangThai = model.Action;
                phanAnh.PhanHoi = model.PhanHoi;
                phanAnh.NguoiXuLyId = user.Id;
                
                if (model.Action == "DaXuLy")
                {
                    phanAnh.NgayXuLy = DateTime.Now;
                }

                _context.Update(phanAnh);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }
            
            return View(model);
        }

        // GET: PhanAnh/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phanAnh = await _context.PhanAnhs
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PhanAnhId == id);

            if (phanAnh == null)
            {
                return NotFound();
            }

            // Check authorization
            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && phanAnh.UserId != currentUser.Id)
            {
                return Forbid();
            }
            
            // Residents can only delete pending feedback
            if (User.IsInRole("Resident") && phanAnh.TrangThai != "ChoXuLy")
            {
                return BadRequest("Không thể xóa phản ánh đã được xử lý.");
            }

            return View(phanAnh);
        }

        // POST: PhanAnh/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phanAnh = await _context.PhanAnhs.FindAsync(id);
            
            // Check authorization again
            var currentUser = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && phanAnh.UserId != currentUser.Id)
            {
                return Forbid();
            }
            
            // Residents can only delete pending feedback
            if (User.IsInRole("Resident") && phanAnh.TrangThai != "ChoXuLy")
            {
                return BadRequest("Không thể xóa phản ánh đã được xử lý.");
            }
            
            // Delete the file if it exists
            if (!string.IsNullOrEmpty(phanAnh.FileDinhKem))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "phananh", phanAnh.FileDinhKem);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            
            _context.PhanAnhs.Remove(phanAnh);
            await _context.SaveChangesAsync();
            
            if (User.IsInRole("Resident"))
            {
                return RedirectToAction(nameof(MyFeedback));
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: PhanAnh/DownloadFile/5
        [Authorize]
        public async Task<IActionResult> DownloadFile(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phanAnh = await _context.PhanAnhs.FindAsync(id);
            if (phanAnh == null || string.IsNullOrEmpty(phanAnh.FileDinhKem))
            {
                return NotFound();
            }

            // Check authorization
            if (User.IsInRole("Resident"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (phanAnh.UserId != currentUser.Id)
                {
                    return Forbid();
                }
            }

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "phananh", phanAnh.FileDinhKem);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            string fileName = Path.GetFileName(phanAnh.FileDinhKem).Substring(phanAnh.FileDinhKem.IndexOf('_') + 1);
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}
