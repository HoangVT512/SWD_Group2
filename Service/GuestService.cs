using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.ViewModel;

namespace WebApplication1.Service
{
    public class GuestService : IGuestService
    {
        private readonly ClothingShopDbContext _context;

        public GuestService(ClothingShopDbContext context)
        {
            _context = context;
        }

        #region Category Methods
        public async Task<List<Category>> GetFeaturedCategoriesAsync(int count = 4)
        {
            // Return top-level categories with the most products
            return await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .Include(c => c.Products.Where(p => p.Status == true))
                .OrderByDescending(c => c.Products.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Category>> GetMainCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .Include(c => c.InverseParentCategory)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.Categories
                .Include(c => c.InverseParentCategory)
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }

        public async Task<List<Category>> GetSubcategoriesAsync(int parentCategoryId)
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == parentCategoryId)
                .ToListAsync();
        }
        #endregion

        #region Product Methods
        public async Task<List<Product>> GetFeaturedProductsAsync(int count = 4)
        {
            return await _context.Products
                .Where(p => p.Status == true && p.Featured == true)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .OrderBy(p => Guid.NewGuid()) // Random selection
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Product>> GetNewProductsAsync(int count = 6)
        {
            return await _context.Products
                .Where(p => p.Status == true)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Product>> GetProductsWithHighestDiscountAsync(int count = 6)
        {
            return await _context.Products
                .Where(p => p.Status == true && p.DiscountPercent > 0)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .OrderByDescending(p => p.DiscountPercent)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int count = 8)
        {
            // Get the category and its subcategories
            var category = await _context.Categories
                .Include(c => c.InverseParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null)
                return new List<Product>();

            // Get category IDs including the main category and its subcategories
            var categoryIds = new List<int> { categoryId };
            categoryIds.AddRange(category.InverseParentCategory.Select(c => c.CategoryId));

            return await _context.Products
                .Where(p => p.Status == true && categoryIds.Contains(p.CategoryId))
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Size)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Color)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Material)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.Status == true);
        }

        public async Task<double> GetProductAverageRatingAsync(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId && r.Status == "Approved")
                .ToListAsync();

            if (reviews.Count == 0)
                return 0;

            // Use null-coalescing operator to handle nullable rating average
            return reviews.Average(r => r.Rating) ?? 0.0;
        }

        public async Task<int> GetProductReviewCountAsync(int productId)
        {
            return await _context.Reviews
                .CountAsync(r => r.ProductId == productId && r.Status == "Approved");
        }
        #endregion

        #region Home Page Methods
        public async Task<HomeViewModel> GetHomePageDataAsync()
        {
            var featuredProducts = await GetFeaturedProductsAsync(4);
            var newProducts = await GetNewProductsAsync(6);
            var flashSaleProducts = await GetProductsWithHighestDiscountAsync(6);
            var featuredCategories = await GetFeaturedCategoriesAsync(4);

            // Hardcoded slider data (in real app, this would come from database)
            var sliders = GetHeroSliders();

            // Hardcoded theme collections (in real app, this would come from database)
            var themeCollections = GetThemeCollections();

            return new HomeViewModel
            {
                HeroSliders = sliders,
                FeaturedCategories = featuredCategories,
                FeaturedProducts = featuredProducts,
                NewProducts = newProducts,
                FlashSaleProducts = flashSaleProducts,
                ThemeCollections = themeCollections
            };
        }

        private List<SliderItem> GetHeroSliders()
        {
            return new List<SliderItem>
            {
                new SliderItem
                {
                    Id = 1,
                    ImageUrl = "https://images.unsplash.com/photo-1490481651871-ab68de25d43d?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=2070&q=80",
                    Title = "Bộ Sưu Tập Mùa Hè 2025",
                    Subtitle = "Khám phá các thiết kế mới nhất với ưu đãi đặc biệt giảm giá lên đến 30%",
                    PrimaryButtonText = "Khám phá ngay",
                    PrimaryButtonUrl = "/collections/summer",
                    SecondaryButtonText = "Xem khuyến mãi",
                    SecondaryButtonUrl = "/sale",
                    IsContentRight = false
                },
                new SliderItem
                {
                    Id = 2,
                    ImageUrl = "https://images.unsplash.com/photo-1445205170230-053b83016050?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=2071&q=80",
                    Title = "Thời Trang Nam Mới Nhất",
                    Subtitle = "Phong cách lịch lãm, trẻ trung cho quý ông hiện đại",
                    PrimaryButtonText = "Mua ngay",
                    PrimaryButtonUrl = "/products?category=men",
                    SecondaryButtonText = "Xem bộ sưu tập",
                    SecondaryButtonUrl = "/lookbook/men",
                    IsContentRight = false
                },
                new SliderItem
                {
                    Id = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1469334031218-e382a71b716b?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=2070&q=80",
                    Title = "Phong Cách Nữ Thanh Lịch",
                    Subtitle = "Tôn vinh vẻ đẹp phái nữ với thiết kế độc đáo",
                    PrimaryButtonText = "Mua sắm ngay",
                    PrimaryButtonUrl = "/products?category=women",
                    SecondaryButtonText = "Hàng mới về",
                    SecondaryButtonUrl = "/new-arrivals",
                    IsContentRight = true
                }
            };
        }

        private List<ThemeCollection> GetThemeCollections()
        {
            return new List<ThemeCollection>
            {
                new ThemeCollection
                {
                    Id = 1,
                    Name = "Bộ Sưu Tập Mùa Hè",
                    Description = "Tự tin tỏa sáng với những thiết kế rực rỡ, năng động cho ngày nắng",
                    ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=2124&q=80",
                    Url = "/collections/summer",
                    IsFeatured = true,
                    BadgeText = "Nổi bật",
                    Size = "large"
                },
                new ThemeCollection
                {
                    Id = 2,
                    Name = "Đồ Đi Biển",
                    Description = "Trang phục và phụ kiện cho những ngày nghỉ dưỡng tại bãi biển",
                    ImageUrl = "https://images.unsplash.com/photo-1520865925600-c6f70d10b098?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1974&q=80",
                    Url = "/collections/beach",
                    Size = "normal"
                },
                new ThemeCollection
                {
                    Id = 3,
                    Name = "Thời Trang Công Sở",
                    Description = "Lịch lãm, tinh tế, chuyên nghiệp cho môi trường làm việc",
                    ImageUrl = "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1976&q=80",
                    Url = "/collections/office",
                    Size = "normal"
                },
                new ThemeCollection
                {
                    Id = 4,
                    Name = "Casual Everyday",
                    Description = "Thoải mái, năng động và linh hoạt cho mọi hoạt động hàng ngày",
                    ImageUrl = "https://images.unsplash.com/photo-1551232864-3f0890e580d9?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1974&q=80",
                    Url = "/collections/casual",
                    Size = "wide"
                },
                new ThemeCollection
                {
                    Id = 5,
                    Name = "Thể Thao & Fitness",
                    Description = "Trang phục chuyên dụng cho các hoạt động thể thao và tập luyện",
                    ImageUrl = "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=2070&q=80",
                    Url = "/collections/sports",
                    Size = "normal"
                }
            };
        }
        #endregion



        public async Task<ProductDetailViewModel> GetProductDetailAsync(int productId)
        {
            // Get the product with all its related data
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Size)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Color)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Material)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.Status == true);

            if (product == null)
                return null;

            // Get average rating and review statistics
            var reviews = product.Reviews.Where(r => r.Status == "Approved").ToList();
            double averageRating = reviews.Any() ? reviews.Average(r => r.Rating ?? 0) : 0;

            // Create rating distribution
            var ratingDistribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                ratingDistribution[i] = reviews.Count(r => r.Rating == i);
            }

            // Create the variant DTOs to avoid circular references
            var variantDtos = product.ProductVariants.Select(v => new ProductVariantDto
            {
                VariantId = v.VariantId,
                ProductId = v.ProductId,
                ColorId = v.ColorId,
                ColorName = v.Color?.ColorName,
                ColorCode = v.Color?.ColorCode,
                SizeId = v.SizeId,
                SizeName = v.Size?.SizeName,
                MaterialId = v.MaterialId,
                MaterialName = v.Material?.MaterialName,
                AdditionalPrice = v.AdditionalPrice,
                Stock = v.Stock,
                Sku = v.Sku
            }).ToList();

            // Transform product data into view model
            var viewModel = new ProductDetailViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                BasePrice = product.BasePrice,
                DiscountPercent = product.DiscountPercent ?? 0,
                DiscountedPrice = product.BasePrice - (product.BasePrice * (product.DiscountPercent ?? 0) / 100),
                Featured = product.Featured ?? false,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName,

                // Images
                ProductImages = product.ProductImages.OrderByDescending(i => i.IsPrimary).ToList(),
                PrimaryImage = product.ProductImages.FirstOrDefault(i => i.IsPrimary == true)?.ImageUrl ??
                               product.ProductImages.FirstOrDefault()?.ImageUrl,

                // Variants
                ColorOptions = product.ProductVariants
                    .Select(v => v.Color)
                    .Where(c => c != null)
                    .GroupBy(c => c.ColorId)
                    .Select(g => g.First())
                    .ToList(),

                SizeOptions = product.ProductVariants
                    .Select(v => v.Size)
                    .Where(s => s != null)
                    .GroupBy(s => s.SizeId)
                    .Select(g => g.First())
                    .ToList(),

                MaterialOptions = product.ProductVariants
                    .Where(v => v.Material != null)
                    .Select(v => v.Material)
                    .GroupBy(m => m.MaterialId)
                    .Select(g => g.First())
                    .ToList(),

                ProductVariants = product.ProductVariants.ToList(),
                ProductVariantDtos = variantDtos,

                // Reviews
                AverageRating = averageRating,
                ReviewCount = reviews.Count,
                RatingDistribution = ratingDistribution,
                Reviews = reviews.OrderByDescending(r => r.CreatedAt).ToList(),

                // Related Products
                RelatedProducts = await GetRelatedProductsAsync(productId, product.CategoryId)
            };

            return viewModel;
        }


        public async Task<List<CategoryViewModel>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                })
                .ToListAsync();
        }

        public async Task<List<ProductCardViewModel>> SearchProductsAsync(string query, int? categoryId)
        {
            // Start with all active products
            var searchQuery = _context.Products.Where(p => p.Status == true);

            // Apply search filters only if provided
            if (!string.IsNullOrWhiteSpace(query))
            {
                searchQuery = searchQuery.Where(p => p.ProductName.Contains(query) ||
                                                  p.Description.Contains(query));
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                searchQuery = searchQuery.Where(p => p.CategoryId == categoryId);
            }

            // Get results with images and pricing info
            var results = await searchQuery
                .Include(p => p.ProductImages)
                .OrderBy(p => p.ProductName)
                .Select(p => new ProductCardViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    BasePrice = p.BasePrice,
                    DiscountPercent = p.DiscountPercent ?? 0,
                    DiscountedPrice = p.BasePrice - (p.BasePrice * (p.DiscountPercent ?? 0) / 100),
                    ImageUrl = p.ProductImages.FirstOrDefault(i => i.IsPrimary == true).ImageUrl ??
                               p.ProductImages.FirstOrDefault().ImageUrl ?? "/images/no-image.png"
                })
                .ToListAsync();

            return results;
        }

        

        private async Task<List<ProductCardViewModel>> GetRelatedProductsAsync(int productId, int categoryId, int count = 4)
        {
            try
            {
                // Strategy:
                // 1. First try to get products from the same category
                // 2. If not enough, get some featured products
                // 3. If still not enough, get newest products

                var relatedProducts = new List<Product>();

                // 1. Get products from the same category (excluding current product)
                var sameCategoryProducts = await _context.Products
                    .Where(p => p.ProductId != productId && p.CategoryId == categoryId && p.Status == true)
                    .Include(p => p.ProductImages)
                    .OrderByDescending(p => p.Featured)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(count)
                    .ToListAsync();

                relatedProducts.AddRange(sameCategoryProducts);

                // 2. If we need more products, get featured products from other categories
                if (relatedProducts.Count < count)
                {
                    int remaining = count - relatedProducts.Count;

                    var featuredProducts = await _context.Products
                        .Where(p => p.ProductId != productId &&
                                   !relatedProducts.Select(r => r.ProductId).Contains(p.ProductId) &&
                                   p.Status == true &&
                                   p.Featured == true)
                        .Include(p => p.ProductImages)
                        .OrderByDescending(p => p.CreatedAt)
                        .Take(remaining)
                        .ToListAsync();

                    relatedProducts.AddRange(featuredProducts);
                }

                if (relatedProducts.Count < count)
                {
                    int remaining = count - relatedProducts.Count;

                    var newestProducts = await _context.Products
                        .Where(p => p.ProductId != productId &&
                                   !relatedProducts.Select(r => r.ProductId).Contains(p.ProductId) &&
                                   p.Status == true)
                        .Include(p => p.ProductImages)
                        .OrderByDescending(p => p.CreatedAt)
                        .Take(remaining)
                        .ToListAsync();

                    relatedProducts.AddRange(newestProducts);
                }

                return relatedProducts.Select(p => new ProductCardViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    BasePrice = p.BasePrice,
                    DiscountPercent = p.DiscountPercent ?? 0,
                    DiscountedPrice = p.BasePrice - (p.BasePrice * (p.DiscountPercent ?? 0) / 100),
                    ImageUrl = p.ProductImages.FirstOrDefault(i => i.IsPrimary == true)?.ImageUrl ??
                               p.ProductImages.FirstOrDefault()?.ImageUrl ??
                               "/images/no-image.png"
                }).ToList();
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error getting related products: {ex.Message}");
                // Return empty list on error
                return new List<ProductCardViewModel>();
            }
        }
    }
}