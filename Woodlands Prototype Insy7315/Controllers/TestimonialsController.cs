using Microsoft.AspNetCore.Mvc;
using PostgrestConstants = Supabase.Postgrest.Constants;
using SupabaseClient = Supabase.Client;
using Woodlands_Prototype_Insy7315.Models;

namespace Woodlands_Prototype_Insy7315.Controllers
{
    public class TestimonialsController : Controller
    {
        private readonly SupabaseClient _supabase;
        private readonly ILogger<TestimonialsController> _logger;

        public TestimonialsController(
            SupabaseClient supabase,
            ILogger<TestimonialsController> logger)
        {
            _supabase = supabase;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _supabase
                    .From<SupabaseTestimonial>()
                    .Select("*")
                    .Order("id", PostgrestConstants.Ordering.Descending)
                    .Get();

                var testimonials = response.Models
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

                return View(testimonials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading testimonials from Supabase");

                TempData["Error"] = "Unable to load testimonials.";

                return View(new List<Testimonial>());
            }
        }
    }
}