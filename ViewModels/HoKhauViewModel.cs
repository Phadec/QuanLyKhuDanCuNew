using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhuDanCu.ViewModels
{
    public class HoKhauViewModel
    {
        public int HoKhauId { get; set; }

        [Required(ErrorMessage = "Mã hộ khẩu là bắt buộc")]
        [Display(Name = "Mã hộ khẩu")]
        public string MaHoKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Chủ hộ là bắt buộc")]
        [Display(Name = "Chủ hộ")]
        public string ChuHoId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        public DateTime NgayTao { get; set; }

        // For display purposes
        public string? TenChuHo { get; set; }
        public int SoNhanKhau { get; set; }
    }
}
