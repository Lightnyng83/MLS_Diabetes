using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Patient.Models.Bdd;

namespace Patient.Core.Service.PatientService
{
    public interface IPatientService
    {
        Task<IEnumerable<PatientViewModel>> GetPatients();
        Task<PatientViewModel> GetPatient(int id);
        Task<PatientViewModel> GetPatient(string firstname, string lastname);
        Task<PatientViewModel> AddPatient(PatientViewModel patient);
        Task<PatientViewModel> UpdatePatient(PatientViewModel patient);
        Task DeletePatient(int id);
    }
}
