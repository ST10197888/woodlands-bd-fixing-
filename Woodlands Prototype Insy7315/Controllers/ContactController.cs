using Microsoft.AspNetCore.Mvc;
using PostgrestConstants = Supabase.Postgrest.Constants;
using SupabaseClient = Supabase.Client;
using Woodlands_Prototype_Insy7315.Models;

namespace Woodlands_Prototype_Insy7315.Controllers
{
    public class ContactController : Controller
    {
        private readonly SupabaseClient _supabase;
        private readonly ILogger<ContactController> _logger;

        public ContactController(
            SupabaseClient supabase,
            ILogger<ContactController> logger)
        {
            _supabase = supabase;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? service,
            string? product,
            string? productId)
        {
            try
            {
                var servicesResponse = await _supabase
                    .From<SupabaseService>()
                    .Where(s => s.IsActive == true)
                    .Order("name", PostgrestConstants.Ordering.Ascending)
                    .Get();

                var services = servicesResponse.Models
                    .Select(s => new Service
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        Image = s.Image,
                        IsActive = s.IsActive
                    })
                    .ToList();

                ViewBag.Services = services;

                Product? requestedProduct = null;

                if (!string.IsNullOrWhiteSpace(productId))
                {
                    var productResponse = await _supabase
                        .From<SupabaseProduct>()
                        .Where(p => p.Id == productId)
                        .Single();

                    if (productResponse != null)
                    {
                        requestedProduct = new Product
                        {
                            Id = productResponse.Id,
                            Category = productResponse.Category,
                            Title = productResponse.Title,
                            Tagline = productResponse.Tagline,
                            Description = productResponse.Description,
                            Image = productResponse.Image,
                            GalleryJson = productResponse.Gallery ?? "[]",
                            FeaturesJson = productResponse.Features ?? "[]",
                            FinishesJson = productResponse.Finishes ?? "[]",
                            LeadTime = productResponse.LeadTime,
                            Tag = productResponse.Tag,
                            Price = productResponse.Price
                        };
                    }
                }

                ViewBag.RequestedProduct = requestedProduct;

                var resolvedService = requestedProduct?.Category ?? service;
                var resolvedProductTitle = requestedProduct?.Title ?? product;

                var model = new ContactRequest
                {
                    Service = services.Any(s => s.Name == resolvedService)
                        ? resolvedService!
                        : "",

                    Message = !string.IsNullOrWhiteSpace(resolvedProductTitle)
                        ? $"I'm interested in the \"{resolvedProductTitle}\". "
                        : "",

                    ProductId = requestedProduct?.Id
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contact/quote page");

                ViewBag.Services = new List<Service>();
                ViewBag.RequestedProduct = null;

                return View(new ContactRequest
                {
                    Service = service ?? "",
                    Message = !string.IsNullOrWhiteSpace(product)
                        ? $"I'm interested in the \"{product}\". "
                        : "",
                    ProductId = productId
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactRequest request)
        {
            if (User.IsInRole("Admin") ||
                User.IsInRole("Manager (Soweto)") ||
                User.IsInRole("Manager (Roodepoort)") ||
                User.IsInRole("Manager (Randfontein)"))
            {
                return Forbid();
            }

            try
            {
                var servicesResponse = await _supabase
                    .From<SupabaseService>()
                    .Where(s => s.IsActive == true)
                    .Order("name", PostgrestConstants.Ordering.Ascending)
                    .Get();

                ViewBag.Services = servicesResponse.Models
                    .Select(s => new Service
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        Image = s.Image,
                        IsActive = s.IsActive
                    })
                    .ToList();

                if (!string.IsNullOrWhiteSpace(request.ProductId))
                {
                    var productResponse = await _supabase
                        .From<SupabaseProduct>()
                        .Where(p => p.Id == request.ProductId)
                        .Single();

                    if (productResponse != null)
                    {
                        ViewBag.RequestedProduct = new Product
                        {
                            Id = productResponse.Id,
                            Category = productResponse.Category,
                            Title = productResponse.Title,
                            Tagline = productResponse.Tagline,
                            Description = productResponse.Description,
                            Image = productResponse.Image,
                            GalleryJson = productResponse.Gallery ?? "[]",
                            FeaturesJson = productResponse.Features ?? "[]",
                            FinishesJson = productResponse.Finishes ?? "[]",
                            LeadTime = productResponse.LeadTime,
                            Tag = productResponse.Tag,
                            Price = productResponse.Price
                        };
                    }
                }

                if (!ModelState.IsValid)
                {
                    return View(request);
                }

                var quoteCode =
                    $"WL-{DateTime.UtcNow:yyyyMMddHHmmss}-" +
                    $"{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";

                var quote = new SupabaseQuoteRequest
                {
                    QuoteCode = quoteCode,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Branch = request.Branch,
                    Service = request.Service,
                    Message = request.Message,
                    ProductId = request.ProductId ?? "",
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                };

                await _supabase
                    .From<SupabaseQuoteRequest>()
                    .Insert(quote);

                TempData["QuoteCode"] = quoteCode;

                return RedirectToAction(nameof(Confirmation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quote request");

                ModelState.AddModelError(
                    "",
                    "There was a problem submitting your quote request. Please try again.");

                return View(request);
            }
        }

        public IActionResult Confirmation()
        {
            return View();
        }
    }
}