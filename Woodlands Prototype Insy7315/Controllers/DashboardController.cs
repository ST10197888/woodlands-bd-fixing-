using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using SupabaseClient = Supabase.Client;
using Woodlands_Prototype_Insy7315.Data;
using Woodlands_Prototype_Insy7315.Models;

namespace Woodlands_Prototype_Insy7315.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly SupabaseClient _supabase;

        public DashboardController(SupabaseClient supabase)
        {
            _supabase = supabase;
        }

        public async Task<IActionResult> Index()
        {
            var isAdmin = User.IsInRole("Admin");
            var isManager = User.IsInRole("Manager (Soweto)") ||
                            User.IsInRole("Manager (Roodepoort)") ||
                            User.IsInRole("Manager (Randfontein)");

            var displayName = User.FindFirst("FullName")?.Value
                              ?? User.Identity?.Name
                              ?? "User";

            var managerBranch = User.FindFirst("Branch")?.Value;

            var response = await _supabase
                .From<SupabaseQuoteRequest>()
                .Get();

            var allQuotes = response.Models
                .OrderByDescending(q => q.CreatedAt)
                .ToList();

            var viewModel = new DashboardOverviewViewModel
            {
                IsAdmin = isAdmin,
                IsManager = isManager,
                ManagerBranch = managerBranch,
                DisplayName = displayName
            };

            if (isAdmin)
            {
                BuildAdminDashboard(viewModel, allQuotes);
            }
            else if (isManager)
            {
                BuildManagerDashboard(viewModel, allQuotes, managerBranch);
            }
            else
            {
                BuildCustomerDashboard(viewModel, allQuotes, UserEmail());
            }

            return View(viewModel);
        }

        private static void BuildAdminDashboard(
            DashboardOverviewViewModel model,
            List<SupabaseQuoteRequest> quotes)
        {
            model.TotalQuotes = quotes.Count;

            model.TotalCustomers = quotes
                .Select(q => q.Email)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            model.CompletedJobs = quotes.Count(q =>
                string.Equals(q.Status, "Completed", StringComparison.OrdinalIgnoreCase));

            model.CompletionRate = model.TotalQuotes == 0
                ? 0
                : Math.Round(
                    (double)model.CompletedJobs / model.TotalQuotes * 100,
                    1);

            var completedQuotes = quotes
                .Where(q =>
                    string.Equals(q.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var totalRevenue = completedQuotes
                .Sum(GetQuoteValue);

            model.MonthlyRevenue = FormatCurrency(totalRevenue);

            model.Branches = BuildBranchSummaries(quotes);

            model.RecentQuotes = quotes
                .Take(5)
                .Select(MapQuote)
                .ToList();
        }

        private static void BuildManagerDashboard(
            DashboardOverviewViewModel model,
            List<SupabaseQuoteRequest> allQuotes,
            string? managerBranch)
        {
            var branchQuotes = allQuotes
                .Where(q =>
                    !string.IsNullOrWhiteSpace(managerBranch) &&
                    string.Equals(
                        q.Branch,
                        managerBranch,
                        StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(q => q.CreatedAt)
                .ToList();

            model.PendingCount = branchQuotes.Count(q =>
                string.Equals(q.Status, "Pending", StringComparison.OrdinalIgnoreCase));

            model.InProgressCount = branchQuotes.Count(q =>
                string.Equals(q.Status, "In Progress", StringComparison.OrdinalIgnoreCase));

            model.CompletedCount = branchQuotes.Count(q =>
                string.Equals(q.Status, "Completed", StringComparison.OrdinalIgnoreCase));

            var completedRevenue = branchQuotes
                .Where(q =>
                    string.Equals(q.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                .Sum(GetQuoteValue);

            model.WeekRevenue = FormatCurrency(completedRevenue);

            model.WeeklyRevenueDays = BuildWeeklyRevenue(branchQuotes);

            model.QuotesNeedingAttention = branchQuotes
                .Where(q =>
                    string.Equals(q.Status, "Pending", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(q.Status, "In Progress", StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .Select(MapQuote)
                .ToList();
        }

        private static void BuildCustomerDashboard(
        DashboardOverviewViewModel model,
        List<SupabaseQuoteRequest> allQuotes,
        string? email)
        {

            var customerQuotes = allQuotes
                .Where(q =>
                    !string.IsNullOrWhiteSpace(email) &&
                    string.Equals(
                        q.Email,
                        email,
                        StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(q => q.CreatedAt)
                .ToList();

            model.PendingCount = customerQuotes.Count(q =>
                string.Equals(q.Status, "Pending", StringComparison.OrdinalIgnoreCase));

            model.InProgressCount = customerQuotes.Count(q =>
                string.Equals(q.Status, "In Progress", StringComparison.OrdinalIgnoreCase));

            model.CompletedCount = customerQuotes.Count(q =>
                string.Equals(q.Status, "Completed", StringComparison.OrdinalIgnoreCase));

            model.RecentQuotes = customerQuotes
                .Take(10)
                .Select(MapQuote)
                .ToList();
        }

        private static List<BranchSummary> BuildBranchSummaries(
            List<SupabaseQuoteRequest> quotes)
        {
            var branches = new[]
            {
                "Soweto",
                "Roodepoort",
                "Randfontein"
            };

            return branches
                .Select(branch =>
                {
                    var branchQuotes = quotes
                        .Where(q =>
                            string.Equals(
                                q.Branch,
                                branch,
                                StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    var completed = branchQuotes
                        .Where(q =>
                            string.Equals(
                                q.Status,
                                "Completed",
                                StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    var revenue = completed.Sum(GetQuoteValue);

                    var donePercent = branchQuotes.Count == 0
                        ? 0
                        : (int)Math.Round(
                            (double)completed.Count / branchQuotes.Count * 100);

                    return new BranchSummary
                    {
                        Name = branch,
                        Quotes = branchQuotes.Count,
                        Revenue = FormatShortCurrency(revenue),
                        DonePercent = donePercent
                    };
                })
                .ToList();
        }

        private static List<int> BuildWeeklyRevenue(
            List<SupabaseQuoteRequest> quotes)
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(
                -(int)today.DayOfWeek + 1);

            var result = new List<int>();

            for (var i = 0; i < 6; i++)
            {
                var day = startOfWeek.AddDays(i);

                var revenue = quotes
                    .Where(q =>
                        q.CreatedAt.Date == day.Date &&
                        string.Equals(
                            q.Status,
                            "Completed",
                            StringComparison.OrdinalIgnoreCase))
                    .Sum(GetQuoteValue);

                result.Add((int)Math.Round(revenue));
            }

            return result;
        }

        private static MockQuote MapQuote(SupabaseQuoteRequest quote)
        {
            return new MockQuote
            {
                Id = string.IsNullOrWhiteSpace(quote.QuoteCode)
                    ? quote.Id.ToString()
                    : quote.QuoteCode,

                Customer = $"{quote.FirstName} {quote.LastName}".Trim(),

                Phone = quote.Phone ?? "",

                Service = quote.Service ?? "",

                Branch = quote.Branch ?? "",

                Date = quote.CreatedAt.ToString("yyyy-MM-dd"),

                Value = string.IsNullOrWhiteSpace(quote.Value)
                    ? ""
                    : quote.Value,

                Status = string.IsNullOrWhiteSpace(quote.Status)
                    ? "Pending"
                    : quote.Status
            };
        }

        private static double GetQuoteValue(SupabaseQuoteRequest quote)
        {
            if (string.IsNullOrWhiteSpace(quote.Value))
                return 0;

            var cleaned = quote.Value
                .Replace("R", "", StringComparison.OrdinalIgnoreCase)
                .Replace(",", "")
                .Replace(" ", "")
                .Trim();

            return double.TryParse(
                cleaned,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var value)
                ? value
                : 0;
        }

        private static string FormatCurrency(double value)
        {
            return $"R{value:N0}";
        }

        private static string FormatShortCurrency(double value)
        {
            if (value >= 1_000_000)
                return $"R{value / 1_000_000:0.#}m";

            if (value >= 1_000)
                return $"R{value / 1_000:0.#}k";

            return $"R{value:N0}";
        }

        private string? UserEmail()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                   ?? User.Identity?.Name;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuoteStatus(
            string id,
            string status)
        {
            if (!User.IsInRole("Admin") &&
                !User.IsInRole("Manager (Soweto)") &&
                !User.IsInRole("Manager (Roodepoort)") &&
                !User.IsInRole("Manager (Randfontein)"))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(status))
            {
                return BadRequest();
            }

            var response = await _supabase
                .From<SupabaseQuoteRequest>()
                .Where(q => q.QuoteCode == id)
                .Single();

            if (response == null)
            {
                if (long.TryParse(id, out var numericId))
                {
                    response = await _supabase
                        .From<SupabaseQuoteRequest>()
                        .Where(q => q.Id == numericId)
                        .Single();
                }
            }

            if (response == null)
                return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var managerBranch = User.FindFirst("Branch")?.Value;

                if (!string.Equals(
                        response.Branch,
                        managerBranch,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }
            }

            response.Status = status;

            await _supabase
                .From<SupabaseQuoteRequest>()
                .Update(response);

            return RedirectToAction(nameof(Index));
        }
    }
}