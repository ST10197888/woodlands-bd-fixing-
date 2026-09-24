using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Woodlands_Prototype_Insy7315.Models;

namespace Woodlands_Prototype_Insy7315.Controllers
{
    [Authorize(Roles = "Admin,Manager (Soweto),Manager (Roodepoort),Manager (Randfontein)")]
    public class ManagementController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<ManagementController> _logger;
        private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

        public ManagementController(IHttpClientFactory http, ILogger<ManagementController> logger)
        {
            _http = http;
            _logger = logger;
        }

        // PRODUCTS 

        public async Task<IActionResult> Products(string? category)
        {
            var products = new List<Product>();
            var allCategories = new List<string>();

            try
            {
                var client = _http.CreateClient("NodeApi");
                var res = await client.GetAsync("api/products");
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    products = JsonSerializer.Deserialize<List<Product>>(json, _json) ?? new();

                    allCategories = products
                        .Select(p => p.Category)
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct()
                        .OrderBy(c => c)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading management products");
            }

            if (!string.IsNullOrWhiteSpace(category))
                products = products.Where(p => p.Category == category).ToList();

            ViewBag.ActiveCategory = category;
            ViewBag.Categories = allCategories;

            return View(products);
        }

        [HttpGet]
        public IActionResult CreateProduct() => View("ProductForm", new ProductFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductFormViewModel model)
        {
            if (!ModelState.IsValid) return View("ProductForm", model);

            try
            {
                var payload = new
                {
                    category = model.Category,
                    title = model.Title,
                    tagline = model.Tagline,
                    description = model.Description,
                    image = model.Image,
                    gallery = JsonSerializer.Serialize(Lines(model.Gallery)),
                    features = JsonSerializer.Serialize(Lines(model.Features)),
                    lead_time = model.LeadTime,
                    tag = model.Tag,
                    price = model.Price,
                    is_from_price = model.IsFromPrice,
                };

                var client = _http.CreateClient("NodeApi");
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                await client.PostAsync("api/products", content);

                return RedirectToAction(nameof(Products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError("", "Unable to create the product.");
                return View("ProductForm", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            try
            {
                var client = _http.CreateClient("NodeApi");
                var res = await client.GetAsync($"api/products/{id}");
                if (!res.IsSuccessStatusCode) return NotFound();

                var json = await res.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<Product>(json, _json);
                if (product == null) return NotFound();

                return View("ProductForm", ToForm(product));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product {Id}", id);
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(string id, ProductFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Id = id;
                return View("ProductForm", model);
            }

            try
            {
                var payload = new
                {
                    category = model.Category,
                    title = model.Title,
                    tagline = model.Tagline ?? "",
                    description = model.Description ?? "",
                    image = model.Image ?? "",
                    lead_time = model.LeadTime ?? "",
                    tag = model.Tag,
                    price = model.Price,
                    is_from_price = model.IsFromPrice,
                    gallery = JsonSerializer.Serialize(Lines(model.Gallery)),
                    features = JsonSerializer.Serialize(Lines(model.Features)),
                    finishes = JsonSerializer.Serialize(Lines(model.Finishes))
                };

                var client = _http.CreateClient("NodeApi");
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"api/products/{id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errJson = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Node API update error: {Error}", errJson);
                    ModelState.AddModelError("", "Unable to update the product.");
                    model.Id = id;
                    return View("ProductForm", model);
                }

                return RedirectToAction(nameof(Products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {Id}", id);
                ModelState.AddModelError("", "Unable to update the product.");
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
                var client = _http.CreateClient("NodeApi");
                await client.DeleteAsync($"api/products/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {Id}", id);
            }

            return RedirectToAction(nameof(Products));
        }

        // SERVICES 

        public async Task<IActionResult> Services()
        {
            var services = new List<Service>();
            try
            {
                var client = _http.CreateClient("NodeApi");
                var res = await client.GetAsync("api/services");
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    services = JsonSerializer.Deserialize<List<Service>>(json, _json) ?? new();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading services");
            }

            return View(services);
        }

        [HttpGet]
        public IActionResult CreateService() => View("ServiceForm", new Service { IsActive = true });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(Service model)
        {
            if (!ModelState.IsValid) return View("ServiceForm", model);

            try
            {
                var payload = new
                {
                    name = model.Name,
                    description = model.Description,
                    image = model.Image,
                    is_active = model.IsActive
                };

                var client = _http.CreateClient("NodeApi");
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                await client.PostAsync("api/services", content);

                return RedirectToAction(nameof(Services));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service");
                ModelState.AddModelError("", "Unable to create the service.");
                return View("ServiceForm", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditService(int id)
        {
            try
            {
                var client = _http.CreateClient("NodeApi");
                var res = await client.GetAsync("api/services");
                if (!res.IsSuccessStatusCode) return NotFound();

                var json = await res.Content.ReadAsStringAsync();
                var services = JsonSerializer.Deserialize<List<Service>>(json, _json) ?? new();
                var service = services.FirstOrDefault(s => s.Id == id);
                if (service == null) return NotFound();

                return View("ServiceForm", service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service {Id}", id);
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(int id, Service model)
        {
            if (!ModelState.IsValid) { model.Id = id; return View("ServiceForm", model); }

            try
            {
                var payload = new
                {
                    name = model.Name,
                    description = model.Description,
                    image = model.Image,
                    is_active = model.IsActive
                };

                var client = _http.CreateClient("NodeApi");
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                await client.PutAsync($"api/services/{id}", content);

                return RedirectToAction(nameof(Services));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service {Id}", id);
                ModelState.AddModelError("", "Unable to update the service.");
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
                var client = _http.CreateClient("NodeApi");
                await client.DeleteAsync($"api/services/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service {Id}", id);
            }

            return RedirectToAction(nameof(Services));
        }

        // HELPERS

        private static ProductFormViewModel ToForm(Product product)
        {
            return new ProductFormViewModel
            {
                Id = product.Id,
                Category = product.Category,
                Title = product.Title,
                IsFromPrice = product.IsFromPrice,
                Tagline = product.Tagline,
                Description = product.Description,
                Image = product.Image,
                Gallery = string.Join(Environment.NewLine, product.Gallery),
                Features = string.Join(Environment.NewLine, product.Features),
                Finishes = string.Join(Environment.NewLine, product.Finishes),
                LeadTime = product.LeadTime,
                Tag = product.Tag,
                Price = product.Price
            };
        }

        private static List<string> Lines(string value) =>
            (value ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}