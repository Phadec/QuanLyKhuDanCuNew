using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QuanLyKhuDanCu.Models
{
    public class NhanKhau
    {
        [Key]
        public int NhanKhauId { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc.")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime NgaySinh { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc.")]
        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; } = string.Empty;

        [Required(ErrorMessage = "CMND/CCCD là bắt buộc.")]
        [StringLength(20)]
        [Display(Name = "CMND/CCCD")]
        public string CMND { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [StringLength(15)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quốc tịch là bắt buộc.")]
        [StringLength(50)]
        [Display(Name = "Quốc tịch")]
        public string QuocTich { get; set; } = "Việt Nam";

        [Required(ErrorMessage = "Nghề nghiệp là bắt buộc.")]
        [StringLength(100)]
        [Display(Name = "Nghề nghiệp")]
        public string NgheNghiep { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nơi làm việc là bắt buộc.")]
        [StringLength(200)]
        [Display(Name = "Nơi làm việc")]
        public string NoiLamViec { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quan hệ với chủ hộ là bắt buộc.")]
        [StringLength(50)]
        [Display(Name = "Quan hệ với chủ hộ")]
        public string QuanHeVoiChuHo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn hộ khẩu.")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn hộ khẩu hợp lệ.")]
        [Display(Name = "Hộ khẩu")]
        public int HoKhauId { get; set; }

        [ForeignKey("HoKhauId")]
        [ValidateNever]
        public virtual HoKhau? HoKhau { get; set; }

        [Display(Name = "Tài khoản liên kết")]
        public string? UserId { get; set; } // Nullable if a NhanKhau might not have a system user account

        [ForeignKey("UserId")]
        [ValidateNever]
        public virtual ApplicationUser? User { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày đăng ký")]
        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        public virtual ICollection<TamTruTamVang> TamTruTamVangs { get; set; } = new List<TamTruTamVang>();
    }
}
