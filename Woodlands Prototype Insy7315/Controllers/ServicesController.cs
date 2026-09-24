using Microsoft.AspNetCore.Mvc;
using PostgrestConstants = Supabase.Postgrest.Constants;
using SupabaseClient = Supabase.Client;
using Woodlands_Prototype_Insy7315.Models;

namespace Woodlands_Prototype_Insy7315.Controllers
{
    public class ServicesController : Controller
    {
        private readonly SupabaseClient _supabase;
        private readonly ILogger<ServicesController> _logger;

        public ServicesController(
            SupabaseClient supabase,
            ILogger<ServicesController> logger)
        {
            _supabase = supabase;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _supabase
                    .From<SupabaseService>()
                    .Where(s => s.IsActive)
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
                _logger.LogError(ex, "Error loading services from Supabase");

                TempData["Error"] = "Unable to load services.";
                return View(new List<Service>());
            }
        }

        public async Task<IActionResult> Details(int id)
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

                var service = new Service
                {
                    Id = response.Id,
                    Name = response.Name,
                    Description = response.Description,
                    Image = response.Image,
                    IsActive = response.IsActive
                };

                return View(service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service {ServiceId}", id);

                return NotFound();
            }
        }
    }
}