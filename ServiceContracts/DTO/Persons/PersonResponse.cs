using Entities;
using ServiceContracts.Enums;
using ServiceContracts.ReflectUtils;
using System;
using System.Collections.Generic;
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
    public class PersonResponse
    {
        public Guid? PersonID { get; set; }
        public string? PersonName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public Guid? CountryID { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }
        public uint? Age { get; set; }

        /// <summary>
        /// Compares the current object data with the parameter object
        /// </summary>
        /// <param name="obj">The PersonResponse obj</param>
        /// <returns>True or False, indicating whether all person details are matched
        /// with the specified parameter object</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != typeof(PersonResponse))
            {
                return false;
            }

            PersonResponse otherPerson = (PersonResponse)obj;

            return ReflectionUtils.CompareTwoObject(this, otherPerson);
        }        

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"Person ID: {PersonID}; Person Name: {PersonName}; Email: {Email}; Date of birth: {DateOfBirth?.ToString("dd MMM yyyy")}; Gender: {Gender}; Country ID: {CountryID}; Country: {Country}; Address: {Address}; Receive New Letters: {ReceiveNewsLetters}";
        }

        public PersonUpdateRequest ToPersonUpdateRequest()
        {
            return new PersonUpdateRequest()
            {
                PersonID = PersonID,
                PersonName = PersonName,
                Email = Email,
                DateOfBirth = DateOfBirth,
                Gender = (GenderOptions)Enum.Parse(typeof(GenderOptions), Gender!, true),
                CountryID = CountryID,
                Address = Address,
                ReceiveNewsLetters = ReceiveNewsLetters,
            };
        }
    }

    public static class PersonExtensions
    {
        /// <summary>
        /// An extension method to convert an object of Person class 
        /// into PersonResponse class
        /// </summary>
        /// <param name="person">The Person object to convert</param>
        /// <returns>Returns the converted PersonResponse object</returns>
        public static PersonResponse ToPersonResponse(this Person person)
        {
            // Person => PersonResponse
            PersonResponse personResponse = new PersonResponse()
            {
                PersonID = person.PersonID,
                PersonName = person.PersonName,
                Email = person.Email,
                DateOfBirth = person.DateOfBirth,
                Address = person.Address,
                CountryID = person.CountryID,
                Gender = person.Gender,
                Age = person.DateOfBirth.HasValue ? (uint)Math.Round((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365.25) : null,
                ReceiveNewsLetters = person.ReceiveNewsLetters,
            };

            return personResponse;
        }
    }
}
