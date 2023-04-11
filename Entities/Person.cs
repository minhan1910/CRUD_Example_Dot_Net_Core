using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{
    /// <summary>
    /// Person domain model class
    /// </summary>
    public class Person
    {
        [Key]
        public Guid? PersonID { get; set; }

        // nvarchar(40)
        [StringLength(40)]
        public string? PersonName { get; set; }

        [StringLength(40)]
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        // unique_identifier
        public Guid? CountryID { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        // bit
        public bool ReceiveNewsLetters { get; set; }

        // [Column("TaxIdentificationNumber", TypeName= "varchar(8)")]
        public string? TIN { get; set; }

        [ForeignKey("CountryID")] 
        public virtual Country? Country { get; set; } // virtual for common convention

        public bool IsNullOrEmptyPersonName() => string.IsNullOrEmpty(PersonName);         


    }
}
