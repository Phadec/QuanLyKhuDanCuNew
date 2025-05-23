using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhuDanCu.Models
{
    public class DichVu
    {
        [Key]
        public int DichVuId { get; set; }
        public string TenDichVu { get; set; }
        public string MoTa { get; set; }
        public decimal PhiDichVu { get; set; }
        public bool TrangThai { get; set; }

        // Navigation properties
        public virtual ICollection<YeuCauDichVu> YeuCauDichVus { get; set; }
    }
}
