using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhuDanCu.ViewModels
{
    public class CreateHoKhauViewModel
    {
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
    }

    public class EditHoKhauViewModel
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
    }

    public class NhanKhauViewModel
    {
        public int NhanKhauId { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime NgaySinh { get; set; }

        [Required(ErrorMessage = "Giới tính không được để trống")]
        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; } = string.Empty;

        [Required(ErrorMessage = "CMND/CCCD không được để trống")]
        [Display(Name = "CMND/CCCD")]
        public string CMND { get; set; } = string.Empty;

        [Display(Name = "Quốc tịch")]
        public string QuocTich { get; set; } = "Việt Nam";

        [Required(ErrorMessage = "Nghề nghiệp không được để trống")]
        [Display(Name = "Nghề nghiệp")]
        public string NgheNghiep { get; set; } = string.Empty;

        [Display(Name = "Nơi làm việc")]
        public string NoiLamViec { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quan hệ với chủ hộ không được để trống")]
        [Display(Name = "Quan hệ với chủ hộ")]
        public string QuanHeVoiChuHo { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hộ khẩu không được để trống")]
        [Display(Name = "Hộ khẩu")]
        public int HoKhauId { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        [Display(Name = "Liên kết tài khoản")]
        public string? UserId { get; set; }
    }
}
