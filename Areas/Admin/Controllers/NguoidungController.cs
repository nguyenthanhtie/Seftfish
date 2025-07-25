using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seftfish.Models;
using Seftfish.Areas.Admin.Models;

namespace Seftfish.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NguoidungController : Controller
    {
        private readonly AppDbContext _context;

        public NguoidungController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _context.TaiKhoans
                .Include(t => t.DiaChis)
                .Where(t => t.VaiTro == "khach" || t.VaiTro == null || t.VaiTro == "")
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();
            
            // Debug information
            ViewBag.TotalAccounts = await _context.TaiKhoans.CountAsync();
            ViewBag.AllRoles = await _context.TaiKhoans.Select(t => t.VaiTro).Distinct().ToListAsync();
            
            return View(customers);
        }

        public async Task<IActionResult> Lichsu()
        {
            var ordersHistory = await _context.DonHangs
                .Include(d => d.IdTaiKhoanNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.IdBienTheNavigation!)
                        .ThenInclude(bt => bt.IdSanPhamNavigation!)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();
            
            return View(ordersHistory);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerDetail(int id)
        {
            var customer = await _context.TaiKhoans
                .Include(t => t.DiaChis)
                .Include(t => t.DonHangs)
                .FirstOrDefaultAsync(t => t.IdTaiKhoan == id);

            if (customer == null)
            {
                return NotFound();
            }

            var customerDetail = new
            {
                Id = customer.IdTaiKhoan,
                HoTen = customer.HoTen,
                Email = customer.Email,
                NgayTao = customer.NgayTao?.ToString("dd/MM/yyyy"),
                AnhDaiDien = customer.AnhDaiDien,
                TrangThai = customer.TrangThai == true ? "Hoạt động" : "Tạm khóa",
                DiaChi = customer.DiaChis.FirstOrDefault()?.DiaChiChiTiet ?? "Chưa cập nhật",
                TongDonHang = customer.DonHangs.Count,
                TongChiTieu = customer.DonHangs.Sum(d => d.TongTien)?.ToString("N0") + " ₫"
            };

            return Json(customerDetail);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var customer = await _context.TaiKhoans.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            customer.TrangThai = !customer.TrangThai;
            await _context.SaveChangesAsync();

            return Json(new { success = true, status = customer.TrangThai == true ? "Hoạt động" : "Tạm khóa" });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.MatKhau) || string.IsNullOrEmpty(request.VaiTro))
                {
                    return Json(new { success = false, message = "Email, mật khẩu và vai trò là bắt buộc" });
                }

                // Validate field lengths based on database constraints
                if (request.Email.Length > 100)
                {
                    return Json(new { success = false, message = "Email không được vượt quá 100 ký tự" });
                }

                if (request.MatKhau.Length > 255)
                {
                    return Json(new { success = false, message = "Mật khẩu không được vượt quá 255 ký tự" });
                }

                if (request.VaiTro.Length > 20)
                {
                    return Json(new { success = false, message = "Vai trò không được vượt quá 20 ký tự" });
                }

                // Validate allowed roles
                var allowedRoles = new[] { "khach", "Admin", "Staff" };
                if (!allowedRoles.Contains(request.VaiTro))
                {
                    return Json(new { success = false, message = "Vai trò không hợp lệ" });
                }

                if (!string.IsNullOrEmpty(request.HoTen) && request.HoTen.Length > 100)
                {
                    return Json(new { success = false, message = "Họ tên không được vượt quá 100 ký tự" });
                }

                if (!string.IsNullOrEmpty(request.GioiTinh) && request.GioiTinh.Length > 10)
                {
                    return Json(new { success = false, message = "Giới tính không được vượt quá 10 ký tự" });
                }

                // Check if email already exists
                if (await _context.TaiKhoans.AnyAsync(t => t.Email == request.Email.Trim()))
                {
                    return Json(new { success = false, message = "Email đã tồn tại trong hệ thống" });
                }

                // Parse birth date safely
                DateOnly? ngaySinh = null;
                if (!string.IsNullOrEmpty(request.NgaySinh))
                {
                    if (DateOnly.TryParse(request.NgaySinh, out var parsedDate))
                    {
                        ngaySinh = parsedDate;
                    }
                }

                // Create new customer
                var customer = new TaiKhoan
                {
                    Email = request.Email.Trim(),
                    MatKhau = request.MatKhau, // TODO: Hash password properly
                    HoTen = string.IsNullOrEmpty(request.HoTen) ? null : request.HoTen.Trim(),
                    GioiTinh = string.IsNullOrEmpty(request.GioiTinh) ? null : request.GioiTinh,
                    NgaySinh = ngaySinh,
                    TrangThai = request.TrangThai,
                    VaiTro = request.VaiTro,
                    NgayTao = DateTime.Now
                };

                _context.TaiKhoans.Add(customer);
                await _context.SaveChangesAsync();

                // Add address if provided
                if (!string.IsNullOrEmpty(request.DiaChiChiTiet) || !string.IsNullOrEmpty(request.SoDienThoai))
                {
                    var address = new DiaChi
                    {
                        IdTaiKhoan = customer.IdTaiKhoan,
                        HoTenNguoiNhan = customer.HoTen,
                        SoDienThoai = string.IsNullOrEmpty(request.SoDienThoai) ? null : request.SoDienThoai.Trim(),
                        DiaChiChiTiet = string.IsNullOrEmpty(request.DiaChiChiTiet) ? null : request.DiaChiChiTiet.Trim(),
                        MacDinh = true
                    };

                    _context.DiaChis.Add(address);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Thêm khách hàng thành công" });
            }
            catch (Exception ex)
            {
                // Log the actual error for debugging
                var innerException = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = $"Có lỗi xảy ra: {innerException}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var order = await _context.DonHangs
                .Include(d => d.IdTaiKhoanNavigation)
                .Include(d => d.IdDiaChiNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.IdBienTheNavigation!)
                        .ThenInclude(bt => bt.IdSanPhamNavigation!)
                .FirstOrDefaultAsync(d => d.IdDonHang == id);

            if (order == null)
            {
                return NotFound();
            }

            var orderDetail = new
            {
                Id = order.IdDonHang,
                CustomerName = order.IdTaiKhoanNavigation?.HoTen ?? "Khách vãng lai",
                CustomerEmail = order.IdTaiKhoanNavigation?.Email ?? "N/A",
                Address = order.IdDiaChiNavigation?.DiaChiChiTiet ?? "Chưa cập nhật",
                OrderDate = order.NgayDat?.ToString("dd/MM/yyyy HH:mm") ?? "N/A",
                Status = order.TrangThai ?? "Không xác định",
                PaymentMethod = order.PhuongThucThanhToan ?? "Chưa xác định",
                TotalAmount = order.TongTien?.ToString("N0") + " ₫",
                Items = order.ChiTietDonHangs.Select(ct => new
                {
                    ProductName = ct.IdBienTheNavigation?.IdSanPhamNavigation?.TenSanPham ?? "Sản phẩm không xác định",
                    VariantSku = ct.IdBienTheNavigation?.Sku ?? "N/A",
                    Quantity = ct.SoLuong ?? 0,
                    Price = ct.GiaLucDat?.ToString("N0") + " ₫",
                    SubTotal = ((ct.SoLuong ?? 0) * (ct.GiaLucDat ?? 0)).ToString("N0") + " ₫"
                }).ToList()
            };

            return Json(orderDetail);
        }

        [HttpGet]
        public IActionResult GetFilterData()
        {
            try
            {
                // Get users who have placed orders
                var users = _context.TaiKhoans
                    .Where(u => u.DonHangs.Any())
                    .Select(u => new
                    {
                        u.IdTaiKhoan,
                        u.Email,
                        u.HoTen
                    })
                    .ToList();

                // Get distinct order statuses
                var statuses = _context.DonHangs
                    .Select(o => o.TrangThai)
                    .Where(s => s != null)
                    .Distinct()
                    .ToList();

                return Json(new { 
                    success = true, 
                    users = users,
                    statuses = statuses
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class CreateCustomerRequest
        {
            public string Email { get; set; } = null!;
            public string MatKhau { get; set; } = null!;
            public string VaiTro { get; set; } = null!;
            public string? HoTen { get; set; }
            public string? GioiTinh { get; set; }
            public string? NgaySinh { get; set; }
            public bool TrangThai { get; set; } = true;
            public string? SoDienThoai { get; set; }
            public string? DiaChiChiTiet { get; set; }
        }
    }
}
