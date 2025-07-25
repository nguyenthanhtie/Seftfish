using System;
using System.Collections.Generic;

namespace Seftfish.Models;

public partial class SanPham
{
    public int IdSanPham { get; set; }

    public string? TenSanPham { get; set; }

    public string? MoTa { get; set; }

    public decimal? GiaBan { get; set; }

    public bool? TrangThai { get; set; }

    public int? IdDanhMuc { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<AnhSanPham> AnhSanPhams { get; set; } = new List<AnhSanPham>();

    public virtual ICollection<BienTheSanPham> BienTheSanPhams { get; set; } = new List<BienTheSanPham>();

    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    public virtual DanhMuc? IdDanhMucNavigation { get; set; }
}
