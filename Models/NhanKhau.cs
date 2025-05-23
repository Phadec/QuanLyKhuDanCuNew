using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhuDanCu.Models
{
    public class NhanKhau
    {
        [Key]
        public int NhanKhauId { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; } = string.Empty;
        public string CMND { get; set; } = string.Empty;
        public string QuocTich { get; set; } = "Việt Nam";
        public string NgheNghiep { get; set; } = string.Empty;
        public string NoiLamViec { get; set; } = string.Empty;
        public string QuanHeVoiChuHo { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int HoKhauId { get; set; }
        public bool TrangThai { get; set; } = true;
        public string? UserId { get; set; }
        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual HoKhau HoKhau { get; set; } = null!;
        public virtual ApplicationUser? User { get; set; }
        
        // Add this collection
        public virtual ICollection<TamTruTamVang> TamTruTamVangs { get; set; } = new List<TamTruTamVang>();
    }
}
