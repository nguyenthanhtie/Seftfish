using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seftfish.Models;
using Seftfish.Areas.Admin.Models;

namespace Seftfish.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThongkeController : Controller
    {
        private readonly AppDbContext _context;

        public ThongkeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new ThongKeViewModel();

            // Thống kê tổng quan
            model.TongDonHang = await _context.DonHangs.CountAsync();
            model.TongKhachHang = await _context.TaiKhoans.Where(t => t.VaiTro == "khach").CountAsync();
            model.TongSanPham = await _context.SanPhams.CountAsync();
            model.TongDoanhThu = await _context.DonHangs
                .Where(d => d.TrangThai == "Hoàn thành")
                .SumAsync(d => d.TongTien ?? 0);

            // Doanh thu theo tháng (12 tháng gần nhất)
            var doanhThuTheoThang = new decimal[12];
            var ngayHienTai = DateTime.Now;
            
            for (int i = 0; i < 12; i++)
            {
                var thang = ngayHienTai.AddMonths(-i);
                var doanhThu = await _context.DonHangs
                    .Where(d => d.NgayDat.HasValue && 
                               d.NgayDat.Value.Month == thang.Month && 
                               d.NgayDat.Value.Year == thang.Year &&
                               d.TrangThai == "Hoàn thành")
                    .SumAsync(d => d.TongTien ?? 0);
                doanhThuTheoThang[11 - i] = doanhThu;
            }
            model.DoanhThuTheoThang = doanhThuTheoThang;

            // Sản phẩm bán chạy
            model.SanPhamBanChay = await _context.ChiTietDonHangs
                .Include(ct => ct.IdBienTheNavigation!)
                    .ThenInclude(bt => bt.IdSanPhamNavigation!)
                .Where(ct => ct.IdDonHangNavigation!.TrangThai == "Hoàn thành")
                .GroupBy(ct => new { 
                    ct.IdBienTheNavigation!.IdSanPhamNavigation!.IdSanPham,
                    ct.IdBienTheNavigation.IdSanPhamNavigation.TenSanPham 
                })
                .Select(g => new SanPhamBanChayViewModel
                {
                    TenSanPham = g.Key.TenSanPham!,
                    SoLuongBan = g.Sum(ct => ct.SoLuong ?? 0),
                    DoanhThu = g.Sum(ct => (ct.SoLuong ?? 0) * (ct.GiaLucDat ?? 0))
                })
                .OrderByDescending(sp => sp.SoLuongBan)
                .Take(10)
                .ToListAsync();

            // Đơn hàng theo trạng thái
            model.DonHangTheoTrangThai = await _context.DonHangs
                .GroupBy(d => d.TrangThai ?? "Không xác định")
                .Select(g => new DonHangTheoTrangThaiViewModel
                {
                    TrangThai = g.Key,
                    SoLuong = g.Count()
                })
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetDoanhThuTheoNgay(DateTime? tuNgay, DateTime? denNgay)
        {
            tuNgay ??= DateTime.Now.AddDays(-30);
            denNgay ??= DateTime.Now;

            var doanhThu = await _context.DonHangs
                .Where(d => d.NgayDat.HasValue && 
                           d.NgayDat.Value.Date >= tuNgay.Value.Date && 
                           d.NgayDat.Value.Date <= denNgay.Value.Date &&
                           d.TrangThai == "Hoàn thành")
                .GroupBy(d => d.NgayDat.Value.Date)
                .Select(g => new
                {
                    Ngay = g.Key.ToString("dd/MM/yyyy"),
                    DoanhThu = g.Sum(d => d.TongTien ?? 0),
                    SoDonHang = g.Count()
                })
                .OrderBy(x => x.Ngay)
                .ToListAsync();

            return Json(doanhThu);
        }

        [HttpGet]
        public async Task<IActionResult> GetBaoCaoHieuSuat()
        {
            var ngayHienTai = DateTime.Now;
            var thangHienTai = new DateTime(ngayHienTai.Year, ngayHienTai.Month, 1);
            var thangTruoc = thangHienTai.AddMonths(-1);

            // Doanh thu tháng này
            var doanhThuThangNay = await _context.DonHangs
                .Where(d => d.NgayDat.HasValue && 
                           d.NgayDat.Value >= thangHienTai &&
                           d.TrangThai == "Hoàn thành")
                .SumAsync(d => d.TongTien ?? 0);

            // Doanh thu tháng trước
            var doanhThuThangTruoc = await _context.DonHangs
                .Where(d => d.NgayDat.HasValue && 
                           d.NgayDat.Value >= thangTruoc && 
                           d.NgayDat.Value < thangHienTai &&
                           d.TrangThai == "Hoàn thành")
                .SumAsync(d => d.TongTien ?? 0);

            // Số đơn hàng tháng này
            var donHangThangNay = await _context.DonHangs
                .Where(d => d.NgayDat.HasValue && d.NgayDat.Value >= thangHienTai)
                .CountAsync();

            // Số khách hàng mới tháng này
            var khachHangMoiThangNay = await _context.TaiKhoans
                .Where(t => t.NgayTao.HasValue && 
                           t.NgayTao.Value >= thangHienTai && 
                           t.VaiTro == "khach")
                .CountAsync();

            var phanTramThayDoi = doanhThuThangTruoc > 0 
                ? ((doanhThuThangNay - doanhThuThangTruoc) / doanhThuThangTruoc) * 100 
                : 0;

            return Json(new
            {
                doanhThuThangNay,
                doanhThuThangTruoc,
                donHangThangNay,
                khachHangMoiThangNay,
                phanTramThayDoi = Math.Round(phanTramThayDoi, 2)
            });
        }
    }
}
