using System;
using System.Collections.Generic;

namespace Seftfish.Models;

public partial class ThuocTinh
{
    public int IdThuocTinh { get; set; }

    public string? TenThuocTinh { get; set; }

    public virtual ICollection<GiaTriThuocTinh> GiaTriThuocTinhs { get; set; } = new List<GiaTriThuocTinh>();
}
