using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.ViewModel
{
    public class SearchViewModel
    {
        public string Query { get; set; }
        public int? SelectedCategoryId { get; set; }
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public List<ProductCardViewModel> SearchResults { get; set; } = new List<ProductCardViewModel>();
    }

    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
