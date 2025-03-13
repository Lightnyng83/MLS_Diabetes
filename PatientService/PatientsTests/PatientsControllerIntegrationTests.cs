using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Patient.Data.Data;
using Patient.Models.Bdd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Text.Json.Serialization;

namespace PatientsTests
{
    public class PatientsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _baseFactory;

        public PatientsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            // On garde la factory de base (non modifiée)
            _baseFactory = factory;
        }

        // Crée une instance de WebApplicationFactory avec une base InMemory nommée de façon unique.
        private WebApplicationFactory<Program> CreateFactoryWithUniqueDatabase()
        {
            string uniqueDbName = "TestPatientDb_" + Guid.NewGuid().ToString();
            return _baseFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Supprimer toutes les inscriptions liées au DbContext afin de remplacer la configuration SQL Server.
                    services.RemoveAll<IDbContextFactory<PatientDbContext>>();
                    services.RemoveAll<DbContextOptions<PatientDbContext>>();
                    services.RemoveAll<IEnumerable<IDbContextOptionsConfiguration<PatientDbContext>>>();

                    // Enregistrer PatientDbContext pour utiliser InMemory avec un nom unique.
                    services.AddDbContextFactory<PatientDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(uniqueDbName);
                    });
                });
            });
        }

        // Méthode de nettoyage : supprime la base de données utilisée par la factory.
        private async Task ClearDatabase(WebApplicationFactory<Program> factory)
        {
            using (var scope = factory.Services.CreateScope())
            {
                var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PatientDbContext>>();
                using var context = dbContextFactory.CreateDbContext();
                await context.Database.EnsureDeletedAsync();
            }
        }

        [Fact]
        public async Task GetPatients_ReturnsOkAndPatients()
        {
            // Utiliser une factory avec un DB unique pour ce test.
            var testFactory = CreateFactoryWithUniqueDatabase();

            // Arrange : seeder la base avec deux patients.
            using (var scope = testFactory.Services.CreateScope())
            {
                var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PatientDbContext>>();
                using var context = dbContextFactory.CreateDbContext();
                // On laisse EF Core générer l'ID.
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

            var client = testFactory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/patients");

            // Assert
            response.EnsureSuccessStatusCode();
            var options = new JsonSerializerOptions();
            options.Converters.Add(new DateOnlyJsonConverter());
            var patients = await response.Content.ReadFromJsonAsync<IEnumerable<PatientViewModel>>(options);
            Assert.NotNull(patients);
            Assert.True(patients.Count() >= 2);

            // Nettoyage de la base
            await ClearDatabase(testFactory);
        }

        [Fact]
        public async Task CreatePatient_ReturnsOkAndCreatedPatient()
        {
            var testFactory = CreateFactoryWithUniqueDatabase();
            var client = testFactory.CreateClient();

            var newPatient = new PatientViewModel
            {
                FirstName = "Jane",
                LastName = "Doe",
                BirthDate = new DateOnly(1995, 5, 15),
                Gender = (int)Gender.Femme
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/patients", newPatient);

            // Assert
            response.EnsureSuccessStatusCode();
            var createdPatient = await response.Content.ReadFromJsonAsync<PatientViewModel>();
            Assert.NotNull(createdPatient);
            Assert.Equal("Jane", createdPatient.FirstName);
            Assert.Equal("Doe", createdPatient.LastName);

            await ClearDatabase(testFactory);
        }

        [Fact]
        public async Task GetPatient_ById_ReturnsOkAndPatient()
        {
            var testFactory = CreateFactoryWithUniqueDatabase();

            // Arrange : ajouter un patient dans la DB.
            using (var scope = testFactory.Services.CreateScope())
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

            var client = testFactory.CreateClient();
            // Supposons que le patient ajouté obtienne l'ID 1.
            var response = await client.GetAsync("/api/patients/1");

            response.EnsureSuccessStatusCode();

            var returnedPatient = await response.Content.ReadFromJsonAsync<PatientViewModel>();
            Assert.NotNull(returnedPatient);
            Assert.Equal("John", returnedPatient.FirstName);

            await ClearDatabase(testFactory);
        }

        [Fact]
        public async Task GetPatient_ByName_ReturnsOkAndPatient()
        {
            var testFactory = CreateFactoryWithUniqueDatabase();

            // Arrange : ajouter un patient.
            using (var scope = testFactory.Services.CreateScope())
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

            var client = testFactory.CreateClient();
            var response = await client.GetAsync("/api/patients/byname?firstname=Bob&lastname=Marley");

            response.EnsureSuccessStatusCode();
            var returnedPatient = await response.Content.ReadFromJsonAsync<PatientViewModel>();
            Assert.NotNull(returnedPatient);
            Assert.Equal("Bob", returnedPatient.FirstName);

            await ClearDatabase(testFactory);
        }

        [Fact]
        public async Task UpdatePatient_ReturnsOkAndUpdatedPatient()
        {
            var testFactory = CreateFactoryWithUniqueDatabase();

            // Arrange : ajouter un patient.
            using (var scope = testFactory.Services.CreateScope())
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

            var client = testFactory.CreateClient();

            // Préparer les données mises à jour.
            var updatedPatient = new PatientViewModel
            {
                FirstName = "Charlie",
                LastName = "Brown",
                BirthDate = new DateOnly(1985, 3, 3),
                Gender = (int)Gender.Homme,
                Address = "New Address"
            };

            // Act : appel à PUT /api/patients/1
            var putResponse = await client.PutAsJsonAsync("/api/patients/2", updatedPatient);
            putResponse.EnsureSuccessStatusCode();

            // Vérifier via GET
            var getResponse = await client.GetAsync("/api/patients/2");
            getResponse.EnsureSuccessStatusCode();

            var returnedPatient = await getResponse.Content.ReadFromJsonAsync<PatientViewModel>();
            Assert.NotNull(returnedPatient);
            Assert.Equal("New Address", returnedPatient.Address);

            await ClearDatabase(testFactory);
        }

        [Fact]
        public async Task DeletePatient_ReturnsNoContent()
        {
            var testFactory = CreateFactoryWithUniqueDatabase();

            // Arrange : ajouter un patient.
            using (var scope = testFactory.Services.CreateScope())
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

            var client = testFactory.CreateClient();

            // Act : appel à DELETE /api/patients/1
            var deleteResponse = await client.DeleteAsync("/api/patients/1");
            Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Vérifier que le patient n'est plus disponible.
            var getResponse = await client.GetAsync("/api/patients/1");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);

            await ClearDatabase(testFactory);
        }
    }
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string Format = "yyyy-MM-dd";

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? s = reader.GetString();
            if (s == null)
            {
                throw new JsonException("Unable to convert null to DateOnly.");
            }
            if (DateOnly.TryParseExact(s, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
            {
                return date;
            }
            throw new JsonException($"Unable to convert \"{s}\" to DateOnly.");
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
        }
    }
}
