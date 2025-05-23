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
    [Authorize(Policy = "RequireStaffRole")]
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
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder ?? ""; // Ensure this is never null
            ViewData["MaSortParm"] = String.IsNullOrEmpty(sortOrder) ? "ma_desc" : "";
            ViewData["ChuHoSortParm"] = sortOrder == "ChuHo" ? "chuho_desc" : "ChuHo";
            ViewData["DiaChiSortParm"] = sortOrder == "DiaChi" ? "diachi_desc" : "DiaChi";
            ViewData["NgayTaoSortParm"] = sortOrder == "NgayTao" ? "ngaytao_desc" : "NgayTao";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString ?? ""; // Ensure this is never null

            var hoKhaus = from h in _context.HoKhaus
                          .Include(h => h.ChuHo)
                          .Include(h => h.NhanKhaus)
                          select h;

            if (!String.IsNullOrEmpty(searchString))
            {
                hoKhaus = hoKhaus.Where(h => h.MaHoKhau.Contains(searchString) ||
                                           h.DiaChi.Contains(searchString) ||
                                           h.ChuHo.HoTen.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "ma_desc":
                    hoKhaus = hoKhaus.OrderByDescending(h => h.MaHoKhau);
                    break;
                case "ChuHo":
                    hoKhaus = hoKhaus.OrderBy(h => h.ChuHo.HoTen);
                    break;
                case "chuho_desc":
                    hoKhaus = hoKhaus.OrderByDescending(h => h.ChuHo.HoTen);
                    break;
                case "DiaChi":
                    hoKhaus = hoKhaus.OrderBy(h => h.DiaChi);
                    break;
                case "diachi_desc":
                    hoKhaus = hoKhaus.OrderByDescending(h => h.DiaChi);
                    break;
                case "NgayTao":
                    hoKhaus = hoKhaus.OrderBy(h => h.NgayTao);
                    break;
                case "ngaytao_desc":
                    hoKhaus = hoKhaus.OrderByDescending(h => h.NgayTao);
                    break;
                default:
                    hoKhaus = hoKhaus.OrderBy(h => h.MaHoKhau);
                    break;
            }

            int pageSize = 10;
            return View(await QuanLyKhuDanCu.Helpers.PaginatedList<HoKhau>.CreateAsync(hoKhaus.AsNoTracking(), pageNumber ?? 1, pageSize));
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
                .FirstOrDefaultAsync(m => m.HoKhauId == id);

            if (hoKhau == null)
            {
                return NotFound();
            }

            return View(hoKhau);
        }

        // GET: HoKhau/Create
        public IActionResult Create()
        {
            var users = _userManager.Users
                .Where(u => u.TrangThai)
                .OrderBy(u => u.HoTen)
                .ToList();

            ViewBag.ChuHoId = new SelectList(users, "Id", "HoTen");
            return View();
        }

        // POST: HoKhau/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HoKhauViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check if MaHoKhau already exists
                if (_context.HoKhaus.Any(h => h.MaHoKhau == viewModel.MaHoKhau))
                {
                    ModelState.AddModelError("MaHoKhau", "Mã hộ khẩu đã tồn tại, vui lòng chọn mã khác.");
                    ViewBag.ChuHoId = new SelectList(_userManager.Users.OrderBy(u => u.HoTen), "Id", "HoTen", viewModel.ChuHoId);
                    return View(viewModel);
                }

                var hoKhau = new HoKhau
                {
                    MaHoKhau = viewModel.MaHoKhau,
                    DiaChi = viewModel.DiaChi,
                    ChuHoId = viewModel.ChuHoId,
                    GhiChu = viewModel.GhiChu,
                    NgayTao = DateTime.Now,
                    TrangThai = true
                };

                _context.Add(hoKhau);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Tạo hộ khẩu mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ChuHoId = new SelectList(_userManager.Users.OrderBy(u => u.HoTen), "Id", "HoTen", viewModel.ChuHoId);
            return View(viewModel);
        }

        // GET: HoKhau/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoKhau = await _context.HoKhaus.FindAsync(id);
            if (hoKhau == null)
            {
                return NotFound();
            }

            var viewModel = new HoKhauViewModel
            {
                HoKhauId = hoKhau.HoKhauId,
                MaHoKhau = hoKhau.MaHoKhau,
                DiaChi = hoKhau.DiaChi,
                ChuHoId = hoKhau.ChuHoId,
                GhiChu = hoKhau.GhiChu,
                TrangThai = hoKhau.TrangThai
            };

            ViewBag.ChuHoId = new SelectList(_userManager.Users.OrderBy(u => u.HoTen), "Id", "HoTen", hoKhau.ChuHoId);
            return View(viewModel);
        }

        // POST: HoKhau/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HoKhauViewModel viewModel)
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

                    // Check if MaHoKhau already exists for a different record
                    if (_context.HoKhaus.Any(h => h.MaHoKhau == viewModel.MaHoKhau && h.HoKhauId != id))
                    {
                        ModelState.AddModelError("MaHoKhau", "Mã hộ khẩu đã tồn tại, vui lòng chọn mã khác.");
                        ViewBag.ChuHoId = new SelectList(_userManager.Users.OrderBy(u => u.HoTen), "Id", "HoTen", viewModel.ChuHoId);
                        return View(viewModel);
                    }

                    hoKhau.MaHoKhau = viewModel.MaHoKhau;
                    hoKhau.DiaChi = viewModel.DiaChi;
                    hoKhau.ChuHoId = viewModel.ChuHoId;
                    hoKhau.GhiChu = viewModel.GhiChu;
                    hoKhau.TrangThai = viewModel.TrangThai;

                    _context.Update(hoKhau);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Cập nhật hộ khẩu thành công!";
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
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ChuHoId = new SelectList(_userManager.Users.OrderBy(u => u.HoTen), "Id", "HoTen", viewModel.ChuHoId);
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
                .Include(h => h.NhanKhaus)
                .FirstOrDefaultAsync(m => m.HoKhauId == id);

            if (hoKhau == null)
            {
                return NotFound();
            }

            if (hoKhau.NhanKhaus.Count > 0)
            {
                TempData["Error"] = "Không thể xóa hộ khẩu đang có nhân khẩu!";
                return RedirectToAction(nameof(Index));
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
                var nhanKhauCount = await _context.NhanKhaus.CountAsync(n => n.HoKhauId == id);
                if (nhanKhauCount > 0)
                {
                    TempData["Error"] = "Không thể xóa hộ khẩu đang có nhân khẩu!";
                    return RedirectToAction(nameof(Index));
                }

                _context.HoKhaus.Remove(hoKhau);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Xóa hộ khẩu thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool HoKhauExists(int id)
        {
            return _context.HoKhaus.Any(e => e.HoKhauId == id);
        }
    }
}
