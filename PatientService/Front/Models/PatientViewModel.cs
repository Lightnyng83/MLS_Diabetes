namespace Front.Models
{
    public class PatientViewModel
    {

        public string LastName { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public DateOnly BirthDate { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public int Gender { get; set; }
    }
}
