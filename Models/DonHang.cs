using System;
using System.Collections.Generic;

namespace Seftfish.Models;

public partial class DonHang
{
    public int IdDonHang { get; set; }

    public int? IdTaiKhoan { get; set; }

    public decimal? TongTien { get; set; }

    public DateTime? NgayDat { get; set; }

    public string? TrangThai { get; set; }

    public int? IdDiaChi { get; set; }

    public string? PhuongThucThanhToan { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual DiaChi? IdDiaChiNavigation { get; set; }

    public virtual TaiKhoan? IdTaiKhoanNavigation { get; set; }
}
