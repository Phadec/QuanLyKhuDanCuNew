using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhuDanCu.Data;
using QuanLyKhuDanCu.Helpers; // Add this import (already exists)
using QuanLyKhuDanCu.Models;
using QuanLyKhuDanCu.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace QuanLyKhuDanCu.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ThongBaoController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: ThongBao
        [Authorize]
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            var query = _context.ThongBaos.AsQueryable();

            // Filter by role
            if (roles.Contains("Admin") || roles.Contains("Manager") || roles.Contains("Staff"))
            {
                // Staff can see all except those targeted to Residents only
                query = query.Where(t => t.DoiTuong != "Resident" || t.NguoiTaoId == user.Id);
            }
            else if (roles.Contains("Resident"))
            {
                // Residents can see only public announcements or those targeted to residents
                query = query.Where(t => t.DoiTuong == "TatCa" || t.DoiTuong == "Resident");
            }

            // Filter by search term
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => 
                    t.TieuDe.Contains(searchString) || 
                    t.NoiDung.Contains(searchString));
            }

            // Hide expired announcements for regular users
            if (!roles.Contains("Admin") && !roles.Contains("Manager"))
            {
                query = query.Where(t => 
                    t.TrangThai && 
                    (t.NgayHetHan == null || t.NgayHetHan > DateTime.Now));
            }

            int pageSize = 10;
            var thongBaos = await QuanLyKhuDanCu.Helpers.PaginatedList<ThongBao>.CreateAsync(
                query.Include(t => t.NguoiTao)
                     .OrderByDescending(t => t.NgayTao), 
                page, pageSize);

            return View(thongBaos);
        }

        // GET: ThongBao/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thongBao = await _context.ThongBaos
                .Include(t => t.NguoiTao)
                .FirstOrDefaultAsync(m => m.ThongBaoId == id);

            if (thongBao == null)
            {
                return NotFound();
            }

            // Check if the user has permission to view this announcement
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            bool canView = false;
            
            if (roles.Contains("Admin") || roles.Contains("Manager"))
            {
                // Admins and managers can see all announcements
                canView = true;
            }
            else if (roles.Contains("Staff"))
            {
                // Staff can see all except those for residents only (unless they created it)
                canView = (thongBao.DoiTuong != "Resident" || thongBao.NguoiTaoId == user.Id);
            }
            else if (roles.Contains("Resident"))
            {
                // Residents can see only public announcements or those for residents
                canView = (thongBao.DoiTuong == "TatCa" || thongBao.DoiTuong == "Resident");
            }

            if (!canView)
            {
                return Forbid();
            }

            return View(thongBao);
        }

        // GET: ThongBao/Create
        [Authorize(Policy = "RequireStaffRole")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ThongBao/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> Create(ThongBaoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                
                var thongBao = new ThongBao
                {
                    TieuDe = model.TieuDe,
                    NoiDung = model.NoiDung,
                    NgayTao = DateTime.Now,
                    NgayHetHan = model.NgayHetHan,
                    DoiTuong = model.DoiTuong,
                    TrangThai = true,
                    NguoiTaoId = user.Id
                };

                // Handle file upload
                if (model.FileDinhKem != null && model.FileDinhKem.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "thongbao");
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

                    thongBao.FileDinhKem = uniqueFileName;
                }

                _context.Add(thongBao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: ThongBao/Edit/5
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thongBao = await _context.ThongBaos.FindAsync(id);
            if (thongBao == null)
            {
                return NotFound();
            }

            // Check if user has rights to edit this announcement
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && thongBao.NguoiTaoId != user.Id)
            {
                return Forbid();
            }

            var viewModel = new ThongBaoViewModel
            {
                ThongBaoId = thongBao.ThongBaoId,
                TieuDe = thongBao.TieuDe,
                NoiDung = thongBao.NoiDung,
                NgayHetHan = thongBao.NgayHetHan,
                DoiTuong = thongBao.DoiTuong,
                TrangThai = thongBao.TrangThai,
                ExistingFilePath = thongBao.FileDinhKem
            };

            return View(viewModel);
        }

        // POST: ThongBao/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> Edit(int id, ThongBaoViewModel model)
        {
            if (id != model.ThongBaoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var thongBao = await _context.ThongBaos.FindAsync(id);
                    if (thongBao == null)
                    {
                        return NotFound();
                    }

                    // Check if user has rights to edit this announcement
                    var user = await _userManager.GetUserAsync(User);
                    var isAdmin = User.IsInRole("Admin");
                    
                    if (!isAdmin && thongBao.NguoiTaoId != user.Id)
                    {
                        return Forbid();
                    }

                    thongBao.TieuDe = model.TieuDe;
                    thongBao.NoiDung = model.NoiDung;
                    thongBao.NgayHetHan = model.NgayHetHan;
                    thongBao.DoiTuong = model.DoiTuong;
                    thongBao.TrangThai = model.TrangThai;

                    // Handle file upload if a new file is provided
                    if (model.FileDinhKem != null && model.FileDinhKem.Length > 0)
                    {
                        // Delete the old file if it exists
                        if (!string.IsNullOrEmpty(thongBao.FileDinhKem))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "thongbao", thongBao.FileDinhKem);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Save the new file
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "thongbao");
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

                        thongBao.FileDinhKem = uniqueFileName;
                    }

                    _context.Update(thongBao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThongBaoExists(id))
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
            return View(model);
        }

        // GET: ThongBao/Delete/5
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thongBao = await _context.ThongBaos
                .Include(t => t.NguoiTao)
                .FirstOrDefaultAsync(m => m.ThongBaoId == id);

            if (thongBao == null)
            {
                return NotFound();
            }

            // Check if user has rights to delete this announcement
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && thongBao.NguoiTaoId != user.Id)
            {
                return Forbid();
            }

            return View(thongBao);
        }

        // POST: ThongBao/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var thongBao = await _context.ThongBaos.FindAsync(id);
            
            // Check if user has rights to delete this announcement
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && thongBao.NguoiTaoId != user.Id)
            {
                return Forbid();
            }

            // Delete the file if it exists
            if (!string.IsNullOrEmpty(thongBao.FileDinhKem))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "thongbao", thongBao.FileDinhKem);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            
            _context.ThongBaos.Remove(thongBao);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ThongBao/DownloadFile/5
        [Authorize]
        public async Task<IActionResult> DownloadFile(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thongBao = await _context.ThongBaos.FindAsync(id);
            if (thongBao == null || string.IsNullOrEmpty(thongBao.FileDinhKem))
            {
                return NotFound();
            }

            // Check if the user has permission to view this announcement
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            bool canView = false;
            
            if (roles.Contains("Admin") || roles.Contains("Manager"))
            {
                canView = true;
            }
            else if (roles.Contains("Staff"))
            {
                canView = (thongBao.DoiTuong != "Resident" || thongBao.NguoiTaoId == user.Id);
            }
            else if (roles.Contains("Resident"))
            {
                canView = (thongBao.DoiTuong == "TatCa" || thongBao.DoiTuong == "Resident");
            }

            if (!canView)
            {
                return Forbid();
            }

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "thongbao", thongBao.FileDinhKem);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            string fileName = Path.GetFileName(thongBao.FileDinhKem).Substring(thongBao.FileDinhKem.IndexOf('_') + 1);
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        private bool ThongBaoExists(int id)
        {
            return _context.ThongBaos.Any(e => e.ThongBaoId == id);
        }
    }
}
