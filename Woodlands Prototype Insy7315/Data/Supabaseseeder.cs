using Woodlands_Prototype_Insy7315.Models;

namespace Woodlands_Prototype_Insy7315.Data
{

    public static class SupabaseSeeder
    {
        public static async Task SeedAsync(Supabase.Client supabase, ILogger logger)
        {
            await SeedProducts(supabase, logger);
            await SeedTestimonials(supabase, logger);
            await SeedServices(supabase, logger);
            await SeedFaqs(supabase, logger);
            await SeedPrototypeAccounts(supabase, logger);
        }

        private static async Task SeedProducts(Supabase.Client supabase, ILogger logger)
        {
            try
            {
                var existing = await supabase.From<SupabaseProduct>().Select("id").Get();
                if (existing.Models.Count > 0)
                {
                    return;
                }

                foreach (var p in WoodLinkData.Products)
                {
                    logger.LogInformation(
                    "INSERTING PRODUCT: Id={Id}, Title={Title}",
                    p.Id,
                    p.Title);
                    await supabase.From<SupabaseProduct>().Insert(new SupabaseProduct
                    {
                        Id = p.Id,
                        Category = p.Category,
                        Title = p.Title,
                        Tagline = p.Tagline,
                        Description = p.Description,
                        Image = p.Image,
                        Gallery = p.GalleryJson,
                        Features = p.FeaturesJson,
                        Finishes = p.FinishesJson,
                        LeadTime = p.LeadTime,
                        Tag = p.Tag,
                        Price = p.Price
                    });
                }

                logger.LogInformation("Seeded {Count} products into Supabase.", WoodLinkData.Products.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "FAILED TO SEED PRODUCTS INTO SUPABASE.");
                throw;
            }
        }

        private static async Task SeedTestimonials(Supabase.Client supabase, ILogger logger)
        {
            try
            {
                var existing = await supabase.From<SupabaseTestimonial>().Select("id").Get();
                if (existing.Models.Count > 0)
                {
                    return;
                }

                foreach (var t in WoodLinkData.Testimonials)
                {
                    await supabase.From<SupabaseTestimonial>().Insert(new SupabaseTestimonial
                    {
                        Name = t.Name,
                        Role = t.Role,
                        Location = t.Location,
                        Rating = t.Rating,
                        Review = t.Review,
                        Project = t.Project
                    });
                }

                logger.LogInformation("Seeded {Count} testimonials into Supabase.", WoodLinkData.Testimonials.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "FAILED TO SEED TESTIMONIALS INTO SUPABASE.");
                throw;
            }
        }

        private static async Task SeedServices(Supabase.Client supabase, ILogger logger)
        {
            try
            {
                var existing = await supabase.From<SupabaseService>().Select("id").Get();
                if (existing.Models.Count > 0)
                {
                    return;
                }

                var services = new List<SupabaseService>
                {
                    new() { Name = "Kitchen Units", Description = "Custom kitchen cabinetry", IsActive = true },
                    new() { Name = "TV Stands", Description = "Wall-mounted and floor-standing units", IsActive = true },
                    new() { Name = "Built-In Cupboards", Description = "Floor-to-ceiling wardrobes", IsActive = true },
                    new() { Name = "Cutting & Edging", Description = "CNC cutting and edge banding", IsActive = true },
                    new() { Name = "General Enquiry", Description = "Anything else", IsActive = true },
                };

                foreach (var s in services)
                {
                    await supabase.From<SupabaseService>().Insert(s);
                }

                logger.LogInformation("Seeded {Count} services into Supabase.", services.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "FAILED TO SEED SERVICES INTO SUPABASE.");
                throw;
            }
        }

        private static async Task SeedFaqs(Supabase.Client supabase, ILogger logger)
        {
            try
            {
                var existing = await supabase.From<SupabaseFaqItem>().Select("id").Get();
                if (existing.Models.Count > 0)
                {
                    return;
                }

                var faqs = new List<SupabaseFaqItem>
                {
                    new() { Category = "Materials", Question = "What is PG Bison and why does it matter?", Answer = "PG Bison is South Africa's leading manufacturer of timber-based board products — melamine, chipboard, MDF, and Supawood. Using PG Bison materials means your units meet the highest local standards for strength, durability, and finish quality. As an authorised partner, we guarantee every board is 100% genuine." },
                    new() { Category = "Process", Question = "Do you offer a measuring and design service?", Answer = "Yes. We offer a free consultation and site measurement for all custom built-in units. A team member will visit your space, take precise measurements, suggest the best material and finish options, and provide a detailed quote before any work begins." },
                    new() { Category = "Services", Question = "What services do you offer besides custom units?", Answer = "In addition to full kitchen units, TV stands, and built-in cupboards, we provide precision CNC board cutting and professional edge-banding services. Contractors and DIY builders who just need boards cut to size or edged are welcome — often with same-day turnaround." },
                    new() { Category = "Process", Question = "How long does a custom kitchen or cupboard take?", Answer = "Lead times depend on complexity. A standard built-in cupboard typically takes 5–10 business days from confirmed order to installation. A full kitchen suite may take 2–4 weeks. You'll receive a firm delivery timeline in your quote." },
                    new() { Category = "Materials", Question = "What colour and finish options are available?", Answer = "We stock the full PG Bison Melamine range — over 70 colours and textures from solid whites and greys to wood-grain effects and high-gloss finishes. Special-order finishes from the PG Bison catalogue are also available upon request." },
                    new() { Category = "Installation", Question = "Do you install the units or just supply them?", Answer = "We do both. Choose a supply-only option (ideal for contractors with their own installation teams) or a full supply-and-install service. All installation is carried out by our own trained team with proper tools and fixings." },
                    new() { Category = "Branches", Question = "Which branch should I visit?", Answer = "We have three branches: Soweto, Roodepoort, and Randfontein. Visit whichever is closest to your project site. All three branches stock the same product ranges and offer the same services. You can also call or WhatsApp us to start your quote remotely." },
                    new() { Category = "Trade", Question = "Do you work with interior designers and contractors?", Answer = "Absolutely. A significant portion of our work comes from interior designers and building contractors. We offer trade pricing, bulk-order discounts, and dedicated account management for regular clients." },
                    new() { Category = "Services", Question = "Is there a minimum order for cutting and edging?", Answer = "There is no strict minimum for walk-in customers — we can cut and edge a single sheet if needed. For bulk orders (10+ sheets), we offer discounted pricing and prioritised turnaround. Contact your nearest branch for current bulk pricing." },
                    new() { Category = "Warranty", Question = "What warranty do you offer?", Answer = "All custom-built units come with a 12-month workmanship warranty covering any defects in construction or installation. PG Bison board materials carry their own manufacturer warranty for structural integrity. We're committed to making it right if anything falls short." }
                };

                foreach (var f in faqs)
                {
                    await supabase.From<SupabaseFaqItem>().Insert(f);
                }

                logger.LogInformation("Seeded {Count} FAQs into Supabase.", faqs.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "FAILED TO SEED FAQS INTO SUPABASE.");
                throw;
            }
        }

        private static async Task SeedPrototypeAccounts(Supabase.Client supabase, ILogger logger)
        {
            try
            {
                await SeedAccount(supabase, logger, "Admin User", "admin@woodlandsdb.co.za", "admin123", "Admin", null);
                await SeedAccount(supabase, logger, "Soweto Manager", "soweto@woodlandsdb.co.za", "manager123", "Manager (Soweto)", "Soweto");
                await SeedAccount(supabase, logger, "Roodepoort Manager", "roodepoort@woodlandsdb.co.za", "manager123", "Manager (Roodepoort)", "Roodepoort");
                await SeedAccount(supabase, logger, "Randfontein Manager", "randfontein@woodlandsdb.co.za", "manager123", "Manager (Randfontein)", "Randfontein");
                await SeedAccount(supabase, logger, "Prototype Customer", "customer@example.com", "customer123", "Customer", null);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "FAILED TO SEED ACCOUNTS INTO SUPABASE.");
                throw;
            }
        }

        private static async Task SeedAccount(
            Supabase.Client supabase,
            ILogger logger,
            string fullName,
            string email,
            string password,
            string role,
            string? branch)
        {
            var existing = await supabase
                .From<SupabaseAppUser>()
                .Where(u => u.Email == email)
                .Get();

            if (existing.Models.Count > 0)
            {
                return;
            }

            try
            {
                var session = await supabase.Auth.SignUp(email: email, password: password);

                if (session?.User == null)
                {
                    logger.LogWarning("Could not create prototype account {Email} — no user returned.", email);
                    return;
                }

                await supabase.From<SupabaseAppUser>().Insert(new SupabaseAppUser
                {
                    Id = session.User.Id.ToString(),
                    FullName = fullName,
                    Email = email,
                    Phone = "",
                    Role = role,
                    Branch = branch,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                });

                logger.LogInformation("Seeded prototype account {Email} ({Role}).", email, role);
            }
            catch (Exception ex)
            {

                logger.LogWarning(ex, "Could not seed prototype account {Email}.", email);
            }
        }
    }
}