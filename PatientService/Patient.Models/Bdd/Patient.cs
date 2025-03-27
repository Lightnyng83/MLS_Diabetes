namespace Patient.Models.Bdd
{
    public class Patient
    {
        public int IdPatient { get; set; }

        public string LastName { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public DateOnly BirthDate { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public int Gender { get; set; }
    }
    public enum Gender
    {
        Femme = 0,
        Homme = 1
    }
}
