using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhuDanCu.Data;
using QuanLyKhuDanCu.Models;
using QuanLyKhuDanCu.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Controllers
{
    [Authorize(Roles = "Admin,Manager,Staff")]
    public class HoKhauController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HoKhauController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: HoKhau
        public async Task<IActionResult> Index()
        {
            var hoKhaus = await _context.HoKhaus
                .Include(h => h.ChuHo)
                .Include(h => h.NhanKhaus)
                .ToListAsync();
            return View(hoKhaus);
        }

        // GET: HoKhau/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoKhau = await _context.HoKhaus
                .Include(h => h.ChuHo)
                .Include(h => h.NhanKhaus)
                    .ThenInclude(n => n.User)
                .FirstOrDefaultAsync(m => m.HoKhauId == id);

            if (hoKhau == null)
            {
                return NotFound();
            }

            return View(hoKhau);
        }

        // GET: HoKhau/Create
        public async Task<IActionResult> Create()
        {
            var users = await _userManager.Users.ToListAsync();
            ViewData["ChuHoId"] = new SelectList(users, "Id", "HoTen");
            return View(new CreateHoKhauViewModel());
        }

        // POST: HoKhau/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateHoKhauViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var hoKhau = new HoKhau
                {
                    MaHoKhau = viewModel.MaHoKhau,
                    ChuHoId = viewModel.ChuHoId,
                    DiaChi = viewModel.DiaChi,
                    GhiChu = viewModel.GhiChu,
                    TrangThai = viewModel.TrangThai,
                    NgayTao = DateTime.Now
                };

                _context.Add(hoKhau);
                await _context.SaveChangesAsync(); // Save HoKhau to get its ID

                // Automatically create NhanKhau for the ChuHo
                var chuHoUser = await _userManager.FindByIdAsync(hoKhau.ChuHoId);
                if (chuHoUser != null)
                {
                    var chuHoNhanKhau = new NhanKhau
                    {
                        HoTen = chuHoUser.HoTen,
                        NgaySinh = chuHoUser.NgaySinh,
                        // GioiTinh might not be available in ApplicationUser, set a default or handle appropriately
                        GioiTinh = "Chưa rõ", // Or make NhanKhau.GioiTinh nullable
                        CMND = chuHoUser.CMND,
                        SoDienThoai = chuHoUser.SoDienThoai,
                        Email = chuHoUser.Email ?? string.Empty, // Ensure Email is not null
                        QuocTich = "Việt Nam", // Default
                        // NgheNghiep and NoiLamViec might not be available, set defaults
                        NgheNghiep = "Chưa rõ", // Or make NhanKhau.NgheNghiep nullable
                        NoiLamViec = "Chưa rõ", // Or make NhanKhau.NoiLamViec nullable
                        QuanHeVoiChuHo = "Chủ hộ",
                        HoKhauId = hoKhau.HoKhauId, // Link to the newly created HoKhau
                        UserId = chuHoUser.Id,      // Link to the ApplicationUser
                        NgayDangKy = DateTime.Now,
                        TrangThai = true
                    };
                    _context.NhanKhaus.Add(chuHoNhanKhau);
                    await _context.SaveChangesAsync(); // Save the new NhanKhau
                }

                TempData["SuccessMessage"] = "Tạo hộ khẩu và nhân khẩu chủ hộ thành công!";
                return RedirectToAction(nameof(Index));
            }
            
            var users = await _userManager.Users.ToListAsync();
            ViewData["ChuHoId"] = new SelectList(users, "Id", "HoTen", viewModel.ChuHoId);
            return View(viewModel);
        }

        // GET: HoKhau/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoKhau = await _context.HoKhaus.Include(h => h.ChuHo).FirstOrDefaultAsync(h => h.HoKhauId == id);
            if (hoKhau == null)
            {
                return NotFound();
            }

            var viewModel = new EditHoKhauViewModel
            {
                HoKhauId = hoKhau.HoKhauId,
                MaHoKhau = hoKhau.MaHoKhau,
                ChuHoId = hoKhau.ChuHoId,
                DiaChi = hoKhau.DiaChi,
                GhiChu = hoKhau.GhiChu,
                TrangThai = hoKhau.TrangThai,
                NgayTao = hoKhau.NgayTao,
                TenChuHo = hoKhau.ChuHo?.HoTen
            };
            
            var users = await _userManager.Users.ToListAsync();
            ViewData["ChuHoId"] = new SelectList(users, "Id", "HoTen", hoKhau.ChuHoId);
            return View(viewModel);
        }

        // POST: HoKhau/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditHoKhauViewModel viewModel)
        {
            if (id != viewModel.HoKhauId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var hoKhau = await _context.HoKhaus.FindAsync(id);
                    if (hoKhau == null)
                    {
                        return NotFound();
                    }

                    hoKhau.MaHoKhau = viewModel.MaHoKhau;
                    hoKhau.ChuHoId = viewModel.ChuHoId;
                    hoKhau.DiaChi = viewModel.DiaChi;
                    hoKhau.GhiChu = viewModel.GhiChu;
                    hoKhau.TrangThai = viewModel.TrangThai;

                    _context.Update(hoKhau);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật hộ khẩu thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HoKhauExists(viewModel.HoKhauId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = viewModel.HoKhauId });
            }
            
            var users = await _userManager.Users.ToListAsync();
            ViewData["ChuHoId"] = new SelectList(users, "Id", "HoTen", viewModel.ChuHoId);
            return View(viewModel);
        }

        // GET: HoKhau/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoKhau = await _context.HoKhaus
                .Include(h => h.ChuHo)
                .FirstOrDefaultAsync(m => m.HoKhauId == id);
            if (hoKhau == null)
            {
                return NotFound();
            }

            return View(hoKhau);
        }

        // POST: HoKhau/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hoKhau = await _context.HoKhaus.FindAsync(id);
            if (hoKhau != null)
            {
                _context.HoKhaus.Remove(hoKhau);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HoKhauExists(int id)
        {
            return _context.HoKhaus.Any(e => e.HoKhauId == id);
        }
    }
}
