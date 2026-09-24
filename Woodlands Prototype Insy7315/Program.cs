using Supabase;
using Woodlands_Prototype_Insy7315.Data;
using Woodlands_Prototype_Insy7315.Models;

var builder = WebApplication.CreateBuilder(args);

// Supabase
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:AnonKey"];

if (string.IsNullOrWhiteSpace(supabaseUrl))
{
    throw new InvalidOperationException(
        "Supabase:Url is missing from appsettings.json.");
}

if (string.IsNullOrWhiteSpace(supabaseKey))
{
    throw new InvalidOperationException(
        "Supabase:AnonKey is missing from appsettings.json.");
}

var supabaseOptions = new SupabaseOptions
{
    AutoRefreshToken = true,
    AutoConnectRealtime = false,
    Headers = new Dictionary<string, string>
    {
        { "X-Client-Info", "supabase-csharp/0.7" }
    }
};

builder.Services.AddScoped<Supabase.Client>(_ =>
    new Supabase.Client(
        supabaseUrl,
        supabaseKey,
        supabaseOptions));

builder.Services.AddScoped<
    Woodlands_Prototype_Insy7315.Services.SupabaseAuthService>();

// CORS for Android app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAndroidApp", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Supabase authentication cookie
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "WoodlandsCookie";
        options.DefaultChallengeScheme = "WoodlandsCookie";
        options.DefaultSignInScheme = "WoodlandsCookie";
    })
    .AddCookie("WoodlandsCookie", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// MVC and Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// MVC and Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAndroidApp");

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var supabase = services.GetRequiredService<Supabase.Client>();
            await supabase.InitializeAsync();
            await SupabaseSeeder.SeedAsync(supabase, logger);
            logger.LogInformation("Supabase seeding completed successfully.");
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Supabase connection failed. Seeding skipped. Check your connection string and credentials.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding Supabase data.");
        }
    }
}

// Default MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();