namespace Seftfish.Areas.Admin.Models
{
    public class ThongKeViewModel
    {
        public int TongDonHang { get; set; }
        public int TongKhachHang { get; set; }
        public int TongSanPham { get; set; }
        public decimal TongDoanhThu { get; set; }
        public decimal[] DoanhThuTheoThang { get; set; } = new decimal[12];
        public List<SanPhamBanChayViewModel> SanPhamBanChay { get; set; } = new();
        public List<DonHangTheoTrangThaiViewModel> DonHangTheoTrangThai { get; set; } = new();
    }

    public class SanPhamBanChayViewModel
    {
        public string TenSanPham { get; set; } = null!;
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class DonHangTheoTrangThaiViewModel
    {
        public string TrangThai { get; set; } = null!;
        public int SoLuong { get; set; }
    }
}
