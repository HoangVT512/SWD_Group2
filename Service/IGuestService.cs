using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.ViewModel;

namespace WebApplication1.Service
{
    public interface IGuestService 
    {
        // Category methods
        Task<List<Category>> GetFeaturedCategoriesAsync(int count = 4);
        Task<List<Category>> GetMainCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(int categoryId);
        Task<List<Category>> GetSubcategoriesAsync(int parentCategoryId);

        // Product methods
        Task<List<Product>> GetFeaturedProductsAsync(int count = 4);
        Task<List<Product>> GetNewProductsAsync(int count = 6);
        Task<List<Product>> GetProductsWithHighestDiscountAsync(int count = 6);
        Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int count = 8);
        Task<Product> GetProductByIdAsync(int productId);
        Task<double> GetProductAverageRatingAsync(int productId);
        Task<int> GetProductReviewCountAsync(int productId);

        // Home page methods
        Task<HomeViewModel> GetHomePageDataAsync();

        // Add this method to your existing IGuestService interface
        Task<ProductDetailViewModel> GetProductDetailAsync(int productId);


        Task<List<CategoryViewModel>> GetAllCategoriesAsync();

        Task<List<ProductCardViewModel>> SearchProductsAsync(string query, int? categoryId);

    }
}
