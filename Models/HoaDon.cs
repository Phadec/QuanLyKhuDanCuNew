using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhuDanCu.Models
{
    public class HoaDon
    {
        [Key]
        public int HoaDonId { get; set; }
        public int HoKhauId { get; set; }
        public int LoaiPhiId { get; set; }
        public string MaHoaDon { get; set; }
        public decimal TongTien { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime HanThanhToan { get; set; }
        public DateTime? NgayThanhToan { get; set; }
        public string TrangThai { get; set; } // "ChuaThanhToan", "DaThanhToan", "QuaHan"
        public string GhiChu { get; set; }
        public string NguoiThuId { get; set; }

        // Navigation properties
        public virtual HoKhau HoKhau { get; set; }
        public virtual LoaiPhi LoaiPhi { get; set; }
        public virtual ApplicationUser NguoiThu { get; set; }
    }
}
