using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhuDanCu.ViewModels
{

    public class ProcessFeedbackViewModel
    {
        public int PhanAnhId { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string NguoiGui { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public string TrangThai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn hành động xử lý.")]
        public string Action { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung phản hồi không được để trống.")]
        [Display(Name = "Nội dung phản hồi")]
        public string PhanHoi { get; set; } = string.Empty;
    }
}
