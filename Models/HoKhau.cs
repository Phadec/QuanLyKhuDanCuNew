using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhuDanCu.Models
{
    public class HoKhau
    {
        [Key]
        public int HoKhauId { get; set; }
        public string MaHoKhau { get; set; }
        public string DiaChi { get; set; }
        public DateTime NgayTao { get; set; }
        public string ChuHoId { get; set; }
        public string GhiChu { get; set; }
        public bool TrangThai { get; set; }

        // Navigation properties
        public virtual ApplicationUser ChuHo { get; set; }
        public virtual ICollection<NhanKhau> NhanKhaus { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; }
    }
}
