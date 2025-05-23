using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QuanLyKhuDanCu.ViewModels
{
    // YeuCauDichVuViewModel for create/edit operations
    public class YeuCauDichVuViewModel
    {
        public int YeuCauDichVuId { get; set; }

        [Required(ErrorMessage = "Dịch vụ không được để trống")]
        [Display(Name = "Dịch vụ")]
        public int DichVuId { get; set; }

        // UserId will be null for residents creating their own requests
        // and will be set by staff when creating a request on behalf of a resident
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        [Display(Name = "Nội dung yêu cầu")]
        public string NoiDung { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; } = string.Empty;
    }

    // DichVuViewModel for the DichVu entity
    public class DichVuViewModel
    {
        public int DichVuId { get; set; }

        [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
        [Display(Name = "Tên dịch vụ")]
        public string TenDichVu { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string MoTa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phí dịch vụ không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Phí dịch vụ phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Phí dịch vụ")]
        public decimal PhiDichVu { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;
    }

    // ProcessRequestViewModel for handling requests
    public class ProcessRequestViewModel
    {
        public int YeuCauDichVuId { get; set; }

        [Display(Name = "Tên dịch vụ")]
        public string TenDichVu { get; set; } = string.Empty;

        [Display(Name = "Người yêu cầu")]
        public string NguoiYeuCau { get; set; } = string.Empty;

        [Display(Name = "Ngày yêu cầu")]
        public DateTime NgayYeuCau { get; set; }

        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hành động xử lý không được để trống")]
        [Display(Name = "Hành động")]
        public string Action { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; } = string.Empty;
    }

    // PhanAnhViewModel for creating feedback
    public class PhanAnhViewModel
    {
        public int PhanAnhId { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung không được để trống")]
        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; } = string.Empty;

        [Display(Name = "File đính kèm")]
        public IFormFile? FileDinhKem { get; set; }
    }

    // ProcessFeedbackViewModel for handling feedback
    public class ProcessFeedbackViewModel
    {
        public int PhanAnhId { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; } = string.Empty;

        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; } = string.Empty;

        [Display(Name = "Người gửi")]
        public string NguoiGui { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = string.Empty;
        
        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; }

        [Required(ErrorMessage = "Hành động xử lý không được để trống")]
        [Display(Name = "Hành động")]
        public string Action { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phản hồi không được để trống")]
        [Display(Name = "Phản hồi")]
        public string PhanHoi { get; set; } = string.Empty;
    }
}
