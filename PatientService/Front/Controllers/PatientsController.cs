using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Front.Models;

namespace Front.Controllers
{
    public class PatientsController : Controller
    {
        private readonly HttpClient _httpClient;

        public PatientsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Afficher la liste des patients
        public async Task<IActionResult> Index()
        {
            var patients = await _httpClient.GetFromJsonAsync<IEnumerable<PatientViewModel>>("/api/patient/Patients");
            return View(patients);
        }

        // Afficher le formulaire de création
        public IActionResult Create() => View();

        // Traiter le formulaire de création
        [HttpPost]
        public async Task<IActionResult> Create(PatientViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _httpClient.PostAsJsonAsync("/api/patient/Patients", model);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Erreur lors de la création du patient");
            return View(model);
        }

        // Méthodes Edit et Delete à ajouter de façon similaire
    }
}

