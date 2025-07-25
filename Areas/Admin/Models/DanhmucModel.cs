namespace Seftfish.Areas.Admin.Models
{
    public class CategoryOrderModel
    {
        public int IdDanhMuc { get; set; }
        public int ThuTu { get; set; }
    }

    public class CreateCategoryRequest
    {
        public required string TenDanhMuc { get; set; }
        public string? MoTa { get; set; }
    }

    public class UpdateCategoryRequest
    {
        public int Id { get; set; }
        public required string TenDanhMuc { get; set; }
        public string? MoTa { get; set; }
    }
}
