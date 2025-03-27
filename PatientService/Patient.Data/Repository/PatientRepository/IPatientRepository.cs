namespace Patient.Data.Repository.PatientRepository
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Models.Bdd.Patient>> GetPatients();
        Task<Models.Bdd.Patient?> GetPatient(int id);
        Task<Models.Bdd.Patient?> GetPatient(string firstname, string lastname);
        Task<Models.Bdd.Patient> AddPatient(Models.Bdd.Patient patient);
        Task<Models.Bdd.Patient> UpdatePatient(Models.Bdd.Patient patient);
        Task DeletePatient(int id);
    }
}
