﻿using System;
using System.Collections.Generic;

namespace Seftfish.Models;

public partial class DiaChi
{
    public int IdDiaChi { get; set; }

    public int? IdTaiKhoan { get; set; }

    public string? HoTenNguoiNhan { get; set; }

    public string? SoDienThoai { get; set; }

    public string? DiaChiChiTiet { get; set; }

    public bool? MacDinh { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual TaiKhoan? IdTaiKhoanNavigation { get; set; }
}
