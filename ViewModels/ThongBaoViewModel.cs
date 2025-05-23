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
        public string TieuDe { get; set; }

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày hết hạn")]
        public DateTime? NgayHetHan { get; set; }

        [Required(ErrorMessage = "Đối tượng là bắt buộc")]
        [Display(Name = "Đối tượng")]
        public string DoiTuong { get; set; } = "TatCa";

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        [Display(Name = "File đính kèm")]
        public IFormFile FileDinhKem { get; set; }

        [Display(Name = "File hiện tại")]
        public string ExistingFilePath { get; set; }
    }
}
