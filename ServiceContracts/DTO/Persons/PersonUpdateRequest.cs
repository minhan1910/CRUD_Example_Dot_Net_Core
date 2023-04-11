using Entities;
using ServiceContracts.Enums;
using ServiceContracts.ReflectUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTO.Persons
{
    /// <summary>
    /// Represents DTO class that is used as 
    /// return type of most methods of Persons Service
    /// </summary>
    public class PersonUpdateRequest
    {
        [Required(ErrorMessage = "Person ID can't be blank")]
        public Guid? PersonID { get; set; }

        [Required(ErrorMessage = "Person name can't be blank")]
        public string? PersonName { get; set; }

        [Required(ErrorMessage = "Email can't be blank")]
        [EmailAddress(ErrorMessage = "Email value should be a valid email")]
        [DataType(DataType.EmailAddress)]   
        public string? Email { get; set; }

        [DataType(DataType.Date)]   
        public DateTime? DateOfBirth { get; set; }
        public GenderOptions Gender { get; set; }
        public Guid? CountryID { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }
        public uint? Age { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"Person ID: {PersonID}; Person Name: {PersonName}; Email: {Email}; Date of birth: {DateOfBirth?.ToString("dd MMM yyyy")}; Gender: {Gender}; Country ID: {CountryID}; Country: {Country}; Address: {Address}; Receive New Letters: {ReceiveNewsLetters}";
        }

        public Person ToPerson()
        {
            return new Person()
            {
                PersonID = PersonID,
                PersonName = PersonName,
                Email = Email,
                DateOfBirth = DateOfBirth!.Value,
                Gender = Gender!.ToString(),
                CountryID = CountryID,
                Address = Address,
                ReceiveNewsLetters = ReceiveNewsLetters
            };
        }
    }
}
