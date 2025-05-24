using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhuDanCu.Data;
using QuanLyKhuDanCu.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Controllers
{
    [Authorize(Roles = "Admin,Manager,Staff")]
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
        public async Task<IActionResult> Index(int? hoKhauId)
        {
            var nhanKhaus = _context.NhanKhaus
                .Include(n => n.HoKhau)
                .Include(n => n.User)
                .AsQueryable();

            if (hoKhauId.HasValue)
            {
                nhanKhaus = nhanKhaus.Where(n => n.HoKhauId == hoKhauId);
                var hoKhau = await _context.HoKhaus.FindAsync(hoKhauId);
                ViewData["HoKhauInfo"] = hoKhau;
            }

            return View(await nhanKhaus.ToListAsync());
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
        public async Task<IActionResult> Create(int? hoKhauId)
        {
            var hoKhaus = await _context.HoKhaus.ToListAsync();
            ViewData["HoKhauId"] = new SelectList(hoKhaus, "HoKhauId", "MaHoKhau", hoKhauId);
            
            var users = await _userManager.Users
                .Where(u => !_context.NhanKhaus.Any(n => n.UserId == u.Id))
                .ToListAsync();
            ViewData["UserId"] = new SelectList(users, "Id", "HoTen");

            if (hoKhauId.HasValue)
            {
                var hoKhau = await _context.HoKhaus.FindAsync(hoKhauId);
                ViewData["HoKhauInfo"] = hoKhau;
                // Pre-fill the model with hoKhauId
                var model = new NhanKhau
                {
                    HoKhauId = hoKhauId.Value,
                    NgaySinh = DateTime.Now.AddYears(-18), // Default age
                    QuocTich = "Việt Nam", // Default
                    TrangThai = true, // Default active
                    NgayDangKy = DateTime.Now // Current date
                };
                return View(model);
            }

            return View(new NhanKhau { NgaySinh = DateTime.Now.AddYears(-18), QuocTich = "Việt Nam", TrangThai = true, NgayDangKy = DateTime.Now });
        }

        // POST: NhanKhau/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HoTen,NgaySinh,GioiTinh,CMND,SoDienThoai,Email,QuocTich,NgheNghiep,NoiLamViec,QuanHeVoiChuHo,HoKhauId,UserId,TrangThai")] NhanKhau nhanKhau)
        {
            if (ModelState.IsValid)
            {
                nhanKhau.NgayDangKy = DateTime.Now;
                _context.Add(nhanKhau);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm nhân khẩu thành công!";
                return RedirectToAction(nameof(Details), "HoKhau", new { id = nhanKhau.HoKhauId });
            }
            
            var hoKhaus = await _context.HoKhaus.ToListAsync();
            ViewData["HoKhauId"] = new SelectList(hoKhaus, "HoKhauId", "MaHoKhau", nhanKhau.HoKhauId);
            
            var users = await _userManager.Users
                .Where(u => !_context.NhanKhaus.Any(nk => nk.UserId == u.Id) || (nhanKhau.UserId != null && u.Id == nhanKhau.UserId))
                .ToListAsync();
            ViewData["UserId"] = new SelectList(users, "Id", "HoTen", nhanKhau.UserId);
            
            if (nhanKhau.HoKhauId > 0)
            {
                var hoKhau = await _context.HoKhaus.FindAsync(nhanKhau.HoKhauId);
                ViewData["HoKhauInfo"] = hoKhau;
            }
            
            return View(nhanKhau);
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
            
            var hoKhaus = await _context.HoKhaus.ToListAsync();
            ViewData["HoKhauId"] = new SelectList(hoKhaus, "HoKhauId", "MaHoKhau", nhanKhau.HoKhauId);
            
            var users = await _userManager.Users
                .Where(u => !_context.NhanKhaus.Any(nk => nk.UserId == u.Id && nk.NhanKhauId != id) || u.Id == nhanKhau.UserId)
                .ToListAsync();
            ViewData["UserId"] = new SelectList(users, "Id", "HoTen", nhanKhau.UserId);
            
            return View(nhanKhau);
        }

        // POST: NhanKhau/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NhanKhauId,HoTen,NgaySinh,GioiTinh,CMND,SoDienThoai,Email,QuocTich,NgheNghiep,NoiLamViec,QuanHeVoiChuHo,HoKhauId,UserId,NgayDangKy,TrangThai")] NhanKhau nhanKhau)
        {
            if (id != nhanKhau.NhanKhauId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhanKhau);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật nhân khẩu thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanKhauExists(nhanKhau.NhanKhauId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), "HoKhau", new { id = nhanKhau.HoKhauId });
            }
            
            var hoKhaus = await _context.HoKhaus.ToListAsync();
            ViewData["HoKhauId"] = new SelectList(hoKhaus, "HoKhauId", "MaHoKhau", nhanKhau.HoKhauId);
            
            var users = await _userManager.Users
                .Where(u => !_context.NhanKhaus.Any(nk => nk.UserId == u.Id && nk.NhanKhauId != id) || u.Id == nhanKhau.UserId)
                .ToListAsync();
            ViewData["UserId"] = new SelectList(users, "Id", "HoTen", nhanKhau.UserId);
            
            return View(nhanKhau);
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
                .Include(n => n.User)
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
            if (nhanKhau != null)
            {
                int hoKhauId = nhanKhau.HoKhauId;
                _context.NhanKhaus.Remove(nhanKhau);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa nhân khẩu thành công!";
                return RedirectToAction(nameof(Details), "HoKhau", new { id = hoKhauId });
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool NhanKhauExists(int id)
        {
            return _context.NhanKhaus.Any(e => e.NhanKhauId == id);
        }
    }
}
