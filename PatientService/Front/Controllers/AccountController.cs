using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Front.Models; 
using System.Net.Http;
using System.Net.Http.Json;

namespace Front.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;

        public AccountController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Appel à l'API Gateway pour s'authentifier
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", model);
            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                // Stockez le token (ex. via cookies ou session)
                // Puis redirigez vers la page CRUD
                return RedirectToAction("Index", "Patients");
            }
            else
            {
                ModelState.AddModelError("", "Login failed");
                return View(model);
            }
        }
    }
}