using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace TechMoveGLMS.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration     _config;

        public AuthController(IHttpClientFactory factory, IConfiguration config)
        {
            _factory = factory;
            _config  = config;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password,
                                               string? returnUrl = null)
        {
            var apiBase = _config["ApiSettings:BaseUrl"] ?? "https://localhost:7100/";
            var client  = _factory.CreateClient();
            client.BaseAddress = new Uri(apiBase);

            var payload = JsonSerializer.Serialize(
                new { username, password });

            var resp = await client.PostAsync(
                "api/auth/login",
                new StringContent(payload, Encoding.UTF8, "application/json"));

            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }

            var body = await resp.Content.ReadAsStringAsync();
            var auth = JsonSerializer.Deserialize<JsonElement>(body);
            var token = auth.GetProperty("token").GetString()!;

            // Store raw JWT in a cookie (ApiClientService reads it for API calls)
            Response.Cookies.Append("glms_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure   = true,
                SameSite = SameSiteMode.Lax,
                Expires  = DateTimeOffset.UtcNow.AddHours(8)
            });

            // Sign in with cookie auth so [Authorize] in MVC works
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity  = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("CookieAuth", principal);

            return Redirect(returnUrl ?? "/");
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            Response.Cookies.Delete("glms_token");
            return RedirectToAction("Login");
        }
    }
}
