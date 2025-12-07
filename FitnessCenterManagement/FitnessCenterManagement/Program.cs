using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // Uygulamanýn çalýþtýðý ana dizini garanti altýna alýyoruz
    ContentRootPath = Directory.GetCurrentDirectory(),
    // wwwroot klasörünü açýkça belirtiyoruz
    WebRootPath = "wwwroot"
});

// 1. Veritabaný Baðlantýsýný Ekliyoruz (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Identity (Üyelik) Sistemini Ekliyoruz
// Password ayarlarýný geliþtirme aþamasýnda kolaylýk olsun diye gevþetiyoruz (Ýsteðe baðlý)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();


// Configure the HTTP request pipeline.
app.Environment.EnvironmentName = "Development";

// HTTP istek hattý ayarlarý
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// VERÝTABANI TOHUMLAMA (SEEDING) ÝÞLEMÝ
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Yazdýðýmýz metodu çaðýrýyoruz
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        // Hata olursa loglayabiliriz veya boþ geçebiliriz
        Console.WriteLine("Seeding sýrasýnda hata oluþtu: " + ex.Message);
    }
}

app.Run();
