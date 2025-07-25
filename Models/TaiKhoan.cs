using System;
using System.Collections.Generic;

namespace Seftfish.Models;

public partial class TaiKhoan
{
    public int IdTaiKhoan { get; set; }

    public string Email { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? VaiTro { get; set; }

    public string? HoTen { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? GioiTinh { get; set; }

    public string? AnhDaiDien { get; set; }

    public bool? TrangThai { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    public virtual ICollection<DiaChi> DiaChis { get; set; } = new List<DiaChi>();

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();
}
