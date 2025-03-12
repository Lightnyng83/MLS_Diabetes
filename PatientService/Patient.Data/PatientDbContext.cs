using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Patient.Data
{
    public partial class PatientDbContext : DbContext
    {
        #region Constructor

        public PatientDbContext()
        {
        }
        public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options)
        {
        }

        #endregion

    }
}
