require('dotenv').config();
const { createClient } = require('@supabase/supabase-js');

const supabase = createClient(process.env.SUPABASE_URL, process.env.SUPABASE_KEY);

// ============================================================
// SEED DATA — edit these arrays as needed
// ============================================================

const products = [
    {
        id: "oak-table-001",
        category: "Kitchen Units",
        title: "Solid Oak Dining Table",
        tagline: "Handcrafted elegance for your home",
        description: "A beautifully handcrafted solid oak dining table, perfect for family gatherings.",
        image: "https://images.unsplash.com/photo-1617806118233-18e1de247200",
        gallery: JSON.stringify(["https://images.unsplash.com/photo-1617806118233-18e1de247200"]),
        features: JSON.stringify(["Solid oak construction", "Seats 6-8 people", "Hand-finished"]),
        lead_time: "3-4 weeks",
        tag: "Popular",
        price: "R12,500"
    },
    {
        id: "modern-kitchen-002",
        category: "Kitchen Units",
        title: "Modern Kitchen Cabinet Set",
        tagline: "Sleek design meets function",
        description: "A complete modern kitchen cabinet set with soft-close doors and premium finish.",
        image: "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136",
        gallery: JSON.stringify(["https://images.unsplash.com/photo-1556909114-f6e7ad7d3136"]),
        features: JSON.stringify(["Soft-close doors", "Premium melamine finish", "Custom sizing"]),
        lead_time: "4-6 weeks",
        tag: "New",
        price: "R45,000"
    },
    {
        id: "walnut-shelf-003",
        category: "TV Stands",
        title: "Walnut Floating Shelf",
        tagline: "Minimalist storage solution",
        description: "Elegant floating shelf crafted from premium walnut wood.",
        image: "https://images.unsplash.com/photo-1594026112284-02bb6f3352fe",
        gallery: JSON.stringify([]),
        features: JSON.stringify(["Hidden mounting", "Solid walnut", "Various lengths"]),
        lead_time: "1-2 weeks",
        tag: null,
        price: "R1,200"
    }
];

const testimonials = [
    {
        name: "Sarah van der Merwe",
        role: "Homeowner",
        location: "Soweto",
        rating: 5,
        review: "Absolutely thrilled with my new kitchen! The team was professional and the finish is flawless.",
        project: "Custom Kitchen Renovation"
    },
    {
        name: "Thabo Nkosi",
        role: "Interior Designer",
        location: "Johannesburg",
        rating: 5,
        review: "Woodlands is my go-to for custom cabinetry. Their attention to detail is unmatched.",
        project: "Multiple Residential Projects"
    },
    {
        name: "Emily Jacobs",
        role: "Homeowner",
        location: "Roodepoort",
        rating: 4,
        review: "Great quality cupboards. Installation was quick and the team cleaned up after themselves.",
        project: "Built-in Cupboard Installation"
    }
];

const services = [
    { name: "Kitchen Units", description: "Custom kitchen cabinetry built to your exact specs.", image: "", is_active: true },
    { name: "TV Stands", description: "Wall-mounted and floor-standing TV units.", image: "", is_active: true },
    { name: "Built-In Cupboards", description: "Floor-to-ceiling wardrobes and storage.", image: "", is_active: true },
    { name: "Cutting & Edging", description: "CNC precision cutting and edge banding.", image: "", is_active: true },
    { name: "General Enquiry", description: "Anything else you need — just ask.", image: "", is_active: true }
];

const faqs = [
    { category: "Materials", question: "What is PG Bison and why does it matter?", answer: "PG Bison is South Africa's leading manufacturer of timber-based board products. Using PG Bison materials means your units meet the highest local standards." },
    { category: "Process", question: "Do you offer a measuring and design service?", answer: "Yes. We offer a free consultation and site measurement for all custom built-in units." },
    { category: "Services", question: "What services do you offer besides custom units?", answer: "We provide precision CNC board cutting and professional edge-banding services in addition to full custom units." },
    { category: "Process", question: "How long does a custom kitchen or cupboard take?", answer: "A standard built-in cupboard typically takes 5-10 business days. A full kitchen suite may take 2-4 weeks." },
    { category: "Warranty", question: "What warranty do you offer?", answer: "All custom-built units come with a 12-month workmanship warranty." }
];

const branches = [
    { name: "Soweto", region: "Soweto", phone: "+27 11 xxx xxxx", hours: "Mon-Fri: 8am-5pm", notes: "Main branch" },
    { name: "Roodepoort", region: "Roodepoort", phone: "+27 11 xxx xxxx", hours: "Mon-Fri: 8am-5pm", notes: "Branch office" },
    { name: "Randfontein", region: "Randfontein", phone: "+27 11 xxx xxxx", hours: "Mon-Fri: 8am-5pm", notes: "Branch office" }
];

// Seeding


async function clearTable(table) {
    // Delete all rows — use .neq to bypass "no filter" restriction
    const { error } = await supabase.from(table).delete().neq('id', -1);
    if (error) console.warn(`  Could not clear ${table}: ${error.message}`);
}

async function seedTable(table, rows, label) {
    console.log(`\n Seeding ${label}...`);

    const { data: existing } = await supabase.from(table).select('id').limit(1);
    if (existing && existing.length > 0) {
        console.log(`  ${label} already has data — skipping.`);
        return;
    }

    const { error } = await supabase.from(table).insert(rows);
    if (error) {
        console.error(`  Failed to seed ${label}:`, error.message);
    } else {
        console.log(`  Seeded ${rows.length} ${label}.`);
    }
}

async function seedPrototypeUsers() {
    console.log(`\n Seeding prototype accounts...`);

    const accounts = [
        { email: "admin@woodlandsdb.co.za", password: "admin123", full_name: "Admin User", role: "Admin", branch: null },
        { email: "soweto@woodlandsdb.co.za", password: "manager123", full_name: "Soweto Manager", role: "Manager (Soweto)", branch: "Soweto" },
        { email: "roodepoort@woodlandsdb.co.za", password: "manager123", full_name: "Roodepoort Manager", role: "Manager (Roodepoort)", branch: "Roodepoort" },
        { email: "randfontein@woodlandsdb.co.za", password: "manager123", full_name: "Randfontein Manager", role: "Manager (Randfontein)", branch: "Randfontein" },
        { email: "customer@example.com", password: "customer123", full_name: "Prototype Customer", role: "Customer", branch: null }
    ];

    for (const acct of accounts) {
        // Check if user profile already exists
        const { data: existing } = await supabase
            .from('app_users')
            .select('id')
            .eq('email', acct.email)
            .limit(1);

        if (existing && existing.length > 0) {
            console.log(`  ${acct.email} already exists — skipping.`);
            continue;
        }

        // Create Supabase Auth user (admin endpoint requires service role key)
        const { data: authData, error: authError } = await supabase.auth.admin.createUser({
            email: acct.email,
            password: acct.password,
            email_confirm: true
        });

        if (authError || !authData.user) {
            console.error(`  Auth failed for ${acct.email}:`, authError?.message);
            continue;
        }

        // Insert app_users profile
        const { error: profileError } = await supabase.from('app_users').insert([{
            id: authData.user.id,
            full_name: acct.full_name,
            email: acct.email,
            phone: "",
            role: acct.role,
            branch: acct.branch,
            active: true
        }]);

        if (profileError) {
            console.error(` Profile failed for ${acct.email}:`, profileError.message);
        } else {
            console.log(` Seeded ${acct.email} (${acct.role})`);
        }
    }
}

async function run() {
    console.log(" Starting Woodlands database seed...\n");

    // Comment out any line you DON'T want to re-seed
    await seedTable('products', products, 'products');
    await seedTable('testimonials', testimonials, 'testimonials');
    await seedTable('services', services, 'services');
    await seedTable('faqs', faqs, 'faqs');
    await seedTable('branches', branches, 'branches');
    await seedPrototypeUsers();

    console.log("\n Seeding complete!\n");
    process.exit(0);
}

run().catch(err => {
    console.error("Fatal error during seeding:", err);
    process.exit(1);
});