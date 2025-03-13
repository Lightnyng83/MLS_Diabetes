using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patient.Models.Bdd
{
    public class PatientCreateViewModel
    {

        public string LastName { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public DateOnly BirthDate { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public int Gender { get; set; }
    }

    
}
