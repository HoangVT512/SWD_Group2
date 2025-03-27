using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.ViewModel
{
    public class HomeViewModel
    {
        public List<SliderItem> HeroSliders { get; set; }
        public List<Category> FeaturedCategories { get; set; }
        public List<Product> FeaturedProducts { get; set; }
        public List<Product> NewProducts { get; set; }
        public List<Product> FlashSaleProducts { get; set; }
        public List<ThemeCollection> ThemeCollections { get; set; }
    }
}
