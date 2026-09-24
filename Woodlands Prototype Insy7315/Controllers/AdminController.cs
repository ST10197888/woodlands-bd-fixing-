using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostgrestConstants = Supabase.Postgrest.Constants;
using SupabaseClient = Supabase.Client;
using Woodlands_Prototype_Insy7315.Models;

namespace Woodlands_Prototype_Insy7315.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly SupabaseClient _supabase;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            SupabaseClient supabase,
            ILogger<AdminController> logger)
        {
            _supabase = supabase;
            _logger = logger;
        }

        // Users

        public async Task<IActionResult> Users()
        {
            try
            {
                var response = await _supabase
                    .From<SupabaseAppUser>()
                    .Select("*")
                    .Order(
                        "full_name",
                        PostgrestConstants.Ordering.Ascending)
                    .Get();

                var rows = response.Models
                    .Select(user => new AdminUserRowViewModel
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.Phone,
                        Role = string.IsNullOrWhiteSpace(user.Role)
                            ? "Customer"
                            : user.Role,
                        Branch = user.Branch,
                        Active = user.Active
                    })
                    .ToList();

                return View(rows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Supabase users");

                TempData["AdminError"] =
                    "Unable to load user accounts.";

                return View(new List<AdminUserRowViewModel>());
            }
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View(
                "UserForm",
                new UserFormViewModel
                {
                    Role = "Customer",
                    Active = true
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(
            UserFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(
                    nameof(model.Password),
                    "A password is required when creating a user.");
            }

            if (!IdentitySeederRoles.All.Contains(model.Role))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Invalid role.");
            }

            if (!ModelState.IsValid)
            {
                return View("UserForm", model);
            }

            try
            {
                var session = await _supabase.Auth.SignUp(
                    email: model.Email,
                    password: model.Password!);

                if (session?.User == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Unable to create the Supabase account.");

                    return View("UserForm", model);
                }

                var appUser = new SupabaseAppUser
                {
                    Id = session!.User!.Id.ToString(),
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.PhoneNumber ?? "",
                    Role = model.Role,
                    Branch = string.IsNullOrWhiteSpace(model.Branch)
                        ? null
                        : model.Branch,
                    Active = model.Active,
                    CreatedAt = DateTime.UtcNow
                };

                await _supabase
                    .From<SupabaseAppUser>()
                    .Insert(appUser);

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating Supabase user");

                ModelState.AddModelError(
                    "",
                    GetUserFriendlyError(ex));

                return View("UserForm", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _supabase
                    .From<SupabaseAppUser>()
                    .Where(u => u.Id == id)
                    .Single();

                if (user == null)
                {
                    return NotFound();
                }

                return View(
                    "UserForm",
                    new UserFormViewModel
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.Phone,
                        Role = string.IsNullOrWhiteSpace(user.Role)
                            ? "Customer"
                            : user.Role,
                        Branch = user.Branch,
                        Active = user.Active
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading Supabase user {UserId}",
                    id);

                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(
            string id,
            UserFormViewModel model)
        {
            if (!IdentitySeederRoles.All.Contains(model.Role))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Invalid role.");
            }

            if (!ModelState.IsValid)
            {
                model.Id = id;
                return View("UserForm", model);
            }

            try
            {
                var user = await _supabase
                    .From<SupabaseAppUser>()
                    .Where(u => u.Id == id)
                    .Single();

                if (user == null)
                {
                    return NotFound();
                }

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.Phone = model.PhoneNumber ?? "";
                user.Role = model.Role;
                user.Branch = string.IsNullOrWhiteSpace(model.Branch)
                    ? null
                    : model.Branch;
                user.Active = model.Active;

                await _supabase
                    .From<SupabaseAppUser>()
                    .Update(user);

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating Supabase user {UserId}",
                    id);

                ModelState.AddModelError(
                    "",
                    "Unable to update the user account.");

                model.Id = id;

                return View("UserForm", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(nameof(Users));
            }

            var currentUserId =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)
                ?.Value;

            if (id == currentUserId)
            {
                TempData["AdminError"] =
                    "You cannot delete the account you are currently using.";

                return RedirectToAction(nameof(Users));
            }

            try
            {
                var user = await _supabase
                    .From<SupabaseAppUser>()
                    .Where(u => u.Id == id)
                    .Single();

                if (user != null)
                {
                    await _supabase
                        .From<SupabaseAppUser>()
                        .Delete(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting Supabase user {UserId}",
                    id);

                TempData["AdminError"] =
                    "Unable to delete the user account.";
            }

            return RedirectToAction(nameof(Users));
        }

        // Testimonials

        public async Task<IActionResult> Testimonials()
        {
            try
            {
                var response = await _supabase
                    .From<SupabaseTestimonial>()
                    .Select("*")
                    .Order(
                        "id",
                        PostgrestConstants.Ordering.Descending)
                    .Get();

                var testimonials = response.Models
                    .Select(ToTestimonial)
                    .ToList();

                return View(testimonials);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading admin testimonials");

                TempData["AdminError"] =
                    "Unable to load testimonials.";

                return View(new List<Testimonial>());
            }
        }

        [HttpGet]
        public IActionResult CreateTestimonial()
        {
            return View(
                "TestimonialForm",
                new TestimonialFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTestimonial(
            TestimonialFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("TestimonialForm", model);
            }

            try
            {
                var testimonial = new SupabaseTestimonial
                {
                    Name = model.Name,
                    Role = model.Role,
                    Location = model.Location,
                    Rating = model.Rating,
                    Review = model.Review,
                    Project = model.Project
                };

                await _supabase
                    .From<SupabaseTestimonial>()
                    .Insert(testimonial);

                return RedirectToAction(nameof(Testimonials));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating testimonial");

                ModelState.AddModelError(
                    "",
                    "Unable to create the testimonial.");

                return View("TestimonialForm", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditTestimonial(int id)
        {
            try
            {
                var response = await _supabase
                    .From<SupabaseTestimonial>()
                    .Where(t => t.Id == id)
                    .Single();

                if (response == null)
                {
                    return NotFound();
                }

                return View(
                    "TestimonialForm",
                    new TestimonialFormViewModel
                    {
                        Id = response.Id,
                        Name = response.Name,
                        Role = response.Role,
                        Location = response.Location,
                        Rating = response.Rating,
                        Review = response.Review,
                        Project = response.Project
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading testimonial {TestimonialId}",
                    id);

                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTestimonial(
            int id,
            TestimonialFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Id = id;
                return View("TestimonialForm", model);
            }

            try
            {
                var testimonial = await _supabase
                    .From<SupabaseTestimonial>()
                    .Where(t => t.Id == id)
                    .Single();

                if (testimonial == null)
                {
                    return NotFound();
                }

                testimonial.Name = model.Name;
                testimonial.Role = model.Role;
                testimonial.Location = model.Location;
                testimonial.Rating = model.Rating;
                testimonial.Review = model.Review;
                testimonial.Project = model.Project;

                await _supabase
                    .From<SupabaseTestimonial>()
                    .Update(testimonial);

                return RedirectToAction(nameof(Testimonials));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating testimonial {TestimonialId}",
                    id);

                ModelState.AddModelError(
                    "",
                    "Unable to update the testimonial.");

                model.Id = id;

                return View("TestimonialForm", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTestimonial(int id)
        {
            try
            {
                var testimonial = await _supabase
                    .From<SupabaseTestimonial>()
                    .Where(t => t.Id == id)
                    .Single();

                if (testimonial != null)
                {
                    await _supabase
                        .From<SupabaseTestimonial>()
                        .Delete(testimonial);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting testimonial {TestimonialId}",
                    id);
            }

            return RedirectToAction(nameof(Testimonials));
        }

        // FAQs

        public async Task<IActionResult> Faqs()
        {
            try
            {
                var response = await _supabase
                    .From<SupabaseFaqItem>()
                    .Select("*")
                    .Order(
                        "category",
                        PostgrestConstants.Ordering.Ascending)
                    .Order(
                        "id",
                        PostgrestConstants.Ordering.Ascending)
                    .Get();

                var faqs = response.Models
                    .Select(ToFaq)
                    .ToList();

                return View(faqs);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading admin FAQs");

                TempData["AdminError"] =
                    "Unable to load FAQs.";

                return View(new List<FaqItem>());
            }
        }

        [HttpGet]
        public IActionResult CreateFaq()
        {
            return View(
                "FaqForm",
                new FaqFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFaq(
            FaqFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("FaqForm", model);
            }

            try
            {
                var faq = new SupabaseFaqItem
                {
                    Category = model.Category,
                    Question = model.Question,
                    Answer = model.Answer
                };

                await _supabase
                    .From<SupabaseFaqItem>()
                    .Insert(faq);

                return RedirectToAction(nameof(Faqs));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating FAQ");

                ModelState.AddModelError(
                    "",
                    "Unable to create the FAQ.");

                return View("FaqForm", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditFaq(int id)
        {
            try
            {
                var faq = await _supabase
                    .From<SupabaseFaqItem>()
                    .Where(f => f.Id == id)
                    .Single();

                if (faq == null)
                {
                    return NotFound();
                }

                return View(
                    "FaqForm",
                    new FaqFormViewModel
                    {
                        Id = faq.Id,
                        Category = faq.Category,
                        Question = faq.Question,
                        Answer = faq.Answer
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading FAQ {FaqId}",
                    id);

                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFaq(
            int id,
            FaqFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Id = id;
                return View("FaqForm", model);
            }

            try
            {
                var faq = await _supabase
                    .From<SupabaseFaqItem>()
                    .Where(f => f.Id == id)
                    .Single();

                if (faq == null)
                {
                    return NotFound();
                }

                faq.Category = model.Category;
                faq.Question = model.Question;
                faq.Answer = model.Answer;

                await _supabase
                    .From<SupabaseFaqItem>()
                    .Update(faq);

                return RedirectToAction(nameof(Faqs));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating FAQ {FaqId}",
                    id);

                ModelState.AddModelError(
                    "",
                    "Unable to update the FAQ.");

                model.Id = id;

                return View("FaqForm", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFaq(int id)
        {
            try
            {
                var faq = await _supabase
                    .From<SupabaseFaqItem>()
                    .Where(f => f.Id == id)
                    .Single();

                if (faq != null)
                {
                    await _supabase
                        .From<SupabaseFaqItem>()
                        .Delete(faq);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting FAQ {FaqId}",
                    id);
            }

            return RedirectToAction(nameof(Faqs));
        }

        private static Testimonial ToTestimonial(
            SupabaseTestimonial testimonial)
        {
            return new Testimonial
            {
                Id = testimonial.Id,
                Name = testimonial.Name,
                Role = testimonial.Role,
                Location = testimonial.Location,
                Rating = testimonial.Rating,
                Review = testimonial.Review,
                Project = testimonial.Project
            };
        }

        private static FaqItem ToFaq(
            SupabaseFaqItem faq)
        {
            return new FaqItem
            {
                Id = faq.Id,
                Category = faq.Category,
                Question = faq.Question,
                Answer = faq.Answer
            };
        }

        private static string GetUserFriendlyError(Exception ex)
        {
            var message = ex.Message.ToLowerInvariant();

            if (message.Contains("already registered") ||
                message.Contains("already exists"))
            {
                return "An account with this email address already exists.";
            }

            if (message.Contains("password"))
            {
                return "The password does not meet the required requirements.";
            }

            if (message.Contains("email"))
            {
                return "Please enter a valid email address.";
            }

            return "The user account could not be created.";
        }
    }

    public class AdminUserRowViewModel
    {
        public string Id { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "";
        public string? Branch { get; set; }
        public bool Active { get; set; }
    }
}