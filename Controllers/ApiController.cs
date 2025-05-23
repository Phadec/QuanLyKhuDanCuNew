using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhuDanCu.Data;
using QuanLyKhuDanCu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKhuDanCu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Stats
        [HttpGet("Stats")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<object>> GetStats()
        {
            // Dashboard statistics
            var stats = new
            {
                TongSoHoKhau = await _context.HoKhaus.CountAsync(),
                TongSoNhanKhau = await _context.NhanKhaus.CountAsync(),
                TongSoDonTamTruTamVang = await _context.TamTruTamVangs.CountAsync(),
                HoaDonChuaThanhToan = await _context.HoaDons.CountAsync(h => h.TrangThai == "ChuaThanhToan" || h.TrangThai == "QuaHan"),
                ThongKeGioiTinh = new
                {
                    Nam = await _context.NhanKhaus.CountAsync(n => n.GioiTinh == "Nam"),
                    Nu = await _context.NhanKhaus.CountAsync(n => n.GioiTinh == "Nữ")
                },
                ThongKeTamTruTamVang = new
                {
                    TamTru = await _context.TamTruTamVangs.CountAsync(t => t.LoaiDon == "TamTru" && t.TrangThai == "DaDuyet" && t.NgayKetThuc > DateTime.Now),
                    TamVang = await _context.TamTruTamVangs.CountAsync(t => t.LoaiDon == "TamVang" && t.TrangThai == "DaDuyet" && t.NgayKetThuc > DateTime.Now)
                }
            };

            return stats;
        }

        // GET: api/HoKhau
        [HttpGet("HoKhau")]
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<ActionResult<IEnumerable<object>>> GetHoKhau()
        {
            var hoKhaus = await _context.HoKhaus
                .Include(h => h.ChuHo)
                .Select(h => new
                {
                    h.HoKhauId,
                    h.MaHoKhau,
                    h.DiaChi,
                    ChuHo = h.ChuHo.HoTen,
                    SoThanhVien = h.NhanKhaus.Count,
                    h.NgayTao,
                    h.TrangThai
                })
                .ToListAsync();

            return hoKhaus;
        }

        // GET: api/HoKhau/5
        [HttpGet("HoKhau/{id}")]
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<ActionResult<object>> GetHoKhau(int id)
        {
            var hoKhau = await _context.HoKhaus
                .Include(h => h.ChuHo)
                .Include(h => h.NhanKhaus)
                .FirstOrDefaultAsync(h => h.HoKhauId == id);

            if (hoKhau == null)
            {
                return NotFound();
            }

            var result = new
            {
                hoKhau.HoKhauId,
                hoKhau.MaHoKhau,
                hoKhau.DiaChi,
                ChuHo = hoKhau.ChuHo.HoTen,
                ThanhVien = hoKhau.NhanKhaus.Select(n => new
                {
                    n.NhanKhauId,
                    n.HoTen,
                    n.NgaySinh,
                    n.GioiTinh,
                    n.QuanHeVoiChuHo
                }),
                hoKhau.NgayTao,
                hoKhau.TrangThai
            };

            return result;
        }

        // GET: api/NhanKhau
        [HttpGet("NhanKhau")]
        [Authorize(Policy = "RequireStaffRole")]
        public async Task<ActionResult<IEnumerable<object>>> GetNhanKhau()
        {
            var nhanKhaus = await _context.NhanKhaus
                .Include(n => n.HoKhau)
                .Select(n => new
                {
                    n.NhanKhauId,
                    n.HoTen,
                    n.NgaySinh,
                    n.GioiTinh,
                    n.CMND,
                    HoKhau = n.HoKhau.MaHoKhau,
                    n.QuanHeVoiChuHo,
                    n.TrangThai
                })
                .ToListAsync();

            return nhanKhaus;
        }

        // GET: api/MyInfo
        [HttpGet("MyInfo")]
        [Authorize(Roles = "Resident")]
        public async Task<ActionResult<object>> GetMyInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            var nhanKhau = await _context.NhanKhaus
                .Include(n => n.HoKhau)
                .FirstOrDefaultAsync(n => n.UserId == user.Id);

            if (nhanKhau == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin dân cư" });
            }

            var result = new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber,
                user.HoTen,
                NhanKhau = new
                {
                    nhanKhau.NhanKhauId,
                    nhanKhau.HoTen,
                    nhanKhau.NgaySinh,
                    nhanKhau.GioiTinh,
                    nhanKhau.CMND,
                    nhanKhau.QuocTich,
                    nhanKhau.NgheNghiep,
                    nhanKhau.NoiLamViec,
                    nhanKhau.QuanHeVoiChuHo,
                    nhanKhau.SoDienThoai,
                    nhanKhau.Email,
                    HoKhau = new
                    {
                        nhanKhau.HoKhau.HoKhauId,
                        nhanKhau.HoKhau.MaHoKhau,
                        nhanKhau.HoKhau.DiaChi
                    }
                }
            };

            return result;
        }

        // GET: api/MyInvoices
        [HttpGet("MyInvoices")]
        [Authorize(Roles = "Resident")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyInvoices()
        {
            var user = await _userManager.GetUserAsync(User);
            var nhanKhau = await _context.NhanKhaus.FirstOrDefaultAsync(n => n.UserId == user.Id);
            
            if (nhanKhau == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin dân cư" });
            }
            
            var hoaDons = await _context.HoaDons
                .Include(h => h.LoaiPhi)
                .Where(h => h.HoKhau.HoKhauId == nhanKhau.HoKhauId)
                .Select(h => new
                {
                    h.HoaDonId,
                    h.MaHoaDon,
                    LoaiPhi = h.LoaiPhi.TenLoaiPhi,
                    h.TongTien,
                    h.NgayTao,
                    h.HanThanhToan,
                    h.NgayThanhToan,
                    h.TrangThai
                })
                .OrderByDescending(h => h.NgayTao)
                .ToListAsync();
                
            return hoaDons;
        }

        // GET: api/MyRequests
        [HttpGet("MyRequests")]
        [Authorize(Roles = "Resident")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var requests = await _context.YeuCauDichVus
                .Include(y => y.DichVu)
                .Where(y => y.UserId == user.Id)
                .Select(y => new
                {
                    y.YeuCauDichVuId,
                    DichVu = y.DichVu.TenDichVu,
                    y.NgayYeuCau,
                    y.NoiDung,
                    y.TrangThai,
                    y.NgayXuLy,
                    y.NgayHoanThanh
                })
                .OrderByDescending(y => y.NgayYeuCau)
                .ToListAsync();
                
            return requests;
        }

        // GET: api/MyFeedback
        [HttpGet("MyFeedback")]
        [Authorize(Roles = "Resident")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyFeedback()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var feedback = await _context.PhanAnhs
                .Where(p => p.UserId == user.Id)
                .Select(p => new
                {
                    p.PhanAnhId,
                    p.TieuDe,
                    p.NoiDung,
                    p.NgayTao,
                    p.TrangThai,
                    p.NgayXuLy,
                    p.PhanHoi
                })
                .OrderByDescending(p => p.NgayTao)
                .ToListAsync();
                
            return feedback;
        }

        // GET: api/Announcements
        [HttpGet("Announcements")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetAnnouncements()
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

            // Hide expired announcements for regular users
            if (!roles.Contains("Admin") && !roles.Contains("Manager"))
            {
                query = query.Where(t => 
                    t.TrangThai && 
                    (t.NgayHetHan == null || t.NgayHetHan > DateTime.Now));
            }

            var announcements = await query
                .Include(t => t.NguoiTao)
                .Select(t => new
                {
                    t.ThongBaoId,
                    t.TieuDe,
                    t.NoiDung,
                    t.NgayTao,
                    t.NgayHetHan,
                    t.DoiTuong,
                    t.TrangThai,
                    NguoiTao = t.NguoiTao.HoTen,
                    HasAttachment = !string.IsNullOrEmpty(t.FileDinhKem)
                })
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();

            return announcements;
        }
    }
}
