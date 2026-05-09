using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Data;
using TechMoveGLMS.Interfaces;
using TechMoveGLMS.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Design Pattern Services ──
// Strategy Pattern: currency conversion
builder.Services.AddHttpClient<ICurrencyStrategy, ExchangeRateApiStrategy>();

// Validation service: business rules separated from controllers
builder.Services.AddScoped<ContractValidationService>();

// Document handling service: file upload / validation
builder.Services.AddScoped<DocumentHandlingService>();

// ── MVC ──
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();