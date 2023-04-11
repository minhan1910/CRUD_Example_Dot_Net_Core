using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO.Persons;
using ServiceContracts.Enums;
using ServiceContracts.ReflectUtils;
using Services.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class PersonService : IPersonService
    {
        private readonly PersonsDbContext _db;
        private readonly ICountriesService _countriesService;

        public PersonService(PersonsDbContext personsDbContext, 
                             ICountriesService countriesService, 
                             bool initialize = false)
        {
            _db = personsDbContext; 
            _countriesService = countriesService;
        }

        public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
        {
            if (personAddRequest is null)
            {
                throw new ArgumentNullException(nameof(personAddRequest));  
            }

            // Model validations
            ValidationHelper.ModelValidation(personAddRequest);

            // Covert 
            Person personToAdd = personAddRequest.ToPerson();
            
            _db.Persons.Add(personToAdd);
            await _db.SaveChangesAsync();

            //_db.sp_InsertPerson(personToAdd); 

            return personToAdd.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {

            var people = await _db.Persons.Include("Country").ToListAsync();

            return people.Select(person => person.ToPersonResponse()).ToList(); 

            // SELECT * from Persons - before using user-fefined method, you must query in Db
            //return _db.sp_GetAllPersons().ToList()
            //    .Select(person => ConvertPersonToPersonResponse(person)).ToList(); // 
        }

        public async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
        {
            if (personID is null)
            {
                return null;
            }

            Person? person = await _db.Persons.Include("Country")
                                        .FirstOrDefaultAsync(person => person.PersonID.Equals(personID));

            if (person is null)
            {
                return null;
            }

            return person.ToPersonResponse();
        }
            
        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {

            List<PersonResponse> allPersons = await GetAllPersons();
            List<PersonResponse> matchingPersons = allPersons;

            if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString))
            {
                return matchingPersons;
            }

            switch (searchBy)
            {
                case nameof(Person.PersonName):

                    matchingPersons = allPersons.Where(person => !string.IsNullOrEmpty(person.PersonName)) // có thể cache để lọc ra cho dễ
                                                .Where(validPerson => validPerson.PersonName!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                                                .ToList();
                    break;

                case nameof(Person.Email):

                    // nếu dùng như vậy phải check matchingPersons.Any hay không ?
                    matchingPersons = allPersons.Where(person => !string.IsNullOrEmpty(person.Email)) // có thể cache để lọc ra cho dễ
                                                .Where(validPerson => validPerson.Email!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                                                .ToList();

                    break;

                case nameof(Person.DateOfBirth):

                    matchingPersons = allPersons.Where(person => person.DateOfBirth is not null) // có thể cache để lọc ra cho dễ
                                                .Where(validPerson => ConvertDateOfBirthIs_dd_MMMM_yyyy(validPerson).Contains(searchString, StringComparison.OrdinalIgnoreCase))
                                                .ToList();

                    break;

                case nameof(Person.Gender):

                    matchingPersons = allPersons.Where(person => !string.IsNullOrEmpty(person.Gender)) 
                                                .Where(validPerson => validPerson.Gender!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                                                .ToList();

                    break;

                case nameof(Person.CountryID):

                    matchingPersons = allPersons.Where(person => !string.IsNullOrEmpty(person.Country))
                                                .Where(validPerson => validPerson.Country!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                                                .ToList();

                    break;

                case nameof(Person.Address):

                    matchingPersons = allPersons.Where(person => !string.IsNullOrEmpty(person.Address))
                                                .Where(validPerson => validPerson.Address!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                                                .ToList();

                    break;

                default: 
                    matchingPersons = allPersons; 
                    break;
            }

            return matchingPersons;
        }

        private string ConvertDateOfBirthIs_dd_MMMM_yyyy(PersonResponse validPerson)
        {
            return validPerson.DateOfBirth!.Value.ToString("dd MMMM yyyy");
        }

        public async Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return allPersons;
            }

            List<PersonResponse> sortedAllPerons = allPersons;

            //  Có thể dùng reflection hoặc switch để giải quyết vấn đề sortByFields

            // Cách 1 dùng reflection với lại OrderBy nó sẽ trả về giá trị để cần sort
            if (sortOrder.Equals(SortOrderOptions.DESC))
            {
                sortedAllPerons = allPersons.OrderByDescending(p => ReflectionUtils.GetPropertyValue(p, sortBy))
                                            .ToList();  
            } else
            {
                sortedAllPerons = allPersons.OrderBy(p => ReflectionUtils.GetPropertyValue(p, sortBy))
                                           .ToList();
            }

            // cách 2 dùng return (sortBy, sortOrderOptions) switch { (nameof(..., ...) => ) }

            return sortedAllPerons;
        }

        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            if (personUpdateRequest is null)
            {
                throw new ArgumentNullException(nameof(Person));
            }

            // Validation all fields
            ValidationHelper.ModelValidation(personUpdateRequest);

            // get matching person object to update
            Person? matchingPerson = await _db.Persons.FirstOrDefaultAsync(p => p.PersonID == personUpdateRequest.PersonID);

            if (matchingPerson is null)
            {
                throw new ArgumentException("Given person id doesn't exist"); 
            }            

            // update all details
            matchingPerson.PersonName = personUpdateRequest.PersonName;
            matchingPerson.Email = personUpdateRequest.Email;
            matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth!.Value;
            matchingPerson.Gender = personUpdateRequest.Gender.ToString();
            matchingPerson.CountryID = personUpdateRequest.CountryID;
            matchingPerson.Address = personUpdateRequest.Address;
            matchingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

            await _db.SaveChangesAsync(); // UPDATE

            return matchingPerson.ToPersonResponse();
        }

        public async Task<bool> DeletePerson(Guid? personID)
        {
            if (personID is null)
            {
                throw new ArgumentNullException(nameof(personID));
            }

            Person? person = await _db.Persons.FirstOrDefaultAsync(p => p.PersonID == personID);
        
            if (person is null)
            {
                return false;
            }

            // Just remove one and except for duplication personID
            _db.Persons.Remove(_db.Persons.First(p => p.PersonID == personID));
            await _db.SaveChangesAsync();

            return true;
        }
    }
}
