namespace Seftfish.Areas.Admin.Models
{
    public class CreateCustomerRequest
    {
        public required string Email { get; set; }
        public required string MatKhau { get; set; }
        public required string VaiTro { get; set; }
        public string? HoTen { get; set; }
        public string? GioiTinh { get; set; }
        public string? NgaySinh { get; set; }
        public bool TrangThai { get; set; } = true;
        public string? SoDienThoai { get; set; }
        public string? DiaChiChiTiet { get; set; }
    }
}
