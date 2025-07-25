﻿using System;
using System.Collections.Generic;

namespace Seftfish.Models;

public partial class ChiTietGioHang
{
    public int IdChiTiet { get; set; }

    public int? IdGioHang { get; set; }

    public int? IdBienThe { get; set; }

    public int? SoLuong { get; set; }

    public virtual BienTheSanPham? IdBienTheNavigation { get; set; }

    public virtual GioHang? IdGioHangNavigation { get; set; }
}
