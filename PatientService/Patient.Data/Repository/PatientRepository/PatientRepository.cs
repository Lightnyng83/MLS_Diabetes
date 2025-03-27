using Microsoft.EntityFrameworkCore;
using Patient.Data.Data;

namespace Patient.Data.Repository.PatientRepository
{
    public class PatientRepository : IPatientRepository
    {
        private readonly IDbContextFactory<PatientDbContext> _dbContextFactory;

        public PatientRepository(IDbContextFactory<PatientDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<Models.Bdd.Patient>> GetPatients()
        {
            await using var context = _dbContextFactory.CreateDbContext();
            return await context.Patients.ToListAsync();
        }

        public async Task<Models.Bdd.Patient?> GetPatient(int id)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            return await context.Patients.FindAsync(id);
        }

        public async Task<Models.Bdd.Patient?> GetPatient(string firstname, string lastname)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            return await context.Patients.Where(x => x.FirstName == firstname && x.LastName == lastname).FirstAsync();
        }

        public async Task<Models.Bdd.Patient> AddPatient(Models.Bdd.Patient patient)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            var entity = context.Patients.Add(patient);
            await context.SaveChangesAsync();
            return entity.Entity;
        }

        public async Task<Models.Bdd.Patient> UpdatePatient(Models.Bdd.Patient patient)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            var entity = context.Patients.Update(patient);
            await context.SaveChangesAsync();
            return entity.Entity;
        }

        public async Task DeletePatient(int id)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            var patient = await context.Patients.FindAsync(id);
            if (patient != null)
            {
                context.Patients.Remove(patient);
                await context.SaveChangesAsync();
            }
        }
    }
}
