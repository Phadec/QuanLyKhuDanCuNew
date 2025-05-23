using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhuDanCu.Models
{
    public class YeuCauDichVu
    {
        [Key]
        public int YeuCauDichVuId { get; set; }
        public int DichVuId { get; set; }
        public string UserId { get; set; }
        public DateTime NgayYeuCau { get; set; }
        public string NoiDung { get; set; }
        public string TrangThai { get; set; } // "ChoXuLy", "DangXuLy", "DaHoanThanh", "TuChoi"
        public string GhiChu { get; set; }
        public string NguoiXuLyId { get; set; }
        public DateTime? NgayXuLy { get; set; }
        public DateTime? NgayHoanThanh { get; set; }

        // Navigation properties
        public virtual DichVu DichVu { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationUser NguoiXuLy { get; set; }
    }
}
