using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seftfish.Models;
using Seftfish.Areas.Admin.Models;

namespace Seftfish.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DanhmucController : Controller
    {
        private readonly AppDbContext _context;

        public DanhmucController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.DanhMucs
                .Include(d => d.SanPhams)
                .Include(d => d.IdDanhMucChaNavigation)
                .OrderBy(d => d.ThuTuHienThi)
                .ToListAsync();
            
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.TenDanhMuc))
                {
                    return Json(new { success = false, message = "Tên danh mục là bắt buộc" });
                }

                if (request.TenDanhMuc.Length > 100)
                {
                    return Json(new { success = false, message = "Tên danh mục không được vượt quá 100 ký tự" });
                }

                var category = new DanhMuc
                {
                    TenDanhMuc = request.TenDanhMuc.Trim(),
                    MoTa = string.IsNullOrEmpty(request.MoTa) ? null : request.MoTa.Trim(),
                    ThuTuHienThi = await GetNextDisplayOrder()
                };

                _context.DanhMucs.Add(category);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm danh mục thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateCategoryRequest request)
        {
            try
            {
                var category = await _context.DanhMucs.FindAsync(request.Id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy danh mục" });
                }

                if (string.IsNullOrEmpty(request.TenDanhMuc))
                {
                    return Json(new { success = false, message = "Tên danh mục là bắt buộc" });
                }

                category.TenDanhMuc = request.TenDanhMuc.Trim();
                category.MoTa = string.IsNullOrEmpty(request.MoTa) ? null : request.MoTa.Trim();

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật danh mục thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _context.DanhMucs.Include(d => d.SanPhams).FirstOrDefaultAsync(d => d.IdDanhMuc == id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy danh mục" });
                }

                if (category.SanPhams.Any())
                {
                    return Json(new { success = false, message = "Không thể xóa danh mục có chứa sản phẩm" });
                }

                _context.DanhMucs.Remove(category);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa danh mục thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryProducts(int id)
        {
            var category = await _context.DanhMucs
                .Include(d => d.SanPhams)
                .FirstOrDefaultAsync(d => d.IdDanhMuc == id);

            if (category == null)
            {
                return NotFound();
            }

            var products = category.SanPhams.Select(p => new
            {
                Id = p.IdSanPham,
                Name = p.TenSanPham,
                Price = p.GiaBan?.ToString("N0") + " ₫",
                Status = p.TrangThai == true ? "Hiển thị" : "Ẩn"
            }).ToList();

            return Json(products);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder([FromBody] List<int> categoryIds)
        {
            try
            {
                for (int i = 0; i < categoryIds.Count; i++)
                {
                    var category = await _context.DanhMucs.FindAsync(categoryIds[i]);
                    if (category != null)
                    {
                        category.ThuTuHienThi = i + 1;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật thứ tự thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        private async Task<int> GetNextDisplayOrder()
        {
            var maxOrder = await _context.DanhMucs.MaxAsync(d => (int?)d.ThuTuHienThi) ?? 0;
            return maxOrder + 1;
        }

        public class CreateCategoryRequest
        {
            public string TenDanhMuc { get; set; } = null!;
            public string? MoTa { get; set; }
        }

        public class UpdateCategoryRequest
        {
            public int Id { get; set; }
            public string TenDanhMuc { get; set; } = null!;
            public string? MoTa { get; set; }
        }
    }
}
