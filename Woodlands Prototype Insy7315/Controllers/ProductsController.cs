using Microsoft.AspNetCore.Mvc;
using Woodlands_Prototype_Insy7315.Data;
using Woodlands_Prototype_Insy7315.Models;
using PostgrestConstants = Supabase.Postgrest.Constants;
using SupabaseClient = Supabase.Client;

namespace Woodlands_Prototype_Insy7315.Controllers
{
    public class ProductsController : Controller
    {
        private readonly SupabaseClient _supabase;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            SupabaseClient supabase,
            ILogger<ProductsController> logger)
        {
            _supabase = supabase;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? category)
        {
            try
            {
                var response = await _supabase
                    .From<SupabaseProduct>()
                    .Select("*")
                    .Order("category", PostgrestConstants.Ordering.Ascending)
                    .Order("title", PostgrestConstants.Ordering.Ascending)
                    .Get();

                var products = response.Models
                    .Select(ToProduct)
                    .ToList();

                string? activeCategoryLabel = null;

                if (!string.IsNullOrWhiteSpace(category) &&
                    WoodLinkData.SlugToCategory.TryGetValue(category, out var label))
                {
                    activeCategoryLabel = label;
                }

                var filteredProducts = activeCategoryLabel == null
                    ? products
                    : products
                        .Where(p => p.Category == activeCategoryLabel)
                        .ToList();

                var categories = WoodLinkData.Categories
                    .Select(c => new ProductCategory
                    {
                        Id = c.Id,
                        Label = c.Label,
                        Image = c.Image,
                        Slug = c.Slug,
                        Description = c.Description,
                        Count = products.Count(p => p.Category == c.Id)
                    })
                    .ToList();

                var subtitle = activeCategoryLabel == null
                    ? "Browse our full range of custom-built units and board services."
                    : categories
                        .FirstOrDefault(c => c.Id == activeCategoryLabel)
                        ?.Description ?? "";

                var vm = new ProductsIndexViewModel
                {
                    ActiveCategory = activeCategoryLabel,
                    PageSubtitle = subtitle,
                    Products = filteredProducts,
                    Categories = categories,
                    TotalProductCount = products.Count
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products from Supabase");

                TempData["Error"] = "Unable to load products.";

                var emptyVm = new ProductsIndexViewModel
                {
                    ActiveCategory = null,
                    PageSubtitle = "Browse our full range of custom-built units and board services.",
                    Products = new List<Product>(),
                    Categories = WoodLinkData.Categories
                        .Select(c => new ProductCategory
                        {
                            Id = c.Id,
                            Label = c.Label,
                            Image = c.Image,
                            Slug = c.Slug,
                            Description = c.Description,
                            Count = 0
                        })
                        .ToList(),
                    TotalProductCount = 0
                };

                return View(emptyVm);
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var response = await _supabase
                    .From<SupabaseProduct>()
                    .Where(p => p.Id == id)
                    .Single();

                if (response == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                var product = ToProduct(response);

                var relatedResponse = await _supabase
                    .From<SupabaseProduct>()
                    .Where(p => p.Category == product.Category)
                    .Order("title", PostgrestConstants.Ordering.Ascending)
                    .Get();

                var related = relatedResponse.Models
                    .Where(p => p.Id != product.Id)
                    .Take(3)
                    .Select(ToProduct)
                    .ToList();

                var vm = new ProductDetailViewModel
                {
                    Product = product,
                    CategorySlug = WoodLinkData.CategoryToSlug
                        .GetValueOrDefault(product.Category, ""),
                    Related = related
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product {ProductId}", id);

                return RedirectToAction(nameof(Index));
            }
        }

        private static Product ToProduct(SupabaseProduct product)
        {
            return new Product
            {
                Id = product.Id,
                Category = product.Category,
                Title = product.Title,
                Tagline = product.Tagline,
                Description = product.Description,
                Image = product.Image,
                GalleryJson = product.Gallery ?? "[]",
                FeaturesJson = product.Features ?? "[]",
                FinishesJson = product.Finishes ?? "[]",
                LeadTime = product.LeadTime,
                Tag = product.Tag,
                Price = product.Price
            };
        }
    }
}