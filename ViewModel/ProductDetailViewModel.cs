using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.ViewModel
{
    public class ProductDetailViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountedPrice { get; set; }
        public bool Featured { get; set; }

        // Category info
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        // Images
        public string PrimaryImage { get; set; }
        public List<ProductImage> ProductImages { get; set; }

        // Variants
        public List<Color> ColorOptions { get; set; }
        public List<Size> SizeOptions { get; set; }
        public List<Material> MaterialOptions { get; set; }
        public List<ProductVariant> ProductVariants { get; set; }

        public List<ProductCardViewModel> RelatedProducts { get; set; } = new List<ProductCardViewModel>();


        // Reviews
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; }
        public List<Review> Reviews { get; set; }

        // Helper methods for display purposes
        public string GetFormattedPrice(decimal price)
        {
            return string.Format("{0:N0}₫", price);
        }

        public string GetDiscountText()
        {
            if (DiscountPercent <= 0)
                return string.Empty;

            return $"-{DiscountPercent}%";
        }

        public decimal GetSavingsAmount()
        {
            return BasePrice - DiscountedPrice;
        }

        public string GetSavingsText()
        {
            return GetFormattedPrice(GetSavingsAmount());
        }

        public List<ProductVariantDto> ProductVariantDtos { get; set; }

        public int GetRatingPercentage(int rating)
        {
            if (ReviewCount == 0) return 0;

            int ratingCount = RatingDistribution.ContainsKey(rating) ? RatingDistribution[rating] : 0;
            return (int)Math.Round((double)ratingCount / ReviewCount * 100);
        }
    }
}
