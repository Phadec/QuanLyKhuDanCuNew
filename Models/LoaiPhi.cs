using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhuDanCu.Models
{
    public class LoaiPhi
    {
        [Key]
        public int LoaiPhiId { get; set; }
        public string TenLoaiPhi { get; set; }
        public decimal MucPhi { get; set; }
        public string ChuKy { get; set; } // Monthly, Quarterly, Yearly
        public string MoTa { get; set; }
        public bool BatBuoc { get; set; }
        public bool TrangThai { get; set; }

        // Navigation properties
        public virtual ICollection<HoaDon> HoaDons { get; set; }
    }
}
