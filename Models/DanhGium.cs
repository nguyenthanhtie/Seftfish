using System;
using System.Collections.Generic;

namespace Seftfish.Models;

public partial class DanhGium
{
    public int IdDanhGia { get; set; }

    public int? IdTaiKhoan { get; set; }

    public int? IdSanPham { get; set; }

    public int? SoSao { get; set; }

    public string? BinhLuan { get; set; }

    public string? AnhDanhGia { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public virtual SanPham? IdSanPhamNavigation { get; set; }

    public virtual TaiKhoan? IdTaiKhoanNavigation { get; set; }
}
