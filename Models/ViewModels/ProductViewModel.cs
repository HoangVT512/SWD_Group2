using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required.")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Supplier is required.")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Base Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Base Price must be greater than 0.")]
        public decimal BasePrice { get; set; }

        [Range(0, 100, ErrorMessage = "Discount Percent must be between 0 and 100.")]
        public decimal? DiscountPercent { get; set; }

        public bool Featured { get; set; } = false;

        public bool? Status { get; set; } = true;

        public string? Description { get; set; }

        public List<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public List<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    }
}