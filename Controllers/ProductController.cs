using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApplication1.Controllers
{
    public class ProductController : Controller
    {
        private readonly ClothingShopDbContext _context;

        public ProductController(ClothingShopDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search, string sortBy, int page = 1, int pageSize = 10)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.ProductName.Contains(search));
            }

            switch (sortBy)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.BasePrice);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.BasePrice);
                    break;
                case "name_asc":
                    query = query.OrderBy(p => p.ProductName);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(p => p.ProductName);
                    break;
                default:
                    query = query.OrderBy(p => p.ProductId);
                    break;
            }

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var products = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.PageNumber = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(products);
        }

        public IActionResult AddProduct()
        {
            var model = new ProductViewModel();
            ViewBag.Categories = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            });
            ViewBag.Suppliers = _context.Suppliers.Select(s => new SelectListItem
            {
                Value = s.SupplierId.ToString(),
                Text = s.SupplierName
            });

            return View(model);
        }

        [HttpPost]
        public IActionResult AddProduct(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {

                ViewBag.Categories = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                });
                ViewBag.Suppliers = _context.Suppliers.Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.SupplierName
                });
                return View(model);
            }

            var product = new Product
            {
                ProductName = model.ProductName,
                CategoryId = model.CategoryId,
                SupplierId = model.SupplierId,
                BasePrice = model.BasePrice,
                DiscountPercent = model.DiscountPercent,
                Status = model.Status,
                Description = model.Description,
                Featured = model.Featured,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult EditProduct(int id)
        {
            var product = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .ThenInclude(v => v.Color)
                .Include(p => p.ProductVariants)
                .ThenInclude(v => v.Material)
                .Include(p => p.ProductVariants)
                .ThenInclude(v => v.Size)
                .FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();

            var model = new ProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                CategoryId = product.CategoryId,
                SupplierId = product.SupplierId ?? 0,
                BasePrice = product.BasePrice,
                DiscountPercent = product.DiscountPercent,
                Status = product.Status,
                Description = product.Description,
                Featured = product.Featured,
                ProductImages = [.. product.ProductImages],
                ProductVariants = [.. product.ProductVariants]
            };


            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName", model.CategoryId);
            ViewBag.Suppliers = new SelectList(_context.Suppliers, "SupplierId", "SupplierName", model.SupplierId);

            ViewBag.Colors = new SelectList(_context.Colors, "ColorId", "ColorName");
            ViewBag.Sizes = new SelectList(_context.Sizes, "SizeId", "SizeName");
            ViewBag.Materials = new SelectList(_context.Materials, "MaterialId", "MaterialName");

            return View(model);
        }

        [HttpPost]
        public IActionResult EditProduct(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName", model.CategoryId);
                ViewBag.Suppliers = new SelectList(_context.Suppliers, "SupplierId", "SupplierName", model.SupplierId);

                ViewBag.Colors = new SelectList(_context.Colors, "ColorId", "ColorName");
                ViewBag.Sizes = new SelectList(_context.Sizes, "SizeId", "SizeName");
                ViewBag.Materials = new SelectList(_context.Materials, "MaterialId", "MaterialName");

                return View(model);
            }

            var product = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .ThenInclude(v => v.Color)
                .Include(p => p.ProductVariants)
                .ThenInclude(v => v.Material)
                .Include(p => p.ProductVariants)
                .ThenInclude(v => v.Size)
                .FirstOrDefault(p => p.ProductId == model.ProductId);
            if (product == null) return NotFound();

            product.ProductName = model.ProductName;
            product.CategoryId = model.CategoryId;
            product.SupplierId = model.SupplierId;
            product.BasePrice = model.BasePrice;
            product.DiscountPercent = model.DiscountPercent;
            product.Status = model.Status;
            product.Description = model.Description;
            product.Featured = model.Featured;
            product.UpdatedAt = DateTime.Now;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();


            if (product.ProductImages != null && product.ProductImages.Any())
            {
                _context.ProductImages.RemoveRange(product.ProductImages);
            }

            if (product.ProductVariants != null && product.ProductVariants.Any())
            {
                _context.ProductVariants.RemoveRange(product.ProductVariants);
            }

            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddImage(IFormFile ImageFile, int ProductId, bool? IsPrimary)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var filePath = Path.Combine("wwwroot/images", ImageFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                var productImage = new ProductImage
                {
                    ProductId = ProductId,
                    ImageUrl = "/images/" + ImageFile.FileName,
                    IsPrimary = IsPrimary ?? false
                };

                _context.ProductImages.Add(productImage);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("EditProduct", new { id = ProductId });
        }

        [HttpPost]
        public IActionResult EditImage(int ImageId, int ProductId, string ImageUrl, bool? IsPrimary)
        {
            var image = _context.ProductImages.Find(ImageId);
            if (image == null) return NotFound();

            image.ImageUrl = ImageUrl;
            image.IsPrimary = IsPrimary ?? false;

            _context.ProductImages.Update(image);
            _context.SaveChanges();

            return RedirectToAction("EditProduct", new { id = ProductId });
        }

        [HttpGet]
        public IActionResult DeleteImage(int id)
        {
            var image = _context.ProductImages.Find(id);
            if (image == null) return NotFound();

            _context.ProductImages.Remove(image);
            _context.SaveChanges();

            return RedirectToAction("EditProduct", new { id = image.ProductId });
        }


        [HttpPost]
        public async Task<IActionResult> AddVariant(ProductVariantViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newVariant = new ProductVariant
                {
                    ProductId = model.ProductId,
                    SizeId = model.SizeId,
                    ColorId = model.ColorId,
                    MaterialId = model.MaterialId,
                    Sku = model.Sku,
                    Stock = model.Stock,
                    Image = model.Image,
                    AdditionalPrice = model.AdditionalPrice
                };
                _context.ProductVariants.Add(newVariant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(EditProduct), new { id = model.ProductId });
            }

            return RedirectToAction(nameof(EditProduct), new { id = model.ProductId });
        }

        [HttpPost]
        public async Task<IActionResult> EditVariant(ProductVariantViewModel model)
        {
            if (ModelState.IsValid)
            {
                var variant = _context.ProductVariants.Find(model.VariantId);
                if (variant != null)
                {
                    variant.SizeId = model.SizeId;
                    variant.ColorId = model.ColorId;
                    variant.MaterialId = model.MaterialId;
                    variant.Sku = model.Sku;
                    variant.Stock = model.Stock;
                    variant.Image = model.Image;
                    variant.AdditionalPrice = model.AdditionalPrice;

                    _context.SaveChanges();
                }
                return RedirectToAction(nameof(EditProduct), new { id = model.ProductId });
            }

            return RedirectToAction(nameof(EditProduct), new { id = model.ProductId });
        }

        [HttpGet]
        public IActionResult DeleteVariant(int id)
        {
            var variant = _context.ProductVariants.Find(id);
            if (variant == null) return NotFound();

            _context.ProductVariants.Remove(variant);
            _context.SaveChanges();

            return RedirectToAction("EditProduct", new { id = variant.ProductId });
        }

        public IActionResult ProductDetail(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Size)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Color)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Material)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }
    }
}
