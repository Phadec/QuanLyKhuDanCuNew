using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhuDanCu.Models
{
    public class TamTruTamVang
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int NhanKhauId { get; set; }
        
        [Required]
        public string LoaiDon { get; set; } = string.Empty;
        
        [Required]
        public DateTime NgayBatDau { get; set; }
        
        [Required]
        public DateTime NgayKetThuc { get; set; }
        
        [Required]
        public string LyDo { get; set; } = string.Empty;
        
        [Required]
        public string DiaChiTamTruTamVang { get; set; } = string.Empty;
        
        [Required]
        public string TrangThai { get; set; } = string.Empty;
        
        [Required]
        public DateTime NgayTao { get; set; }
        
        public DateTime? NgayDuyet { get; set; }
        
        // Make NguoiDuyetId nullable
        public string? NguoiDuyetId { get; set; }
        
        public string GhiChu { get; set; } = string.Empty;
        
        // Navigation properties
        [ForeignKey("NhanKhauId")]
        public NhanKhau? NhanKhau { get; set; }
        
        [ForeignKey("NguoiDuyetId")]
        public ApplicationUser? NguoiDuyet { get; set; }
    }
}
