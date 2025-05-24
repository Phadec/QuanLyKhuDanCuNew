using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhuDanCu.Models
{
    public class YeuCauDichVu
    {
        [Key]
        public int YeuCauDichVuId { get; set; }
        
        public int DichVuId { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public DateTime NgayYeuCau { get; set; }
        
        [Required]
        public string NoiDung { get; set; } = string.Empty;
        
        [Required]
        public string TrangThai { get; set; } = string.Empty;
        
        [Required]
        public string GhiChu { get; set; } = string.Empty;
        
        // Make NguoiXuLyId nullable since it won't be set initially
        public string? NguoiXuLyId { get; set; }
        
        public DateTime? NgayXuLy { get; set; }
        
        public DateTime? NgayHoanThanh { get; set; }
        
        // Navigation properties
        [ForeignKey("DichVuId")]
        public DichVu? DichVu { get; set; }
        
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
        
        [ForeignKey("NguoiXuLyId")]
        public ApplicationUser? NguoiXuLy { get; set; }
    }
}
