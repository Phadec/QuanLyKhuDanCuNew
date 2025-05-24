using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhuDanCu.Models
{
    public class ThongBao
    {
        [Key]
        public int ThongBaoId { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống.")]
        public string TieuDe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung không được để trống.")]
        public string NoiDung { get; set; } = string.Empty;

        public DateTime NgayTao { get; set; }
        public DateTime? NgayHetHan { get; set; }

        [Required(ErrorMessage = "Đối tượng không được để trống.")]
        public string DoiTuong { get; set; } = string.Empty; // Ví dụ: "TatCa", "CuDan", "NhanVien"
        public bool TrangThai { get; set; } // True: Hiển thị, False: Ẩn

        [Required]
        public string NguoiTaoId { get; set; } = string.Empty;

        public string? FileDinhKem { get; set; } // Make this nullable

        // Navigation properties
        [ForeignKey("NguoiTaoId")]
        public virtual ApplicationUser? NguoiTao { get; set; }
    }
}
