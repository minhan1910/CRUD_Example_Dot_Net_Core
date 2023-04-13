using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
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
        private readonly IPersonsRepository _personsRepository;     

        public PersonService(IPersonsRepository personsRepository)
        {
            _personsRepository = personsRepository; 
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

            await _personsRepository.AddPerson(personToAdd);           

            //_db.sp_InsertPerson(personToAdd); 

            return personToAdd.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {

            var people = await _personsRepository.GetAllPersons();

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

            Person? person = await _personsRepository.GetPersonByPersonId(personID.Value);

            if (person is null)
            {
                return null;
            }

            return person.ToPersonResponse();
        }
            
        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            //List<PersonResponse> allPersons = await GetAllPersons();            

            //if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString))
            //{
            //    return allPersons;
            //}

            List<Person> filteredPersons = searchBy switch
            {
                nameof(Person.PersonName) => await _personsRepository.GetFilteredPersons(p => p.PersonName!.Contains(searchString)),
                nameof(Person.Email) => await _personsRepository.GetFilteredPersons(p => p.Email!.Contains(searchString)),
                nameof(Person.DateOfBirth) => await _personsRepository.GetFilteredPersons(p => p.DateOfBirth!.Value.ToString("dd MMMM yyyy")!.Contains(searchString)),
                nameof(Person.CountryID) => await _personsRepository.GetFilteredPersons(p => p.Country!.CountryName!.Contains(searchString)),
                nameof(Person.Address) => await _personsRepository.GetFilteredPersons(p => p.Address!.Contains(searchString)),
                _ => await _personsRepository.GetAllPersons()
            };            

            return filteredPersons.Select(temp => temp.ToPersonResponse())
                             .ToList();
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
            Person? matchingPerson = await _personsRepository.GetPersonByPersonId(personUpdateRequest.PersonID!.Value);

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

            //await _personsRepository.SaveChangesAsync(); // UPDATE

            await _personsRepository.UpdatePerson(matchingPerson);  

            return matchingPerson.ToPersonResponse();
        }

        public async Task<bool> DeletePerson(Guid? personID)
        {
            if (personID is null)
            {
                throw new ArgumentNullException(nameof(personID));
            }

            Person? person = await _personsRepository.GetPersonByPersonId(personID.Value);
        
            if (person is null)
            {
                return false;
            }

            // Just remove one and except for duplication personID
            //_personsRepository.Persons.Remove(_personsRepository.Persons.First(p => p.PersonID == personID));
            //await _personsRepository.SaveChangesAsync();

            await _personsRepository.DeletePersonByPersonID(person.PersonID!.Value);

            return true;
        }
    }
}
