using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace QuanLyKhuDanCu.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string HoTen { get; set; } = string.Empty;
        public DateTime NgaySinh { get; set; }
        public string DiaChi { get; set; } = string.Empty;
        public string CMND { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public bool TrangThai { get; set; } = true;

        // Navigation properties
        public ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();
        public ICollection<YeuCauDichVu> YeuCauDichVus { get; set; } = new List<YeuCauDichVu>();
        public ICollection<PhanAnh> PhanAnhs { get; set; } = new List<PhanAnh>();
    }
}
