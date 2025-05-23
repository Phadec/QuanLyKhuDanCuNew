using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhuDanCu.Models
{
    public class PhanAnh
    {
        [Key]
        public int PhanAnhId { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string TieuDe { get; set; } = string.Empty;
        
        [Required]
        public string NoiDung { get; set; } = string.Empty;
        
        public DateTime NgayTao { get; set; }
        
        [Required]
        public string TrangThai { get; set; } = string.Empty;
        
        // Make PhanHoi nullable
        public string? PhanHoi { get; set; }
        
        public string? NguoiXuLyId { get; set; }
        
        public DateTime? NgayXuLy { get; set; }
        
        public string FileDinhKem { get; set; } = string.Empty;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
        
        [ForeignKey("NguoiXuLyId")]
        public ApplicationUser? NguoiXuLy { get; set; }
    }
}
