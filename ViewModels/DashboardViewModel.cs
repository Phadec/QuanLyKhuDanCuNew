using System.Collections.Generic;

namespace QuanLyKhuDanCu.ViewModels
{
    public class DashboardViewModel
    {
        public int TongSoHoKhau { get; set; }
        public int TongSoNhanKhau { get; set; }
        public int TongSoDonTamTruTamVang { get; set; }
        public int HoaDonChuaThanhToan { get; set; }
        public int TongSoYeuCauDichVu { get; set; }
        public int TongSoPhanAnh { get; set; }
        
        public Dictionary<string, int> ThongKeGioiTinh { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ThongKeTamTruTamVang { get; set; } = new Dictionary<string, int>();
        
        public decimal TongThuTheoThang { get; set; }
        public decimal TongThuTheoNam { get; set; }
    }

    public class ThongKeDanCuViewModel
    {
        public int TongSoHoKhau { get; set; }
        public int TongSoNhanKhau { get; set; }
        public Dictionary<string, int> ThongKeDoTuoi { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> PhanBoQuyMoHo { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ThongKeQuocTich { get; set; } = new Dictionary<string, int>();
        public List<BiendongDanCuItemViewModel> BiendongDanCuGanDay { get; set; } = new List<BiendongDanCuItemViewModel>();
    }

    public class BiendongDanCuItemViewModel
    {
        public string HoTen { get; set; } = string.Empty;
        public System.DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public System.DateTime NgayDangKy { get; set; }
    }

    public class ThongKeThuPhiViewModel
    {
        public int Year { get; set; }
        public decimal TongThuNam { get; set; }
        public Dictionary<string, decimal> DoanhThuTheoThang { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> DoanhThuTheoLoaiPhi { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, int> ThongKeTrangThaiThanhToan { get; set; } = new Dictionary<string, int>();
        public List<QuanLyKhuDanCu.Models.HoaDon> ThanhToanGanDay { get; set; } = new List<QuanLyKhuDanCu.Models.HoaDon>();
    }

    public class ThongKeYeuCauDichVuViewModel
    {
        public int Year { get; set; }
        public int TongSoYeuCau { get; set; }
        public Dictionary<string, int> YeuCauTheoThang { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> YeuCauTheoTrangThai { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> YeuCauTheoDichVu { get; set; } = new Dictionary<string, int>();
        public double ThoiGianXuLyTrungBinh { get; set; }
        public List<QuanLyKhuDanCu.Models.YeuCauDichVu> YeuCauGanDay { get; set; } = new List<QuanLyKhuDanCu.Models.YeuCauDichVu>();
    }
}
