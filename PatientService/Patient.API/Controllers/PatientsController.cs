using Microsoft.AspNetCore.Mvc;
using Patient.Core.Service.PatientService;
using Patient.Models.Bdd;

namespace Patient.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        // GET api/patients
        [HttpGet]
        public async Task<IActionResult> GetPatients()
        {
            var patients = await _patientService.GetPatients();
            return Ok(patients);
        }

        // GET api/patients/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatient(int id)
        {
            var patient = await _patientService.GetPatient(id);
            if (patient == null)
                return NotFound();

            return Ok(patient);
        }

        // GET api/patients/byname?firstname=John&lastname=Doe
        [HttpGet("byname")]
        public async Task<IActionResult> GetPatientByName([FromQuery] string firstname, [FromQuery] string lastname)
        {
            var patient = await _patientService.GetPatient(firstname, lastname);
            if (patient == null)
                return NotFound();

            return Ok(patient);
        }

        // POST api/patients
        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] PatientViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdPatient = await _patientService.AddPatient(model);
            // Comme le ViewModel ne contient pas d'ID, on retourne simplement le modèle créé
            return Ok(createdPatient);
        }

        // PUT api/patients/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] PatientViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ici, on pourrait éventuellement récupérer le patient existant via l'ID, 
            // fusionner avec le model partiel et ensuite mettre à jour.
            // Pour simplifier, on passe directement le model au service.
            var updatedPatient = await _patientService.UpdatePatient(model);
            return Ok(updatedPatient);
        }

        // DELETE api/patients/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            await _patientService.DeletePatient(id);
            return NoContent();
        }
    }
}
