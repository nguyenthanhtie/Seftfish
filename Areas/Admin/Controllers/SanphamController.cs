using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seftfish.Models;
using Seftfish.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
using System.Threading.Tasks;

namespace Seftfish.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SanphamController : Controller
    {
        private readonly AppDbContext _context;

        public SanphamController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sanPhams = await _context.SanPhams
                .Include(s => s.IdDanhMucNavigation)
                .Include(s => s.AnhSanPhams)
                .Include(s => s.BienTheSanPhams)
                .OrderByDescending(s => s.NgayTao)
                .ToListAsync();

            var danhMucs = await _context.DanhMucs
                .OrderBy(d => d.ThuTuHienThi)
                .ToListAsync();

            ViewBag.DanhMucs = danhMucs;

            return View(sanPhams);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var sanPham = await _context.SanPhams
                .Include(s => s.IdDanhMucNavigation)
                .Include(s => s.AnhSanPhams)
                .Include(s => s.BienTheSanPhams)
                .FirstOrDefaultAsync(s => s.IdSanPham == id);

            if (sanPham == null)
                return NotFound();

            return PartialView("_ProductDetailPartial", sanPham);
        }

        [HttpGet]
        public async Task<IActionResult> AddOrEdit(int? id)
        {
            SanPham model;
            if (id == null)
            {
                model = new SanPham();
            }
            else
            {
                model = await _context.SanPhams
                    .Include(s => s.AnhSanPhams)
                    .Include(s => s.BienTheSanPhams)
                    .FirstOrDefaultAsync(s => s.IdSanPham == id);
                if (model == null)
                    return NotFound();
            }
            ViewBag.DanhMucs = await _context.DanhMucs.OrderBy(d => d.ThuTuHienThi).ToListAsync();
            return PartialView("_AddEditProductPartial", model);
        }

        // Thêm helper method để lấy các giá trị hợp lệ cho LoaiAnh
        private string[] GetMainImageTypes()
        {
            return new[] { "Chinh", "Primary", "Main", "Chính" };
        }

        private string[] GetOtherImageTypes()
        {
            return new[] { "Phu", "Secondary", "Other", "Phụ" };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int? id, [Bind("IdSanPham,TenSanPham,MoTa,GiaBan,TrangThai,IdDanhMuc")] SanPham sanPham, 
            List<IFormFile> ImageFiles, List<string> ImageUrls, string mainImage, List<string> ExistingImages)
        {
            if (ModelState.IsValid)
            {
                bool isNewProduct = id == null || id == 0;

                if (isNewProduct)
                {
                    sanPham.IdSanPham = 0;
                    sanPham.NgayTao = DateTime.Now;
                    _context.Add(sanPham);
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = $"Không thể tạo sản phẩm mới: {ex.Message}" });
                    }
                }
                else
                {
                    try
                    {
                        var existingProduct = await _context.SanPhams.FindAsync(id);
                        if (existingProduct == null)
                        {
                            return Json(new { success = false, message = "Sản phẩm không tồn tại." });
                        }

                        existingProduct.TenSanPham = sanPham.TenSanPham;
                        existingProduct.MoTa = sanPham.MoTa;
                        existingProduct.GiaBan = sanPham.GiaBan;
                        existingProduct.TrangThai = sanPham.TrangThai;
                        existingProduct.IdDanhMuc = sanPham.IdDanhMuc;

                        // Xóa ảnh cũ nếu không còn trong danh sách ExistingImages
                        var currentImages = await _context.AnhSanPhams
                            .Where(a => a.IdSanPham == id)
                            .ToListAsync();

                        var imagesToDelete = currentImages
                            .Where(img => ExistingImages == null || !ExistingImages.Contains(img.DuongDan))
                            .ToList();

                        if (imagesToDelete.Any())
                        {
                            _context.AnhSanPhams.RemoveRange(imagesToDelete);
                        }

                        await _context.SaveChangesAsync();
                        sanPham.IdSanPham = existingProduct.IdSanPham;
                    }
                    catch (DbUpdateConcurrencyException concurrencyEx)
                    {
                        return Json(new { success = false, message = $"Dữ liệu đã bị thay đổi bởi người khác: {concurrencyEx.Message}" });
                    }
                }

                // Xử lý lưu ảnh mới
                try
                {
                    List<string> newImageUrls = new List<string>();
                    
                    // Xử lý file upload
                    if (ImageFiles != null && ImageFiles.Count > 0)
                    {
                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);
                        
                        foreach (var file in ImageFiles)
                        {
                            if (file != null && file.Length > 0)
                            {
                                // Validate file type
                                var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                                if (!allowedTypes.Contains(fileExtension))
                                {
                                    return Json(new { success = false, message = $"File {file.FileName} không phải là định dạng ảnh hợp lệ." });
                                }

                                // Validate file size (5MB max)
                                if (file.Length > 5 * 1024 * 1024)
                                {
                                    return Json(new { success = false, message = $"File {file.FileName} quá lớn. Kích thước tối đa là 5MB." });
                                }

                                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                                
                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                }
                                
                                newImageUrls.Add($"/images/products/{uniqueFileName}");
                            }
                        }
                    }
                    
                    // Thêm URLs được nhập trực tiếp
                    if (ImageUrls != null && ImageUrls.Count > 0)
                    {
                        var validUrls = ImageUrls
                            .Where(url => !string.IsNullOrEmpty(url) && url.Length <= 255)
                            .Where(url => !newImageUrls.Contains(url)) // Tránh trùng lặp
                            .ToList();
                        newImageUrls.AddRange(validUrls);
                    }
                    
                    // Lưu ảnh mới vào database
                    if (newImageUrls.Any())
                    {
                        var (mainType, otherType) = await GetValidImageTypes();
                        
                        // Xác định ảnh chính
                        string primaryImageUrl = !string.IsNullOrEmpty(mainImage) && newImageUrls.Contains(mainImage)
                            ? mainImage
                            : newImageUrls.First();
                        
                        foreach (var url in newImageUrls)
                        {
                            var anhSanPham = new AnhSanPham
                            {
                                IdSanPham = sanPham.IdSanPham,
                                DuongDan = url,
                                LoaiAnh = url == primaryImageUrl ? mainType : otherType
                            };
                            
                            _context.AnhSanPhams.Add(anhSanPham);
                        }
                        
                        await _context.SaveChangesAsync();
                    }

                    // Cập nhật ảnh chính nếu có thay đổi
                    if (!string.IsNullOrEmpty(mainImage))
                    {
                        var allImages = await _context.AnhSanPhams
                            .Where(a => a.IdSanPham == sanPham.IdSanPham)
                            .ToListAsync();

                        var (mainType, otherType) = await GetValidImageTypes();

                        foreach (var img in allImages)
                        {
                            img.LoaiAnh = img.DuongDan == mainImage ? mainType : otherType;
                        }

                        await _context.SaveChangesAsync();
                    }
                    
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    var message = ex.InnerException?.Message ?? ex.Message;
                    return Json(new { success = false, message = $"Lỗi xử lý ảnh: {message}" });
                }
            }
            
            ViewBag.DanhMucs = await _context.DanhMucs.OrderBy(d => d.ThuTuHienThi).ToListAsync();
            var html = await RenderViewToStringAsync("_AddEditProductPartial", sanPham, true);
            return Json(new { success = false, html });
        }

        [HttpGet]
        public async Task<IActionResult> CheckLoaiAnhConstraint()
        {
            try
            {
                // Lấy danh sách giá trị LoaiAnh đã tồn tại
                var existingTypes = await _context.AnhSanPhams
                    .Where(a => a.LoaiAnh != null)
                    .Select(a => a.LoaiAnh)
                    .Distinct()
                    .ToListAsync();
                
                // Kiểm tra một số giá trị phổ biến
                var testValues = new List<string> { "Chinh", "Phu", "Primary", "Secondary", "Main", "Other" };
                var validValues = new List<string>();
                
                foreach (var value in testValues)
                {
                    try
                    {
                        var testEntity = new AnhSanPham
                        {
                            IdSanPham = 1, // Sử dụng ID sản phẩm đã tồn tại để test
                            DuongDan = "/test-path.jpg",
                            LoaiAnh = value
                        };
                        
                        _context.AnhSanPhams.Add(testEntity);
                        await _context.SaveChangesAsync();
                        
                        // Nếu lưu thành công, thêm vào danh sách giá trị hợp lệ
                        validValues.Add(value);
                        
                        // Xóa dữ liệu test
                        _context.AnhSanPhams.Remove(testEntity);
                        await _context.SaveChangesAsync();
                    }
                    catch
                    {
                        // Giá trị không hợp lệ, bỏ qua
                    }
                }
                
                return Json(new { 
                    success = true, 
                    existingTypes = existingTypes, 
                    validTestValues = validValues 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // Thêm helper method để chuẩn hóa giá trị LoaiAnh
        private async Task<(string MainType, string OtherType)> GetValidImageTypes()
        {
            var defaultPairs = new List<(string Main, string Other)>
            {
                ("Chinh", "Phu"),
                ("Primary", "Secondary"),
                ("Main", "Other")
            };
            
            try
            {
                // Kiểm tra database xem có giá trị nào đã được sử dụng chưa
                var existingTypes = await _context.AnhSanPhams
                    .Where(a => a.LoaiAnh != null)
                    .Select(a => a.LoaiAnh)
                    .Distinct()
                    .ToListAsync();
                
                if (existingTypes.Any())
                {
                    // Tìm cặp giá trị đã tồn tại trong database
                    foreach (var pair in defaultPairs)
                    {
                        if (existingTypes.Contains(pair.Main))
                        {
                            string otherType = existingTypes.Contains(pair.Other) 
                                ? pair.Other 
                                : (existingTypes.Count > 1 ? existingTypes.First(t => t != pair.Main) : pair.Other);
                            
                            return (pair.Main, otherType ?? pair.Other);
                        }
                    }
                    
                    // Sửa lỗi: Xử lý nullability
                    if (existingTypes.Count >= 2)
                    {
                        return (existingTypes[0] ?? defaultPairs[0].Main, existingTypes[1] ?? defaultPairs[0].Other);
                    }
                    
                    if (existingTypes.Count > 0 && existingTypes[0] != null)
                    {
                        return (existingTypes[0], existingTypes[0]);
                    }
                }
            }
            catch
            {
                // Bỏ qua lỗi và sử dụng giá trị mặc định
            }
            
            // Mặc định trả về cặp giá trị đầu tiên
            return defaultPairs[0];
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var sanPham = await _context.SanPhams
                    .Include(s => s.AnhSanPhams)
                    .Include(s => s.BienTheSanPhams)
                    .FirstOrDefaultAsync(s => s.IdSanPham == id);

                if (sanPham == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại." });
                }

                // Kiểm tra xem sản phẩm có đang được sử dụng trong đơn hàng không
                var isUsedInOrders = await _context.ChiTietDonHangs
                    .AnyAsync(ct => _context.BienTheSanPhams
                        .Where(bt => bt.IdSanPham == id)
                        .Select(bt => bt.IdBienThe)
                        .Contains(ct.IdBienThe ?? 0));

                if (isUsedInOrders)
                {
                    return Json(new { success = false, message = "Không thể xóa sản phẩm này vì đã có đơn hàng." });
                }

                // Xóa các ảnh sản phẩm trước
                if (sanPham.AnhSanPhams.Any())
                {
                    // Xóa file ảnh khỏi server (chỉ xóa ảnh upload, không xóa URL)
                    foreach (var anh in sanPham.AnhSanPhams)
                    {
                        if (!string.IsNullOrEmpty(anh.DuongDan) && anh.DuongDan.StartsWith("/images/products/"))
                        {
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", anh.DuongDan.TrimStart('/'));
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }
                    }
                    
                    _context.AnhSanPhams.RemoveRange(sanPham.AnhSanPhams);
                }

                // Xóa các biến thể sản phẩm
                if (sanPham.BienTheSanPhams.Any())
                {
                    _context.BienTheSanPhams.RemoveRange(sanPham.BienTheSanPhams);
                }

                // Xóa sản phẩm
                _context.SanPhams.Remove(sanPham);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi xóa sản phẩm: {ex.Message}" });
            }
        }

        // Thêm phương thức để render view thành string
        private async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model, bool partial = false)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = ControllerContext.ActionDescriptor.ActionName;
            }

            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                var viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                var viewResult = viewEngine!.FindView(ControllerContext, viewName, !partial);

                if (!viewResult.Success)
                {
                    throw new InvalidOperationException($"Không tìm thấy view {viewName}");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }
    }
}