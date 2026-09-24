using Woodlands_Prototype_Insy7315.Models;

namespace Woodlands_Prototype_Insy7315.Data
{
    public static class CatalogSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (!db.Products.Any())
            {
                db.Products.AddRange(WoodLinkData.Products);
            }

            if (!db.Testimonials.Any())
            {
                foreach (var t in WoodLinkData.Testimonials)
                {
                    db.Testimonials.Add(new Testimonial
                    {
                        Name = t.Name,
                        Role = t.Role,
                        Location = t.Location,
                        Rating = t.Rating,
                        Review = t.Review,
                        Project = t.Project
                    });
                }
            }

            if (!db.Services.Any())
            {
                db.Services.AddRange(new List<Service>
                {
                    new Service { Name = "Kitchen Units", Description = "Custom kitchen cabinetry", IsActive = true },
                    new Service { Name = "TV Stands", Description = "Wall-mounted and floor-standing units", IsActive = true },
                    new Service { Name = "Built-In Cupboards", Description = "Floor-to-ceiling wardrobes", IsActive = true },
                    new Service { Name = "Cutting & Edging", Description = "CNC cutting and edge banding", IsActive = true },
                    new Service { Name = "General Enquiry", Description = "Anything else", IsActive = true },
                });
            }

            if (!db.Faqs.Any())
            {
                db.Faqs.AddRange(new List<FaqItem>
                {
                    new FaqItem { Category = "Materials", Question = "What is PG Bison and why does it matter?", Answer = "PG Bison is South Africa's leading manufacturer of timber-based board products — melamine, chipboard, MDF, and Supawood. Using PG Bison materials means your units meet the highest local standards for strength, durability, and finish quality. As an authorised partner, we guarantee every board is 100% genuine." },
                    new FaqItem { Category = "Process", Question = "Do you offer a measuring and design service?", Answer = "Yes. We offer a free consultation and site measurement for all custom built-in units. A team member will visit your space, take precise measurements, suggest the best material and finish options, and provide a detailed quote before any work begins." },
                    new FaqItem { Category = "Services", Question = "What services do you offer besides custom units?", Answer = "In addition to full kitchen units, TV stands, and built-in cupboards, we provide precision CNC board cutting and professional edge-banding services. Contractors and DIY builders who just need boards cut to size or edged are welcome — often with same-day turnaround." },
                    new FaqItem { Category = "Process", Question = "How long does a custom kitchen or cupboard take?", Answer = "Lead times depend on complexity. A standard built-in cupboard typically takes 5–10 business days from confirmed order to installation. A full kitchen suite may take 2–4 weeks. You'll receive a firm delivery timeline in your quote." },
                    new FaqItem { Category = "Materials", Question = "What colour and finish options are available?", Answer = "We stock the full PG Bison Melamine range — over 70 colours and textures from solid whites and greys to wood-grain effects and high-gloss finishes. Special-order finishes from the PG Bison catalogue are also available upon request." },
                    new FaqItem { Category = "Installation", Question = "Do you install the units or just supply them?", Answer = "We do both. Choose a supply-only option (ideal for contractors with their own installation teams) or a full supply-and-install service. All installation is carried out by our own trained team with proper tools and fixings." },
                    new FaqItem { Category = "Branches", Question = "Which branch should I visit?", Answer = "We have three branches: Soweto, Roodepoort, and Randfontein. Visit whichever is closest to your project site. All three branches stock the same product ranges and offer the same services. You can also call or WhatsApp us to start your quote remotely." },
                    new FaqItem { Category = "Trade", Question = "Do you work with interior designers and contractors?", Answer = "Absolutely. A significant portion of our work comes from interior designers and building contractors. We offer trade pricing, bulk-order discounts, and dedicated account management for regular clients." },
                    new FaqItem { Category = "Services", Question = "Is there a minimum order for cutting and edging?", Answer = "There is no strict minimum for walk-in customers — we can cut and edge a single sheet if needed. For bulk orders (10+ sheets), we offer discounted pricing and prioritised turnaround. Contact your nearest branch for current bulk pricing." },
                    new FaqItem { Category = "Warranty", Question = "What warranty do you offer?", Answer = "All custom-built units come with a 12-month workmanship warranty covering any defects in construction or installation. PG Bison board materials carry their own manufacturer warranty for structural integrity. We're committed to making it right if anything falls short." }
                });
            }

            await db.SaveChangesAsync();
        }
    }
}