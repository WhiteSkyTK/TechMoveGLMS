using TechMoveGLMS.Services;

var builder = WebApplication.CreateBuilder(args);

// ── HttpClient pointing at the backend API ────────────────────────
// In Docker the API container is named "glms-backend-api"
// Locally it runs on https://localhost:7100
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                 ?? "https://localhost:7100/";

builder.Services.AddHttpClient<IApiClientService, ApiClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ── Cookie auth — stores JWT received from the API ────────────────
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.Cookie.Name = "glms_session";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

// ── Session — needed to display error messages across redirects ───
builder.Services.AddSession(opts => opts.IdleTimeout = TimeSpan.FromMinutes(30));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();