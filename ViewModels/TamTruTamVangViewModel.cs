using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhuDanCu.ViewModels
{
    // Make sure there's only ONE TamTruTamVangViewModel class in the entire project
    public class TamTruTamVangViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nhân khẩu không được để trống")]
        [Display(Name = "Nhân khẩu")]
        public int NhanKhauId { get; set; }

        [Required(ErrorMessage = "Loại đơn không được để trống")]
        [Display(Name = "Loại đơn")]
        public string LoaiDon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime NgayBatDau { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày kết thúc")]
        public DateTime NgayKetThuc { get; set; } = DateTime.Now.AddMonths(3);

        [Required(ErrorMessage = "Lý do không được để trống")]
        [Display(Name = "Lý do")]
        public string LyDo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ tạm trú/tạm vắng không được để trống")]
        [Display(Name = "Địa chỉ tạm trú/tạm vắng")]
        public string DiaChiTamTruTamVang { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; } = string.Empty;
    }

    // Separate class for processing requests
    public class ProcessTamTruTamVangRequestViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = string.Empty;
        
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; } = string.Empty;
    }
}
