using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MLSDiabetesTests.Json;
using Patient.Data.Data;
using Patient.Models.Bdd;
using Xunit;

namespace PatientsControllerIntegrationTests
{
    public class PatientsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public PatientsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            // Configure le projet API pour utiliser InMemory à la place de SQL Server.
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Supprimer l'enregistrement existant pour IDbContextFactory<PatientDbContext>
                    var factoryDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IDbContextFactory<PatientDbContext>));
                    if (factoryDescriptor != null)
                    {
                        services.Remove(factoryDescriptor);
                    }

                    // Supprimer l'enregistrement pour DbContextOptions<PatientDbContext>
                    var optionsDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<PatientDbContext>));
                    if (optionsDescriptor != null)
                    {
                        services.Remove(optionsDescriptor);
                    }

                    // Supprimer et remplacer l'inscription pour IEnumerable<IDbContextOptionsConfiguration<PatientDbContext>>
                    services.RemoveAll(typeof(IEnumerable<IDbContextOptionsConfiguration<PatientDbContext>>));
                    services.AddSingleton<IEnumerable<IDbContextOptionsConfiguration<PatientDbContext>>>(new List<IDbContextOptionsConfiguration<PatientDbContext>>());

                    // Ajouter une nouvelle configuration pour utiliser InMemory
                    services.AddDbContextFactory<PatientDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestPatientDb");
                    });
                });
            });
        }

        [Fact]
        public async Task GetPatients_ReturnsOkAndPatients()
        {
            // Arrange: seed de la base InMemory avec deux patients.
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PatientDbContext>>();
                using var context = dbContextFactory.CreateDbContext();

                // On n'attribue pas d'ID afin de laisser EF Core générer l'identité
                context.Patients.AddRange(
                    new Patient.Models.Bdd.Patient
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        BirthDate = new DateOnly(1990, 1, 1),
                        Gender = (int)Gender.Homme
                    },
                    new Patient.Models.Bdd.Patient
                    {
                        FirstName = "Jane",
                        LastName = "Doe",
                        BirthDate = new DateOnly(1992, 2, 2),
                        Gender = (int)Gender.Femme
                    }
                );
                await context.SaveChangesAsync();
            }

            var client = _factory.CreateClient();

            // Act: appel à GET /api/patients
            var response = await client.GetAsync("/api/patients");

            // Assert
            response.EnsureSuccessStatusCode();
            var patients = await response.Content.ReadFromJsonAsync<IEnumerable<PatientViewModel>>();
            Assert.NotNull(patients);
            Assert.True(patients.Count() >= 2);
        }

        [Fact]
        public async Task CreatePatient_ReturnsOkAndCreatedPatient()
        {
            // Arrange
            var client = _factory.CreateClient();
            var newPatient = new PatientViewModel
            {
                FirstName = "Jane",
                LastName = "Doe",
                BirthDate = new DateOnly(1995, 5, 15),
                Gender = (int)Gender.Femme
            };

            // Act: appel à POST /api/patients
            var response = await client.PostAsJsonAsync("/api/patients", newPatient);

            // Assert
            response.EnsureSuccessStatusCode();
            var createdPatient = await response.Content.ReadFromJsonAsync<PatientViewModel>();
            Assert.NotNull(createdPatient);
            Assert.Equal("Jane", createdPatient.FirstName);
            Assert.Equal("Doe", createdPatient.LastName);
        }

        [Fact]
        public async Task GetPatient_ById_ReturnsOkAndPatient()
        {
            // Arrange: ajouter un patient dans la DB.
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PatientDbContext>>();
                using var context = dbContextFactory.CreateDbContext();

                var patient = new Patient.Models.Bdd.Patient
                {
                    FirstName = "John",
                    LastName = "Doe",
                    BirthDate = new DateOnly(1975, 2, 2),
                    Gender = (int)Gender.Homme
                };
                context.Patients.Add(patient);
                await context.SaveChangesAsync();
            }

            var client = _factory.CreateClient();
            // Supposons que ce patient ait reçu l'ID 1.
            var response = await client.GetAsync("/api/patients/2");

            // Assert
            response.EnsureSuccessStatusCode();
            var options = new JsonSerializerOptions();
            options.Converters.Add(new DateOnlyJsonConverter());
            var returnedPatient = await response.Content.ReadFromJsonAsync<PatientViewModel>();
            Assert.NotNull(returnedPatient);
            Assert.Equal("John", returnedPatient.FirstName);
        }

        [Fact]
        public async Task GetPatient_ByName_ReturnsOkAndPatient()
        {
            // Arrange: ajouter un patient dans la DB.
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PatientDbContext>>();
                using var context = dbContextFactory.CreateDbContext();

                var patient = new Patient.Models.Bdd.Patient
                {
                    FirstName = "Bob",
                    LastName = "Marley",
                    BirthDate = new DateOnly(1975, 2, 2),
                    Gender = (int)Gender.Homme
                };
                context.Patients.Add(patient);
                await context.SaveChangesAsync();
            }

            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/patients/byname?firstname=Bob&lastname=Marley");

            // Assert
            response.EnsureSuccessStatusCode();
            var returnedPatient = await response.Content.ReadFromJsonAsync<PatientViewModel>();
            Assert.NotNull(returnedPatient);
            Assert.Equal("Bob", returnedPatient.FirstName);
        }

        [Fact]
        public async Task UpdatePatient_ReturnsOkAndUpdatedPatient()
        {
            // Arrange: ajouter un patient dans la DB.
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PatientDbContext>>();
                using var context = dbContextFactory.CreateDbContext();

                var patient = new Patient.Models.Bdd.Patient
                {
                    FirstName = "Charlie",
                    LastName = "Brown",
                    BirthDate = new DateOnly(1985, 3, 3),
                    Gender = (int)Gender.Homme,
                    Address = "Old Address"
                };
                context.Patients.Add(patient);
                await context.SaveChangesAsync();
            }

            var client = _factory.CreateClient();

            // Préparer les données mises à jour.
            var updatedPatient = new PatientViewModel
            {
                FirstName = "Charlie",
                LastName = "Brown",
                BirthDate = new DateOnly(1985, 3, 3),
                Gender = (int)Gender.Homme,
                // Nouvelle valeur pour Address.
                Address = "New Address"
            };

            // Act: appel à PUT /api/patients/1 (supposé être le patient ajouté)
            var putResponse = await client.PutAsJsonAsync("/api/patients/2", updatedPatient);
            putResponse.EnsureSuccessStatusCode();

            // Vérifier via GET
            var getResponse = await client.GetAsync("/api/patients/2");
            getResponse.EnsureSuccessStatusCode();
            var returnedPatient = await getResponse.Content.ReadFromJsonAsync<PatientViewModel>();
            Assert.NotNull(returnedPatient);
            Assert.Equal("New Address", returnedPatient.Address);
        }

        [Fact]
        public async Task DeletePatient_ReturnsNoContent()
        {
            // Arrange: ajouter un patient dans la DB.
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PatientDbContext>>();
                using var context = dbContextFactory.CreateDbContext();

                var patient = new Patient.Models.Bdd.Patient
                {
                    FirstName = "Daisy",
                    LastName = "Duck",
                    BirthDate = new DateOnly(1990, 4, 4),
                    Gender = (int)Gender.Femme
                };
                context.Patients.Add(patient);
                await context.SaveChangesAsync();
            }

            var client = _factory.CreateClient();

            // Act: appel à DELETE /api/patients/1
            var deleteResponse = await client.DeleteAsync("/api/patients/1");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Vérifier que le patient n'est plus disponible
            var getResponse = await client.GetAsync("/api/patients/1");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
