using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Patient.Data.Data
{
    public class SeedData
    {
        private readonly IDbContextFactory<PatientDbContext> _dbContextFactory;

        public SeedData(IDbContextFactory<PatientDbContext> _dbContext)
        {
            _dbContextFactory = _dbContext;
        }

        public async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PatientDbContext>();

            var nom = dbContext.Patients.FirstOrDefault(p => p.LastName == "TestNone");
            if (nom == null)
            {
                dbContext.Patients.Add(new Models.Bdd.Patient
                {
                    LastName = "Test",
                    FirstName = "TestNone",
                    BirthDate = new DateOnly(1996, 12, 31),
                    Address = "1 Brookside St",
                    PhoneNumber = "100-222-3333",
                    Gender = 0
                });
                dbContext.Patients.Add(new Models.Bdd.Patient
                {
                    LastName = "Test",
                    FirstName = "TestBorderline",
                    BirthDate = new DateOnly(1945, 06, 24),
                    Address = "2 High St",
                    PhoneNumber = "200-333-4444",
                    Gender = 1
                });
                dbContext.Patients.Add(new Models.Bdd.Patient
                {
                    LastName = "Test",
                    FirstName = "TestInDanger",
                    BirthDate = new DateOnly(2004, 06, 18),
                    Address = "3 Club Road",
                    PhoneNumber = "300-444-5555",
                    Gender = 1
                });
                dbContext.Patients.Add(new Models.Bdd.Patient
                {
                    LastName = "Test",
                    FirstName = "TestEarlyOnset",
                    BirthDate = new DateOnly(2002, 06, 28),
                    Address = "4 Valley Dr",
                    PhoneNumber = "400-555-6666",
                    Gender = 0
                });
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
