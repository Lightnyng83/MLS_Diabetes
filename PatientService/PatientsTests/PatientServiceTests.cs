using AutoMapper;
using Moq;
using Patient.Core.Service.PatientService;
using Patient.Data.Repository.PatientRepository;
using Patient.Models.Bdd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsTests
{
    public class PatientServiceTests
    {
        private readonly Mock<IPatientRepository> _patientRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly PatientService _service;

        public PatientServiceTests()
        {
            _patientRepositoryMock = new Mock<IPatientRepository>();
            _mapperMock = new Mock<IMapper>();
            _service = new PatientService(_patientRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetPatients_ReturnsMappedPatients()
        {
            // Arrange
            var patients = new List<Patient.Models.Bdd.Patient>
            {
                new Patient.Models.Bdd.Patient { IdPatient = 1, FirstName = "John", LastName = "Doe", BirthDate = new DateOnly(1990, 1, 1), Gender = (int)Gender.Homme },
                new Patient.Models.Bdd.Patient { IdPatient = 2, FirstName = "Jane", LastName = "Doe", BirthDate = new DateOnly(1992, 2, 2), Gender = (int)Gender.Femme }
            };

            _patientRepositoryMock.Setup(repo => repo.GetPatients())
                                  .ReturnsAsync(patients);

            var patientViewModels = new List<PatientViewModel>
            {
                new PatientViewModel { FirstName = "John", LastName = "Doe", BirthDate = new DateOnly(1990, 1, 1), Gender = (int)Gender.Homme },
                new PatientViewModel { FirstName = "Jane", LastName = "Doe", BirthDate = new DateOnly(1992, 2, 2), Gender = (int)Gender.Femme }
            };

            _mapperMock.Setup(m => m.Map<IEnumerable<PatientViewModel>>(patients))
                       .Returns(patientViewModels);

            // Act
            var result = await _service.GetPatients();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("John", result.First().FirstName);
            _patientRepositoryMock.Verify(repo => repo.GetPatients(), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<PatientViewModel>>(patients), Times.Once);
        }

        [Fact]
        public async Task GetPatient_ById_ReturnsMappedPatient()
        {
            // Arrange
            int id = 1;
            var patient = new Patient.Models.Bdd.Patient
            {
                IdPatient = id,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateOnly(1990, 1, 1),
                Gender = (int)Gender.Homme
            };
            _patientRepositoryMock.Setup(repo => repo.GetPatient(id))
                                  .ReturnsAsync(patient);

            var viewModel = new PatientViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateOnly(1990, 1, 1),
                Gender = (int)Gender.Homme
            };
            _mapperMock.Setup(m => m.Map<PatientViewModel>(patient))
                       .Returns(viewModel);

            // Act
            var result = await _service.GetPatient(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
            _patientRepositoryMock.Verify(repo => repo.GetPatient(id), Times.Once);
            _mapperMock.Verify(m => m.Map<PatientViewModel>(patient), Times.Once);
        }

        [Fact]
        public async Task GetPatient_ByName_ReturnsMappedPatient()
        {
            // Arrange
            string firstName = "John";
            string lastName = "Doe";
            var patient = new Patient.Models.Bdd.Patient
            {
                IdPatient = 1,
                FirstName = firstName,
                LastName = lastName,
                BirthDate = new DateOnly(1990, 1, 1),
                Gender = (int)Gender.Homme
            };
            _patientRepositoryMock.Setup(repo => repo.GetPatient(firstName, lastName))
                                  .ReturnsAsync(patient);

            var viewModel = new PatientViewModel
            {
                FirstName = firstName,
                LastName = lastName,
                BirthDate = new DateOnly(1990, 1, 1),
                Gender = (int)Gender.Homme
            };
            _mapperMock.Setup(m => m.Map<PatientViewModel>(patient))
                       .Returns(viewModel);

            // Act
            var result = await _service.GetPatient(firstName, lastName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Doe", result.LastName);
            _patientRepositoryMock.Verify(repo => repo.GetPatient(firstName, lastName), Times.Once);
            _mapperMock.Verify(m => m.Map<PatientViewModel>(patient), Times.Once);
        }

        [Fact]
        public async Task AddPatient_CallsRepositoryAndReturnsMappedPatient()
        {
            // Arrange
            var viewModel = new PatientViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateOnly(1990, 1, 1),
                Gender = (int)Gender.Homme
            };

            var patientEntity = new Patient.Models.Bdd.Patient
            {
                IdPatient = 1, // utilisé uniquement dans l'entité côté serveur
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateOnly(1990, 1, 1),
                Gender = (int)Gender.Homme
            };

            _mapperMock.Setup(m => m.Map<Patient.Models.Bdd.Patient>(viewModel))
                       .Returns(patientEntity);

            _patientRepositoryMock.Setup(repo => repo.AddPatient(patientEntity))
                                  .ReturnsAsync(patientEntity);

            var mappedViewModel = new PatientViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateOnly(1990, 1, 1),
                Gender = (int)Gender.Homme
            };

            _mapperMock.Setup(m => m.Map<PatientViewModel>(patientEntity))
                       .Returns(mappedViewModel);

            // Act
            var result = await _service.AddPatient(viewModel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            _mapperMock.Verify(m => m.Map<Patient.Models.Bdd.Patient>(viewModel), Times.Once);
            _patientRepositoryMock.Verify(repo => repo.AddPatient(patientEntity), Times.Once);
            _mapperMock.Verify(m => m.Map<PatientViewModel>(patientEntity), Times.Once);
        }

        [Fact]
        public async Task UpdatePatient_CallsRepositoryAndReturnsMappedPatient()
        {
            // Arrange
            var viewModel = new PatientViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateOnly(1990, 1, 1),
                Gender = (int)Gender.Homme,
                Address = "Old Address"
            };

            var patientEntity = new Patient.Models.Bdd.Patient
            {
                IdPatient = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateOnly(1990, 1, 1),
                Gender = (int)Gender.Homme,
                Address = "Old Address"
            };

            _mapperMock.Setup(m => m.Map<Patient.Models.Bdd.Patient>(viewModel))
                       .Returns(patientEntity);

            _patientRepositoryMock.Setup(repo => repo.UpdatePatient(patientEntity))
                                  .ReturnsAsync(patientEntity);

            _mapperMock.Setup(m => m.Map<PatientViewModel>(patientEntity))
                       .Returns(viewModel);

            // Act
            var result = await _service.UpdatePatient(viewModel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            _mapperMock.Verify(m => m.Map<Patient.Models.Bdd.Patient>(viewModel), Times.Once);
            _patientRepositoryMock.Verify(repo => repo.UpdatePatient(patientEntity), Times.Once);
            _mapperMock.Verify(m => m.Map<PatientViewModel>(patientEntity), Times.Once);
        }

        [Fact]
        public async Task DeletePatient_CallsRepositoryDeletePatient()
        {
            // Arrange
            int patientId = 1;
            _patientRepositoryMock.Setup(repo => repo.DeletePatient(patientId))
                                  .Returns(Task.CompletedTask);

            // Act
            await _service.DeletePatient(patientId);

            // Assert
            _patientRepositoryMock.Verify(repo => repo.DeletePatient(patientId), Times.Once);
        }
    }
}
