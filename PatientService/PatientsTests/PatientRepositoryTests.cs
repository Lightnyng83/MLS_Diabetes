using Microsoft.EntityFrameworkCore;
using Moq;
using Patient.Data.Data;
using Patient.Data.Repository.PatientRepository;
using Patient.Models.Bdd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsTests
{
    public class PatientRepositoryTests
    {
        // Méthode pour créer un contexte InMemory basé sur un nom unique (pour isoler chaque test)
        private PatientDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<PatientDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new PatientDbContext(options);
        }

        private IDbContextFactory<PatientDbContext> CreateDbContextFactory(string dbName)
        {
            var mockFactory = new Mock<IDbContextFactory<PatientDbContext>>();
            mockFactory.Setup(f => f.CreateDbContext())
                       .Returns(() => GetInMemoryDbContext(dbName));
            return mockFactory.Object;
        }

        [Fact]
        public async Task AddPatient_ShouldAddPatientToDatabase()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var factory = CreateDbContextFactory(dbName);
            var repository = new PatientRepository(factory);

            var patient = new Patient.Models.Bdd.Patient
            {
                LastName = "Doe",
                FirstName = "John",
                BirthDate = new DateOnly(1990, 1, 1),
                Address = "123 Street",
                PhoneNumber = "1234567890",
                Gender = (int)Gender.Homme
            };

            // Act
            var addedPatient = await repository.AddPatient(patient);

            // Assert
            Assert.NotNull(addedPatient);
            var allPatients = await repository.GetPatients();
            Assert.Contains(allPatients, p => p.LastName == "Doe" && p.FirstName == "John");
        }

        [Fact]
        public async Task GetPatients_ShouldReturnAllPatients()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var factory = CreateDbContextFactory(dbName);

            // Pré-remplir la base InMemory avec deux patients
            using (var context = GetInMemoryDbContext(dbName))
            {
                context.Patients.AddRange(
                    new Patient.Models.Bdd.Patient { LastName = "Doe", FirstName = "John", BirthDate = new DateOnly(1990, 1, 1), Gender = (int)Gender.Homme },
                    new Patient.Models.Bdd.Patient { LastName = "Smith", FirstName = "Jane", BirthDate = new DateOnly(1985, 5, 5), Gender = (int)Gender.Femme }
                );
                context.SaveChanges();
            }

            var repository = new PatientRepository(factory);

            // Act
            var patients = await repository.GetPatients();

            // Assert
            Assert.NotNull(patients);
            Assert.Equal(2, patients.Count());
        }

        [Fact]
        public async Task GetPatient_ById_ShouldReturnCorrectPatient()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var factory = CreateDbContextFactory(dbName);
            Patient.Models.Bdd.Patient testPatient;

            using (var context = GetInMemoryDbContext(dbName))
            {
                testPatient = new Patient.Models.Bdd.Patient { LastName = "Doe", FirstName = "John", BirthDate = new DateOnly(1990, 1, 1), Gender = (int)Gender.Homme };
                context.Patients.Add(testPatient);
                context.SaveChanges();
            }

            var repository = new PatientRepository(factory);

            // Act
            var patient = await repository.GetPatient(testPatient.IdPatient);

            // Assert
            Assert.NotNull(patient);
            Assert.Equal(testPatient.IdPatient, patient.IdPatient);
        }

        [Fact]
        public async Task GetPatient_ByName_ShouldReturnCorrectPatient()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var factory = CreateDbContextFactory(dbName);
            Patient.Models.Bdd.Patient testPatient;

            using (var context = GetInMemoryDbContext(dbName))
            {
                testPatient = new Patient.Models.Bdd.Patient { LastName = "Doe", FirstName = "John", BirthDate = new DateOnly(1990, 1, 1), Gender = (int)Gender.Homme };
                context.Patients.Add(testPatient);
                context.SaveChanges();
            }

            var repository = new PatientRepository(factory);

            // Act
            var patient = await repository.GetPatient("John", "Doe");

            // Assert
            Assert.NotNull(patient);
            Assert.Equal(testPatient.IdPatient, patient.IdPatient);
        }

        [Fact]
        public async Task UpdatePatient_ShouldModifyExistingPatient()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var factory = CreateDbContextFactory(dbName);
            Patient.Models.Bdd.Patient testPatient;

            using (var context = GetInMemoryDbContext(dbName))
            {
                testPatient = new Patient.Models.Bdd.Patient
                {
                    LastName = "Doe",
                    FirstName = "John",
                    BirthDate = new DateOnly(1990, 1, 1),
                    Gender = (int)Gender.Homme,
                    Address = "Old Address"
                };
                context.Patients.Add(testPatient);
                context.SaveChanges();
            }

            var repository = new PatientRepository(factory);
            testPatient.Address = "New Address";

            // Act
            var updatedPatient = await repository.UpdatePatient(testPatient);

            // Assert
            Assert.Equal("New Address", updatedPatient.Address);
        }

        [Fact]
        public async Task DeletePatient_ShouldRemovePatient()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var factory = CreateDbContextFactory(dbName);
            Patient.Models.Bdd.Patient testPatient;

            using (var context = GetInMemoryDbContext(dbName))
            {
                testPatient = new Patient.Models.Bdd.Patient
                {
                    LastName = "Doe",
                    FirstName = "John",
                    BirthDate = new DateOnly(1990, 1, 1),
                    Gender = (int)Gender.Homme
                };
                context.Patients.Add(testPatient);
                context.SaveChanges();
            }

            var repository = new PatientRepository(factory);

            // Act
            await repository.DeletePatient(testPatient.IdPatient);

            // Assert
            using (var context = GetInMemoryDbContext(dbName))
            {
                var patientInDb = await context.Patients.FindAsync(testPatient.IdPatient);
                Assert.Null(patientInDb);
            }
        }
    }
}
