using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhuDanCu.Models
{
    public class ThongBao
    {
        [Key]
        public int ThongBaoId { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgayHetHan { get; set; }
        public string DoiTuong { get; set; } // "TatCa", "QuanTri", "DanCu"
        public bool TrangThai { get; set; }
        public string NguoiTaoId { get; set; }
        public string FileDinhKem { get; set; }

        // Navigation properties
        public virtual ApplicationUser NguoiTao { get; set; }
    }
}
