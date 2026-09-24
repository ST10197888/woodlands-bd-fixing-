using Microsoft.AspNetCore.Mvc;
using PostgrestConstants = Supabase.Postgrest.Constants;
using SupabaseClient = Supabase.Client;
using Woodlands_Prototype_Insy7315.Models;

namespace Woodlands_Prototype_Insy7315.Controllers
{
    public class FAQsController : Controller
    {
        private readonly SupabaseClient _supabase;
        private readonly ILogger<FAQsController> _logger;

        public FAQsController(
            SupabaseClient supabase,
            ILogger<FAQsController> logger)
        {
            _supabase = supabase;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _supabase
                    .From<SupabaseFaqItem>()
                    .Select("*")
                    .Order("category", PostgrestConstants.Ordering.Ascending)
                    .Order("id", PostgrestConstants.Ordering.Ascending)
                    .Get();

                var faqs = response.Models
                    .Select(f => new FaqItem
                    {
                        Id = f.Id,
                        Category = f.Category,
                        Question = f.Question,
                        Answer = f.Answer
                    })
                    .ToList();

                return View(faqs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading FAQs from Supabase");

                TempData["Error"] = "Unable to load FAQs.";

                return View(new List<FaqItem>());
            }
        }
    }
}