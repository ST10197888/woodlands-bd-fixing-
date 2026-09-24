using Woodlands_Prototype_Insy7315.Models;

namespace Woodlands_Prototype_Insy7315.Data
{
    public class MockQuote
    {
        public string Id { get; set; } = "";
        public string Customer { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Service { get; set; } = "";
        public string Branch { get; set; } = "";
        public string Date { get; set; } = "";
        public string Value { get; set; } = "";
        public string Status { get; set; } = ""; // Pending, In Progress, Completed, Cancelled
    }

    // Placeholder quote-request data powering the dashboard UI. Not persisted to
    // the database — resets whenever the app restarts. Swap this out once quote
    // requests submitted via Contact/Index are actually saved (see ContactController).
    public static class DashboardMockData
    {
        public static List<MockQuote> Quotes { get; } = new()
        {
            new MockQuote { Id = "QT-0041", Customer = "Thabo Mokoena",  Phone = "071 234 5678", Service = "Kitchen Units",      Branch = "Soweto",      Date = "2026-07-28", Value = "R18 500", Status = "Completed" },
            new MockQuote { Id = "QT-0040", Customer = "Sandra Van Wyk", Phone = "082 555 1234", Service = "Built-In Cupboards", Branch = "Roodepoort",  Date = "2026-07-26", Value = "R9 200",  Status = "In Progress" },
            new MockQuote { Id = "QT-0039", Customer = "Mpho Dlamini",   Phone = "083 456 7890", Service = "TV Stand",           Branch = "Soweto",      Date = "2026-07-25", Value = "R4 800",  Status = "Pending" },
            new MockQuote { Id = "QT-0038", Customer = "Anita Joubert",  Phone = "084 222 9911", Service = "Cutting & Edging",   Branch = "Randfontein", Date = "2026-07-22", Value = "R1 650",  Status = "Completed" },
            new MockQuote { Id = "QT-0037", Customer = "Lebo Sithole",   Phone = "072 333 4455", Service = "Kitchen Units",      Branch = "Randfontein", Date = "2026-07-20", Value = "R22 000", Status = "Cancelled" },
            new MockQuote { Id = "QT-0036", Customer = "Priya Naidoo",   Phone = "073 888 2200", Service = "Built-In Cupboards", Branch = "Roodepoort",  Date = "2026-07-18", Value = "R13 400", Status = "In Progress" },
            new MockQuote { Id = "QT-0035", Customer = "Kagiso Sithole", Phone = "082 111 2222", Service = "TV Stand",           Branch = "Soweto",      Date = "2026-07-16", Value = "R6 100",  Status = "Pending" },
            new MockQuote { Id = "QT-0032", Customer = "Nomsa Dube",     Phone = "071 999 8888", Service = "Kitchen Units",      Branch = "Soweto",      Date = "2026-07-10", Value = "R22 400", Status = "In Progress" },
            new MockQuote { Id = "QT-0029", Customer = "Bongani Zulu",   Phone = "060 333 4444", Service = "Built-In Cupboards", Branch = "Soweto",      Date = "2026-07-05", Value = "R11 200", Status = "Completed" },
        };

        // month -> revenue per branch, used on the Admin Analytics chart.
        public static List<(string Month, int Soweto, int Roodepoort, int Randfontein)> MonthlyRevenue { get; } = new()
        {
            ("Jan", 42000, 38000, 29000),
            ("Feb", 51000, 44000, 33000),
            ("Mar", 48000, 41000, 37000),
            ("Apr", 63000, 55000, 41000),
            ("May", 57000, 49000, 38000),
            ("Jun", 71000, 62000, 45000),
        };

        public static List<(string Month, int Quotes)> QuotesTrend { get; } = new()
        {
            ("Jan", 34), ("Feb", 41), ("Mar", 38), ("Apr", 52), ("May", 47), ("Jun", 61)
        };

        public static List<BranchSummary> AdminBranchSummaries { get; } = new()
        {
            new BranchSummary { Name = "Soweto",      Quotes = 112, Revenue = "R71k", DonePercent = 78 },
            new BranchSummary { Name = "Roodepoort",  Quotes = 94,  Revenue = "R62k", DonePercent = 74 },
            new BranchSummary { Name = "Randfontein", Quotes = 67,  Revenue = "R45k", DonePercent = 69 },
        };

        // Mon-Sat revenue bars for the manager overview chart.
        public static Dictionary<string, List<int>> WeeklyRevenueByBranch { get; } = new()
        {
            ["Soweto"] = new() { 11000, 7000, 13500, 9000, 18000, 5000 },
            ["Roodepoort"] = new() { 9000, 6500, 11000, 8500, 15500, 4200 },
            ["Randfontein"] = new() { 6000, 4500, 7800, 6200, 10500, 3100 },
        };

        public static Dictionary<string, string> WeekRevenueDisplay { get; } = new()
        {
            ["Soweto"] = "R71.4k",
            ["Roodepoort"] = "R58.9k",
            ["Randfontein"] = "R38.6k",
        };
    }
}