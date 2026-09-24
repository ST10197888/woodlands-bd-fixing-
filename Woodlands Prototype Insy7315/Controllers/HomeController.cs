using Microsoft.AspNetCore.Mvc;
using Woodlands_Prototype_Insy7315.Data;
using Woodlands_Prototype_Insy7315.Models;
using PostgrestConstants = Supabase.Postgrest.Constants;
using SupabaseClient = Supabase.Client;

namespace Woodlands_Prototype_Insy7315.Controllers
{
    public class HomeController : Controller
    {
        private readonly SupabaseClient _supabase;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            SupabaseClient supabase,
            ILogger<HomeController> logger)
        {
            _supabase = supabase;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var productsResponse = await _supabase
                    .From<SupabaseProduct>()
                    .Select("*")
                    .Order("title", PostgrestConstants.Ordering.Ascending)
                    .Get();

                var testimonialsResponse = await _supabase
                    .From<SupabaseTestimonial>()
                    .Select("*")
                    .Order("id", PostgrestConstants.Ordering.Ascending)
                    .Get();

                var products = productsResponse.Models
                    .Select(p => new Product
                    {
                        Id = p.Id,
                        Category = p.Category,
                        Title = p.Title,
                        Tagline = p.Tagline,
                        Description = p.Description,
                        Image = p.Image,
                        GalleryJson = p.Gallery ?? "[]",
                        FeaturesJson = p.Features ?? "[]",
                        FinishesJson = p.Finishes ?? "[]",
                        LeadTime = p.LeadTime,
                        Tag = p.Tag,
                        Price = p.Price
                    })
                    .ToList();

                var testimonials = testimonialsResponse.Models
                    .Select(t => new Testimonial
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Role = t.Role,
                        Location = t.Location,
                        Rating = t.Rating,
                        Review = t.Review,
                        Project = t.Project
                    })
                    .ToList();

                var vm = new HomeViewModel
                {
                    HeroSlides = WoodLinkData.HeroSlides,
                    Categories = WoodLinkData.Categories,

                    FeaturedProducts = products
                        .Where(p => p.Tag == "Popular" || p.Tag == "New")
                        .Take(4)
                        .ToList(),

                    TestimonialsSnippet = testimonials
                        .Take(3)
                        .ToList()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading homepage data from Supabase");

                var vm = new HomeViewModel
                {
                    HeroSlides = WoodLinkData.HeroSlides,
                    Categories = WoodLinkData.Categories,
                    FeaturedProducts = new List<Product>(),
                    TestimonialsSnippet = new List<Testimonial>()
                };

                TempData["Error"] = "Some homepage content could not be loaded.";

                return View(vm);
            }
        }

        public IActionResult About()
        {
            return View();
        }
    }
}