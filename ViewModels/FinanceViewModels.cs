using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhuDanCu.ViewModels
{
    public class HoaDonViewModel
    {
        public int HoaDonId { get; set; }

        [Required(ErrorMessage = "Hộ khẩu là bắt buộc")]
        [Display(Name = "Hộ khẩu")]
        public int HoKhauId { get; set; }

        [Required(ErrorMessage = "Loại phí là bắt buộc")]
        [Display(Name = "Loại phí")]
        public int LoaiPhiId { get; set; }        [Display(Name = "Mã hóa đơn")]
        public string? MaHoaDon { get; set; }

        [Display(Name = "Tổng tiền")]
        [DataType(DataType.Currency)]
        public decimal? TongTien { get; set; }

        [Required(ErrorMessage = "Hạn thanh toán là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Hạn thanh toán")]
        public DateTime HanThanhToan { get; set; }

        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }
    }

    public class BatchInvoiceViewModel
    {
        [Required(ErrorMessage = "Loại phí là bắt buộc")]
        [Display(Name = "Loại phí")]
        public int LoaiPhiId { get; set; }

        [Display(Name = "Số tiền tùy chỉnh")]
        [DataType(DataType.Currency)]
        public decimal? CustomAmount { get; set; }

        [Required(ErrorMessage = "Hạn thanh toán là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Hạn thanh toán")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }
    }

    public class LoaiPhiViewModel
    {
        public int LoaiPhiId { get; set; }

        [Required(ErrorMessage = "Tên loại phí là bắt buộc")]
        [Display(Name = "Tên loại phí")]
        public string TenLoaiPhi { get; set; }

        [Required(ErrorMessage = "Mức phí là bắt buộc")]
        [DataType(DataType.Currency)]
        [Display(Name = "Mức phí")]
        public decimal MucPhi { get; set; }

        [Required(ErrorMessage = "Chu kỳ là bắt buộc")]
        [Display(Name = "Chu kỳ")]
        public string ChuKy { get; set; }

        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Display(Name = "Bắt buộc")]
        public bool BatBuoc { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;
    }
}
