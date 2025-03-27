using Microsoft.EntityFrameworkCore;

namespace Patient.Data.Data
{
    public partial class PatientDbContext : DbContext
    {
        //Scaffold-DbContext 'Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=PatientAPI' Microsoft.EntityFrameworkCore.SqlServer -OutputDir ../Patient.Models -ContextDir ../Patient.Data -Context PatientDbContext pour la génération des modèles

        #region Constructor

        public PatientDbContext()
        {
        }

        public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options)
        {
        }
        #endregion
        public virtual DbSet<Models.Bdd.Patient> Patients { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PatientAPI",
                    x => x.UseNetTopologySuite());
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Bdd.Patient>(entity =>
            {
                entity.HasKey(e => e.IdPatient);

                entity.ToTable("Patient");

                entity.Property(e => e.Address)
                    .HasMaxLength(255);
                entity.Property(e => e.FirstName)
                    .HasMaxLength(100);
                entity.Property(e => e.LastName)
                    .HasMaxLength(100);
                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20);
            });

            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
