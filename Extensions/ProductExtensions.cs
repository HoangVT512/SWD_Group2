using System;
using System.Linq;
using WebApplication1.Models;

namespace WebApplication1.Extensions
{
    public static class ProductExtensions
    {
        public static string GetPrimaryImageUrl(this Product product, string defaultImage = "/images/no-image.jpg")
        {
            if (product.ProductImages == null || !product.ProductImages.Any())
                return defaultImage;

            var primaryImage = product.ProductImages.FirstOrDefault(i => i.IsPrimary == true);
            return primaryImage != null ? primaryImage.ImageUrl : product.ProductImages.First().ImageUrl;
        }

        public static string GetDiscountText(this Product product)
        {
            if (product.DiscountPercent == null || product.DiscountPercent <= 0)
                return string.Empty;

            return $"-{product.DiscountPercent}%";
        }

        public static decimal GetDiscountedPrice(this Product product)
        {
            if (product.DiscountPercent == null || product.DiscountPercent <= 0)
                return product.BasePrice;

            return product.BasePrice - (product.BasePrice * product.DiscountPercent.Value / 100);
        }

        public static string GetFormattedPrice(this decimal price)
        {
            return string.Format("{0:N0}đ", price);
        }

        public static string GetBadgeText(this Product product)
        {
            if (product.DiscountPercent != null && product.DiscountPercent >= 30)
                return $"-{product.DiscountPercent}%";

            if (product.CreatedAt != null && product.CreatedAt > DateTime.Now.AddDays(-14))
                return "Mới";

            if (product.Featured == true)
                return "Hot";

            return string.Empty;
        }

        public static string GetBadgeClass(this Product product)
        {
            if (product.DiscountPercent != null && product.DiscountPercent >= 30)
                return "flash-badge";

            if (product.CreatedAt != null && product.CreatedAt > DateTime.Now.AddDays(-14))
                return "new";

            if (product.Featured == true)
                return "hot";

            return string.Empty;
        }

        public static int GetTotalStock(this Product product)
        {
            if (product.ProductVariants == null || !product.ProductVariants.Any())
                return 0;

            return product.ProductVariants.Sum(v => v.Stock ?? 0);
        }

        public static double GetAverageRating(this Product product)
        {
            if (product.Reviews == null || !product.Reviews.Any())
                return 0;

            return product.Reviews.Average(r => r.Rating ?? 0);
        }
    }
}