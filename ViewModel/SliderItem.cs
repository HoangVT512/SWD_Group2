using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.ViewModel
{
    public class SliderItem
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string PrimaryButtonText { get; set; }
        public string PrimaryButtonUrl { get; set; }
        public string SecondaryButtonText { get; set; }
        public string SecondaryButtonUrl { get; set; }
        public bool IsContentRight { get; set; }
    }
}
