using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Woodlands_Prototype_Insy7315.Models;

namespace Woodlands_Prototype_Insy7315.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<AdminController> _logger;
        private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

        public AdminController(IHttpClientFactory http, ILogger<AdminController> logger)
        {
            _http = http;
            _logger = logger;
        }

        // ==================== USERS ====================

        public async Task<IActionResult> Users()
        {
            var rows = new List<AdminUserRowViewModel>();
            try
            {
                var client = _http.CreateClient("NodeApi");
                var res = await client.GetAsync("api/app-users");
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<List<AppUser>>(json, _json) ?? new();

                    rows = users.Select(u => new AdminUserRowViewModel
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        PhoneNumber = u.Phone,
                        Role = string.IsNullOrWhiteSpace(u.Role) ? "Customer" : u.Role,
                        Branch = u.Branch,
                        Active = u.Active
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["AdminError"] = "Unable to load user accounts.";
            }
            return View(rows);
        }

        [HttpGet]
        public IActionResult CreateUser() => View("UserForm", new UserFormViewModel { Role = "Customer", Active = true });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(model.Password), "A password is required when creating a user.");

            if (!IdentitySeederRoles.All.Contains(model.Role))
                ModelState.AddModelError(nameof(model.Role), "Invalid role.");

            if (!ModelState.IsValid) return View("UserForm", model);

            try
            {
                // Register via Node API
                var registerPayload = new
                {
                    fullName = model.FullName,
                    email = model.Email,
                    password = model.Password,
                    phone = model.PhoneNumber ?? ""
                };

                var client = _http.CreateClient("NodeApi");
                var content = new StringContent(JsonSerializer.Serialize(registerPayload), Encoding.UTF8, "application/json");
                var res = await client.PostAsync("api/auth/register", content);

                if (!res.IsSuccessStatusCode)
                {
                    var errJson = await res.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", GetUserFriendlyError(errJson));
                    return View("UserForm", model);
                }

                // If role/branch differs from Customer, update the app_users row via a second call
                if (model.Role != "Customer" || !model.Active || !string.IsNullOrWhiteSpace(model.Branch))
                {
                    // Find the newly-created user
                    var listRes = await client.GetAsync("api/app-users");
                    if (listRes.IsSuccessStatusCode)
                    {
                        var listJson = await listRes.Content.ReadAsStringAsync();
                        var users = JsonSerializer.Deserialize<List<AppUser>>(listJson, _json) ?? new();
                        var created = users.FirstOrDefault(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase));

                        if (created != null)
                        {
                            var updatePayload = new
                            {
                                role = model.Role,
                                branch = model.Branch,
                                active = model.Active
                            };
                            var upContent = new StringContent(JsonSerializer.Serialize(updatePayload), Encoding.UTF8, "application/json");
                            await client.PutAsync($"api/app-users/{created.Id}", upContent);
                        }
                    }
                }

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                ModelState.AddModelError("", "The user account could not be created.");
                return View("UserForm", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            try
            {
                var client = _http.CreateClient("NodeApi");
                var res = await client.GetAsync("api/app-users");
                if (!res.IsSuccessStatusCode) return NotFound();

                var json = await res.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<AppUser>>(json, _json) ?? new();
                var user = users.FirstOrDefault(u => u.Id == id);
                if (user == null) return NotFound();

                return View("UserForm", new UserFormViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.Phone,
                    Role = string.IsNullOrWhiteSpace(user.Role) ? "Customer" : user.Role,
                    Branch = user.Branch,
                    Active = user.Active
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user {Id}", id);
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, UserFormViewModel model)
        {
            if (!IdentitySeederRoles.All.Contains(model.Role))
                ModelState.AddModelError(nameof(model.Role), "Invalid role.");

            if (!ModelState.IsValid) { model.Id = id; return View("UserForm", model); }

            try
            {
                var payload = new
                {
                    full_name = model.FullName,
                    email = model.Email,
                    phone = model.PhoneNumber ?? "",
                    role = model.Role,
                    branch = model.Branch,
                    active = model.Active
                };

                var client = _http.CreateClient("NodeApi");
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                await client.PutAsync($"api/app-users/{id}", content);

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Id}", id);
                ModelState.AddModelError("", "Unable to update the user account.");
                model.Id = id;
                return View("UserForm", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return RedirectToAction(nameof(Users));

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (id == currentUserId)
            {
                TempData["AdminError"] = "You cannot delete the account you are currently using.";
                return RedirectToAction(nameof(Users));
            }

            try
            {
                var client = _http.CreateClient("NodeApi");
                await client.DeleteAsync($"api/app-users/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {Id}", id);
                TempData["AdminError"] = "Unable to delete the user account.";
            }

            return RedirectToAction(nameof(Users));
        }

        // ==================== TESTIMONIALS ====================

        public async Task<IActionResult> Testimonials()
        {
            var testimonials = new List<Testimonial>();
            try
            {
                var client = _http.CreateClient("NodeApi");
                var res = await client.GetAsync("api/testimonials");
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    testimonials = JsonSerializer.Deserialize<List<Testimonial>>(json, _json) ?? new();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading testimonials");
                TempData["AdminError"] = "Unable to load testimonials.";
            }
            return View(testimonials);
        }

        [HttpGet]
        public IActionResult CreateTestimonial() => View("TestimonialForm", new TestimonialFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTestimonial(TestimonialFormViewModel model)
        {
            if (!ModelState.IsValid) return View("TestimonialForm", model);

            try
            {
                var payload = new
                {
                    name = model.Name,
                    role = model.Role,
                    location = model.Location,
                    rating = model.Rating,
                    review = model.Review,
                    project = model.Project
                };

                var client = _http.CreateClient("NodeApi");
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                await client.PostAsync("api/testimonials", content);

                return RedirectToAction(nameof(Testimonials));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating testimonial");
                ModelState.AddModelError("", "Unable to create the testimonial.");
                return View("TestimonialForm", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditTestimonial(int id)
        {
            try
            {
                var client = _http.CreateClient("NodeApi");
                var res = await client.GetAsync("api/testimonials");
                if (!res.IsSuccessStatusCode) return NotFound();

                var json = await res.Content.ReadAsStringAsync();
                var list = JsonSerializer.Deserialize<List<Testimonial>>(json, _json) ?? new();
                var t = list.FirstOrDefault(x => x.Id == id);
                if (t == null) return NotFound();

                return View("TestimonialForm", new TestimonialFormViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Role = t.Role,
                    Location = t.Location,
                    Rating = t.Rating,
                    Review = t.Review,
                    Project = t.Project
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading testimonial {Id}", id);
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTestimonial(int id, TestimonialFormViewModel model)
        {
            if (!ModelState.IsValid) { model.Id = id; return View("TestimonialForm", model); }

            try
            {
                var payload = new
                {
                    name = model.Name,
                    role = model.Role,
                    location = model.Location,
                    rating = model.Rating,
                    review = model.Review,
                    project = model.Project
                };

                var client = _http.CreateClient("NodeApi");
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                await client.PutAsync($"api/testimonials/{id}", content);

                return RedirectToAction(nameof(Testimonials));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating testimonial {Id}", id);
                ModelState.AddModelError("", "Unable to update the testimonial.");
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
                var client = _http.CreateClient("NodeApi");
                await client.DeleteAsync($"api/testimonials/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting testimonial {Id}", id);
            }
            return RedirectToAction(nameof(Testimonials));
        }

        // ==================== FAQS ====================

        public async Task<IActionResult> Faqs()
        {
            var faqs = new List<FaqItem>();
            try
            {
                var client = _http.CreateClient("NodeApi");
                var res = await client.GetAsync("api/faqs");
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    faqs = JsonSerializer.Deserialize<List<FaqItem>>(json, _json) ?? new();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading FAQs");
                TempData["AdminError"] = "Unable to load FAQs.";
            }
            return View(faqs);
        }

        [HttpGet]
        public IActionResult CreateFaq() => View("FaqForm", new FaqFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFaq(FaqFormViewModel model)
        {
            if (!ModelState.IsValid) return View("FaqForm", model);

            try
            {
                var payload = new { category = model.Category, question = model.Question, answer = model.Answer };

                var client = _http.CreateClient("NodeApi");
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                await client.PostAsync("api/faqs", content);

                return RedirectToAction(nameof(Faqs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating FAQ");
                ModelState.AddModelError("", "Unable to create the FAQ.");
                return View("FaqForm", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditFaq(int id)
        {
            try
            {
                var client = _http.CreateClient("NodeApi");
                var res = await client.GetAsync("api/faqs");
                if (!res.IsSuccessStatusCode) return NotFound();

                var json = await res.Content.ReadAsStringAsync();
                var list = JsonSerializer.Deserialize<List<FaqItem>>(json, _json) ?? new();
                var f = list.FirstOrDefault(x => x.Id == id);
                if (f == null) return NotFound();

                return View("FaqForm", new FaqFormViewModel
                {
                    Id = f.Id,
                    Category = f.Category,
                    Question = f.Question,
                    Answer = f.Answer
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading FAQ {Id}", id);
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFaq(int id, FaqFormViewModel model)
        {
            if (!ModelState.IsValid) { model.Id = id; return View("FaqForm", model); }

            try
            {
                var payload = new { category = model.Category, question = model.Question, answer = model.Answer };

                var client = _http.CreateClient("NodeApi");
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                await client.PutAsync($"api/faqs/{id}", content);

                return RedirectToAction(nameof(Faqs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating FAQ {Id}", id);
                ModelState.AddModelError("", "Unable to update the FAQ.");
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
                var client = _http.CreateClient("NodeApi");
                await client.DeleteAsync($"api/faqs/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting FAQ {Id}", id);
            }
            return RedirectToAction(nameof(Faqs));
        }

        private static string GetUserFriendlyError(string errorJson)
        {
            try
            {
                var err = JsonSerializer.Deserialize<ErrorResponse>(errorJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var message = (err?.Error ?? "").ToLowerInvariant();

                if (message.Contains("already registered") || message.Contains("already exists"))
                    return "An account with this email address already exists.";
                if (message.Contains("password"))
                    return "The password does not meet the required requirements.";
                if (message.Contains("email"))
                    return "Please enter a valid email address.";
            }
            catch { }
            return "The user account could not be created.";
        }

        private class ErrorResponse { public string? Error { get; set; } }
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