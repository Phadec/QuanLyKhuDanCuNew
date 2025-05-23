using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Add this for SelectList
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
    [Authorize(Policy = "RequireStaffRole")]
    public class NhanKhauController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NhanKhauController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: NhanKhau
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            var query = _context.NhanKhaus
                .Include(n => n.HoKhau)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(n => 
                    n.HoTen.Contains(searchString) || 
                    n.CMND.Contains(searchString) ||
                    n.SoDienThoai.Contains(searchString));
            }

            int pageSize = 10;
            var nhanKhaus = await QuanLyKhuDanCu.Helpers.PaginatedList<NhanKhau>.CreateAsync(query.OrderBy(n => n.HoTen), page, pageSize);

            return View(nhanKhaus);
        }

        // GET: NhanKhau/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanKhau = await _context.NhanKhaus
                .Include(n => n.HoKhau)
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.NhanKhauId == id);

            if (nhanKhau == null)
            {
                return NotFound();
            }

            return View(nhanKhau);
        }

        // GET: NhanKhau/Create
        public IActionResult Create()
        {
            ViewData["HoKhauId"] = new SelectList(_context.HoKhaus, "HoKhauId", "MaHoKhau");
            return View();
        }

        // POST: NhanKhau/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanKhauViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the person already exists
                var existingNhanKhau = await _context.NhanKhaus
                    .FirstOrDefaultAsync(n => n.CMND == model.CMND);

                if (existingNhanKhau != null)
                {
                    ModelState.AddModelError("CMND", "Số CMND đã tồn tại trong hệ thống.");
                    ViewData["HoKhauId"] = new SelectList(_context.HoKhaus, "HoKhauId", "MaHoKhau");
                    return View(model);
                }

                // Create user account if email is provided
                string userId = null;
                if (!string.IsNullOrEmpty(model.Email))
                {
                    // Check if email is already used
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Email đã được sử dụng.");
                        ViewData["HoKhauId"] = new SelectList(_context.HoKhaus, "HoKhauId", "MaHoKhau");
                        return View(model);
                    }

                    // Create new user
                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        HoTen = model.HoTen,
                        NgaySinh = model.NgaySinh,
                        DiaChi = model.DiaChi,
                        CMND = model.CMND,
                        SoDienThoai = model.SoDienThoai,
                        NgayTao = DateTime.Now,
                        TrangThai = true,
                        EmailConfirmed = true
                    };

                    // Generate a temporary password
                    string tempPassword = $"Resident@{DateTime.Now.Year}";
                    var result = await _userManager.CreateAsync(user, tempPassword);

                    if (result.Succeeded)
                    {
                        // Add to resident role
                        await _userManager.AddToRoleAsync(user, "Resident");
                        userId = user.Id;
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        ViewData["HoKhauId"] = new SelectList(_context.HoKhaus, "HoKhauId", "MaHoKhau");
                        return View(model);
                    }
                }

                var nhanKhau = new NhanKhau
                {
                    HoTen = model.HoTen,
                    NgaySinh = model.NgaySinh,
                    GioiTinh = model.GioiTinh,
                    CMND = model.CMND,
                    QuocTich = model.QuocTich,
                    NgheNghiep = model.NgheNghiep,
                    NoiLamViec = model.NoiLamViec,
                    QuanHeVoiChuHo = model.QuanHeVoiChuHo,
                    SoDienThoai = model.SoDienThoai,
                    Email = model.Email,
                    NgayDangKy = DateTime.Now,
                    TrangThai = true,
                    HoKhauId = model.HoKhauId,
                    UserId = userId
                };

                _context.Add(nhanKhau);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["HoKhauId"] = new SelectList(_context.HoKhaus, "HoKhauId", "MaHoKhau");
            return View(model);
        }

        // GET: NhanKhau/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanKhau = await _context.NhanKhaus.FindAsync(id);
            if (nhanKhau == null)
            {
                return NotFound();
            }

            var viewModel = new NhanKhauViewModel
            {
                NhanKhauId = nhanKhau.NhanKhauId,
                HoTen = nhanKhau.HoTen,
                NgaySinh = nhanKhau.NgaySinh,
                GioiTinh = nhanKhau.GioiTinh,
                CMND = nhanKhau.CMND,
                QuocTich = nhanKhau.QuocTich,
                NgheNghiep = nhanKhau.NgheNghiep,
                NoiLamViec = nhanKhau.NoiLamViec,
                QuanHeVoiChuHo = nhanKhau.QuanHeVoiChuHo,
                SoDienThoai = nhanKhau.SoDienThoai,
                Email = nhanKhau.Email,
                HoKhauId = nhanKhau.HoKhauId,
                DiaChi = _context.HoKhaus.FirstOrDefault(h => h.HoKhauId == nhanKhau.HoKhauId)?.DiaChi
            };

            ViewData["HoKhauId"] = new SelectList(_context.HoKhaus, "HoKhauId", "MaHoKhau");
            return View(viewModel);
        }

        // POST: NhanKhau/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NhanKhauViewModel model)
        {
            if (id != model.NhanKhauId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var nhanKhau = await _context.NhanKhaus.FindAsync(id);
                    if (nhanKhau == null)
                    {
                        return NotFound();
                    }

                    nhanKhau.HoTen = model.HoTen;
                    nhanKhau.NgaySinh = model.NgaySinh;
                    nhanKhau.GioiTinh = model.GioiTinh;
                    nhanKhau.CMND = model.CMND;
                    nhanKhau.QuocTich = model.QuocTich;
                    nhanKhau.NgheNghiep = model.NgheNghiep;
                    nhanKhau.NoiLamViec = model.NoiLamViec;
                    nhanKhau.QuanHeVoiChuHo = model.QuanHeVoiChuHo;
                    nhanKhau.SoDienThoai = model.SoDienThoai;
                    nhanKhau.Email = model.Email;
                    nhanKhau.HoKhauId = model.HoKhauId;

                    // Update user info if exists
                    if (!string.IsNullOrEmpty(nhanKhau.UserId))
                    {
                        var user = await _userManager.FindByIdAsync(nhanKhau.UserId);
                        if (user != null)
                        {
                            user.HoTen = model.HoTen;
                            user.NgaySinh = model.NgaySinh;
                            user.CMND = model.CMND;
                            user.SoDienThoai = model.SoDienThoai;
                            
                            // Handle email change if needed
                            if (user.Email != model.Email)
                            {
                                var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
                                await _userManager.ChangeEmailAsync(user, model.Email, token);
                                user.UserName = model.Email;
                            }
                            
                            await _userManager.UpdateAsync(user);
                        }
                    }

                    _context.Update(nhanKhau);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanKhauExists(id))
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
            return View(model);
        }

        // GET: NhanKhau/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanKhau = await _context.NhanKhaus
                .Include(n => n.HoKhau)
                .FirstOrDefaultAsync(m => m.NhanKhauId == id);

            if (nhanKhau == null)
            {
                return NotFound();
            }

            return View(nhanKhau);
        }

        // POST: NhanKhau/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nhanKhau = await _context.NhanKhaus.FindAsync(id);
            
            // Delete user account if exists
            if (!string.IsNullOrEmpty(nhanKhau.UserId))
            {
                var user = await _userManager.FindByIdAsync(nhanKhau.UserId);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
            }
            
            _context.NhanKhaus.Remove(nhanKhau);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NhanKhauExists(int id)
        {
            return _context.NhanKhaus.Any(e => e.NhanKhauId == id);
        }
    }
}
