using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QuanLyKhuDanCu.ViewModels
{
    public class ThongBaoViewModel
    {
        public int ThongBaoId { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày hết hạn")]
        public DateTime? NgayHetHan { get; set; }

        [Required(ErrorMessage = "Đối tượng là bắt buộc")]
        [Display(Name = "Đối tượng")]
        public string DoiTuong { get; set; } = string.Empty; // Ví dụ: "TatCa", "CuDan", "NhanVien"

        [Display(Name = "Trạng thái (Hiển thị)")]
        public bool TrangThai { get; set; } = true;

        [Display(Name = "Tệp đính kèm")]
        public IFormFile? FileDinhKemUpload { get; set; } // Property for the uploaded file

        public string? ExistingFilePath { get; set; } // To show existing file path during edit
    }
}
