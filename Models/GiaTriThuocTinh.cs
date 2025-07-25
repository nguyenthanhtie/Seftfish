﻿using System;
using System.Collections.Generic;

namespace Seftfish.Models;

public partial class GiaTriThuocTinh
{
    public int IdGiaTri { get; set; }

    public int? IdThuocTinh { get; set; }

    public string? GiaTri { get; set; }

    public virtual ThuocTinh? IdThuocTinhNavigation { get; set; }

    public virtual ICollection<BienTheSanPham> IdBienThes { get; set; } = new List<BienTheSanPham>();
}
