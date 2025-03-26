using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.ViewModels
{
    public class ProductVariantViewModel
    {
        public int? VariantId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int SizeId { get; set; }

        [Required]
        public int ColorId { get; set; }

        public int? MaterialId { get; set; }

        [Required]
        public string Sku { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a positive number.")]
        public int Stock { get; set; }

        public string? Image { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be positive.")]
        public decimal? AdditionalPrice { get; set; }
    }
}
