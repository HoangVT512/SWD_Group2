using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.ViewModel
{
    public class ProductVariantDto
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public int? ColorId { get; set; }
        public string ColorName { get; set; }
        public string ColorCode { get; set; }
        public int? SizeId { get; set; }
        public string SizeName { get; set; }
        public int? MaterialId { get; set; }
        public string MaterialName { get; set; }
        public decimal? AdditionalPrice { get; set; }
        public int? Stock { get; set; }  // Change to nullable int
        public string Sku { get; set; }
    }
}
