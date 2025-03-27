using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.ViewModel
{
    public class ThemeCollection
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public bool IsFeatured { get; set; }
        public string BadgeText { get; set; }
        public string Size { get; set; } // "large", "normal", "wide"
    }
}
