using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostgrestConstants = Supabase.Postgrest.Constants;
using SupabaseClient = Supabase.Client;
using Woodlands_Prototype_Insy7315.Models;

namespace Woodlands_Prototype_Insy7315.Controllers
{
    [Authorize(Roles = "Admin,Manager (Soweto),Manager (Roodepoort),Manager (Randfontein)")]
    public class ManagementController : Controller
    {
        private readonly SupabaseClient _supabase;
        private readonly ILogger<ManagementController> _logger;

        public ManagementController(
            SupabaseClient supabase,
            ILogger<ManagementController> logger)
        {
            _supabase = supabase;
            _logger = logger;
        }

        public async Task<IActionResult> Products(string? category)
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

                if (!string.IsNullOrWhiteSpace(category))
                {
                    products = products
                        .Where(p => p.Category == category)
                        .ToList();
                }

                ViewBag.ActiveCategory = category;

                ViewBag.Categories = response.Models
                    .Select(p => p.Category)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading management products");

                ViewBag.ActiveCategory = category;
                ViewBag.Categories = new List<string>();

                return View(new List<Product>());
            }
        }

        [HttpGet]
        public IActionResult CreateProduct()
        {
            return View("ProductForm", new ProductFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ProductForm", model);
            }

            try
            {
                var baseId = CreateSlug(model.Title);
                var id = baseId;
                var counter = 2;

                while (await ProductExists(id))
                {
                    id = $"{baseId}-{counter++}";
                }

                var product = new SupabaseProduct
                {
                    Id = id,
                    Category = model.Category,
                    Title = model.Title,
                    Tagline = model.Tagline,
                    Description = model.Description,
                    Image = model.Image,
                    Gallery = WriteList(Lines(model.Gallery)),
                    Features = WriteList(Lines(model.Features)),
                    Finishes = WriteList(Lines(model.Finishes)),
                    LeadTime = model.LeadTime,
                    Tag = model.Tag,
                    Price = model.Price
                };

                await _supabase
                    .From<SupabaseProduct>()
                    .Insert(product);

                return RedirectToAction(nameof(Products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");

                ModelState.AddModelError(
                    "",
                    "Unable to create the product.");

                return View("ProductForm", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            try
            {
                var response = await _supabase
                    .From<SupabaseProduct>()
                    .Where(p => p.Id == id)
                    .Single();

                if (response == null)
                {
                    return NotFound();
                }

                return View("ProductForm", ToForm(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product {ProductId}", id);

                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(
            string id,
            ProductFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Id = id;
                return View("ProductForm", model);
            }

            try
            {
                var response = await _supabase
                    .From<SupabaseProduct>()
                    .Where(p => p.Id == id)
                    .Single();

                if (response == null)
                {
                    return NotFound();
                }

                response.Category = model.Category;
                response.Title = model.Title;
                response.Tagline = model.Tagline;
                response.Description = model.Description;
                response.Image = model.Image;
                response.Gallery = WriteList(Lines(model.Gallery));
                response.Features = WriteList(Lines(model.Features));
                response.Finishes = WriteList(Lines(model.Finishes));
                response.LeadTime = model.LeadTime;
                response.Tag = model.Tag;
                response.Price = model.Price;

                await _supabase
                    .From<SupabaseProduct>()
                    .Update(response);

                return RedirectToAction(nameof(Products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);

                ModelState.AddModelError(
                    "",
                    "Unable to update the product.");

                model.Id = id;

                return View("ProductForm", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            try
            {
                var response = await _supabase
                    .From<SupabaseProduct>()
                    .Where(p => p.Id == id)
                    .Single();

                if (response != null)
                {
                    await _supabase
                        .From<SupabaseProduct>()
                        .Delete(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
            }

            return RedirectToAction(nameof(Products));
        }

        public async Task<IActionResult> Services()
        {
            try
            {
                var response = await _supabase
                    .From<SupabaseService>()
                    .Select("*")
                    .Order("name", PostgrestConstants.Ordering.Ascending)
                    .Get();

                var services = response.Models
                    .Select(s => new Service
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        Image = s.Image,
                        IsActive = s.IsActive
                    })
                    .ToList();

                return View(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading management services");

                return View(new List<Service>());
            }
        }

        [HttpGet]
        public IActionResult CreateService()
        {
            return View("ServiceForm", new Service
            {
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(Service model)
        {
            if (!ModelState.IsValid)
            {
                return View("ServiceForm", model);
            }

            try
            {
                var service = new SupabaseService
                {
                    Name = model.Name,
                    Description = model.Description,
                    Image = model.Image,
                    IsActive = model.IsActive
                };

                await _supabase
                    .From<SupabaseService>()
                    .Insert(service);

                return RedirectToAction(nameof(Services));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service");

                ModelState.AddModelError(
                    "",
                    "Unable to create the service.");

                return View("ServiceForm", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditService(int id)
        {
            try
            {
                var response = await _supabase
                    .From<SupabaseService>()
                    .Where(s => s.Id == id)
                    .Single();

                if (response == null)
                {
                    return NotFound();
                }

                return View("ServiceForm", new Service
                {
                    Id = response.Id,
                    Name = response.Name,
                    Description = response.Description,
                    Image = response.Image,
                    IsActive = response.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service {ServiceId}", id);

                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(
            int id,
            Service model)
        {
            if (!ModelState.IsValid)
            {
                model.Id = id;
                return View("ServiceForm", model);
            }

            try
            {
                var response = await _supabase
                    .From<SupabaseService>()
                    .Where(s => s.Id == id)
                    .Single();

                if (response == null)
                {
                    return NotFound();
                }

                response.Name = model.Name;
                response.Description = model.Description;
                response.Image = model.Image;
                response.IsActive = model.IsActive;

                await _supabase
                    .From<SupabaseService>()
                    .Update(response);

                return RedirectToAction(nameof(Services));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service {ServiceId}", id);

                ModelState.AddModelError(
                    "",
                    "Unable to update the service.");

                model.Id = id;

                return View("ServiceForm", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            try
            {
                var response = await _supabase
                    .From<SupabaseService>()
                    .Where(s => s.Id == id)
                    .Single();

                if (response != null)
                {
                    await _supabase
                        .From<SupabaseService>()
                        .Delete(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service {ServiceId}", id);
            }

            return RedirectToAction(nameof(Services));
        }

        private async Task<bool> ProductExists(string id)
        {
            var response = await _supabase
                .From<SupabaseProduct>()
                .Where(p => p.Id == id)
                .Get();

            return response.Models.Any();
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

        private static ProductFormViewModel ToForm(SupabaseProduct product)
        {
            var mapped = ToProduct(product);

            return new ProductFormViewModel
            {
                Id = mapped.Id,
                Category = mapped.Category,
                Title = mapped.Title,
                Tagline = mapped.Tagline,
                Description = mapped.Description,
                Image = mapped.Image,
                Gallery = string.Join(
                    Environment.NewLine,
                    mapped.Gallery),
                Features = string.Join(
                    Environment.NewLine,
                    mapped.Features),
                Finishes = string.Join(
                    Environment.NewLine,
                    mapped.Finishes),
                LeadTime = mapped.LeadTime,
                Tag = mapped.Tag,
                Price = mapped.Price
            };
        }

        private static List<string> Lines(string value)
        {
            return (value ?? "")
                .Split(
                    '\n',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries)
                .ToList();
        }

        private static string WriteList(List<string> values)
        {
            return System.Text.Json.JsonSerializer.Serialize(
                values ?? new List<string>());
        }

        private static string CreateSlug(string title)
        {
            var chars = (title ?? "")
                .ToLowerInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '-')
                .ToArray();

            return string.Join(
                "-",
                new string(chars)
                    .Split(
                        '-',
                        StringSplitOptions.RemoveEmptyEntries));
        }
    }
}