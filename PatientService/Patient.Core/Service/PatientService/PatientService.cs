using AutoMapper;
using Patient.Data.Repository.PatientRepository;
using Patient.Models.Bdd;

namespace Patient.Core.Service.PatientService
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IMapper _mapper;

        public PatientService(IPatientRepository patientRepository, IMapper mapper)
        {
            _patientRepository = patientRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PatientViewModel>> GetPatients()
        {
            var patients = await _patientRepository.GetPatients();
            return _mapper.Map<IEnumerable<PatientViewModel>>(patients);
        }

        public async Task<PatientViewModel> GetPatient(int id)
        {
            var patient = await _patientRepository.GetPatient(id);
            return _mapper.Map<PatientViewModel>(patient);
        }

        public async Task<PatientViewModel> GetPatient(string firstname, string lastname)
        {
            var patient = await _patientRepository.GetPatient(firstname, lastname);
            return _mapper.Map<PatientViewModel>(patient);
        }

        public async Task<PatientViewModel> AddPatient(PatientViewModel patient)
        {
            var patientModel = _mapper.Map<Models.Bdd.Patient>(patient);
            var addedPatient = await _patientRepository.AddPatient(patientModel);
            return _mapper.Map<PatientViewModel>(addedPatient);
        }

        public async Task<PatientViewModel> UpdatePatient(PatientViewModel patient)
        {
            var patientModel = _mapper.Map<Models.Bdd.Patient>(patient);
            var updatedPatient = await _patientRepository.UpdatePatient(patientModel);
            return _mapper.Map<PatientViewModel>(updatedPatient);
        }

        public async Task DeletePatient(int id)
        {
            await _patientRepository.DeletePatient(id);
        }
    }
}
