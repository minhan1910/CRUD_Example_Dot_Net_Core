using Entities;
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
        private readonly List<Person> _people;
        private readonly ICountriesService _countriesService;

        public PersonService(ICountriesService countriesService, bool initialize = true)
        {
            _people = new List<Person>();
            _countriesService = countriesService;

            if (initialize)
            {
                // https://www.mockaroo.com/ to mock data
                _people.Add(new Person() { PersonID = Guid.Parse("8082ED0C-396D-4162-AD1D-29A13F929824"), PersonName = "Aguste", Email = "aleddy0@booking.com", DateOfBirth = DateTime.Parse("1993-01-02"), Gender = "Male", Address = "0858 Novick Terrace", ReceiveNewsLetters = false, CountryID = Guid.Parse("6DB09C01-2555-4C0F-9F76-05573DD117E3") });

                _people.Add(new Person() { PersonID = Guid.Parse("06D15BAD-52F4-498E-B478-ACAD847ABFAA"), PersonName = "Jasmina", Email = "jsyddie1@miibeian.gov.cn", DateOfBirth = DateTime.Parse("1991-06-24"), Gender = "Female", Address = "0742 Fieldstone Lane", ReceiveNewsLetters = true, CountryID = Guid.Parse("49E28576-CAD8-428E-B7D6-819DD76F3B3C") });

                _people.Add(new Person() { PersonID = Guid.Parse("D3EA677A-0F5B-41EA-8FEF-EA2FC41900FD"), PersonName = "Kendall", Email = "khaquard2@arstechnica.com", DateOfBirth = DateTime.Parse("1993-08-13"), Gender = "Male", Address = "7050 Pawling Alley", ReceiveNewsLetters = false, CountryID = Guid.Parse("A375ACFA-54D1-41C0-87D4-20E29A6A630A") });
                    
                _people.Add(new Person() { PersonID = Guid.Parse("89452EDB-BF8C-4283-9BA4-8259FD4A7A76"), PersonName = "Kilian", Email = "kaizikowitz3@joomla.org", DateOfBirth = DateTime.Parse("1991-06-17"), Gender = "Male", Address = "233 Buhler Junction", ReceiveNewsLetters = true, CountryID = Guid.Parse("F6BB7888-FAA7-4A4F-B8BB-903E793E8C98") });

                _people.Add(new Person() { PersonID = Guid.Parse("F5BD5979-1DC1-432C-B1F1-DB5BCCB0E56D"), PersonName = "Dulcinea", Email = "dbus4@pbs.org", DateOfBirth = DateTime.Parse("1996-09-02"), Gender = "Female", Address = "56 Sundown Point", ReceiveNewsLetters = false, CountryID = Guid.Parse("1E2CDD04-17BE-4D4A-9846-A22B7C823079") });

                _people.Add(new Person() { PersonID = Guid.Parse("A795E22D-FAED-42F0-B134-F3B89B8683E5"), PersonName = "Corabelle", Email = "cadams5@t-online.de", DateOfBirth = DateTime.Parse("1993-10-23"), Gender = "Female", Address = "4489 Hazelcrest Place", ReceiveNewsLetters = false, CountryID = Guid.Parse("15889048-AF93-412C-B8F3-22103E943A6D") });

                _people.Add(new Person() { PersonID = Guid.Parse("3C12D8E8-3C1C-4F57-B6A4-C8CAAC893D7A"), PersonName = "Faydra", Email = "fbischof6@boston.com", DateOfBirth = DateTime.Parse("1996-02-14"), Gender = "Female", Address = "2010 Farragut Pass", ReceiveNewsLetters = true, CountryID = Guid.Parse("1E2CDD04-17BE-4D4A-9846-A22B7C823079") });

                _people.Add(new Person() { PersonID = Guid.Parse("7B75097B-BFF2-459F-8EA8-63742BBD7AFB"), PersonName = "Oby", Email = "oclutheram7@foxnews.com", DateOfBirth = DateTime.Parse("1992-05-31"), Gender = "Male", Address = "2 Fallview Plaza", ReceiveNewsLetters = false, CountryID = Guid.Parse("1E2CDD04-17BE-4D4A-9846-A22B7C823079") });

                _people.Add(new Person() { PersonID = Guid.Parse("6717C42D-16EC-4F15-80D8-4C7413E250CB"), PersonName = "Seumas", Email = "ssimonitto8@biglobe.ne.jp", DateOfBirth = DateTime.Parse("1999-02-02"), Gender = "Male", Address = "76779 Norway Maple Crossing", ReceiveNewsLetters = false, CountryID = Guid.Parse("1E2CDD04-17BE-4D4A-9846-A22B7C823079") });

                _people.Add(new Person() { PersonID = Guid.Parse("6E789C86-C8A6-4F18-821C-2ABDB2E95982"), PersonName = "Freemon", Email = "faugustin9@vimeo.com", DateOfBirth = DateTime.Parse("1996-04-27"), Gender = "Male", Address = "8754 Becker Street", ReceiveNewsLetters = false, CountryID = Guid.Parse("1E2CDD04-17BE-4D4A-9846-A22B7C823079") });
            }
        }

        private PersonResponse ConvertPersonToPersonResponse(Person person)
        {
            PersonResponse personResponse = person.ToPersonResponse();

            personResponse.Country = _countriesService.GetCountryByCountryID(person.CountryID)
                                                      ?.CountryName;

            return personResponse;
        }

        public PersonResponse AddPerson(PersonAddRequest? personAddRequest)
        {
            if (personAddRequest is null)
            {
                throw new ArgumentNullException(nameof(personAddRequest));  
            }

            // Model validations
            ValidationHelper.ModelValidation(personAddRequest);

            // Covert 
            Person personToAdd = personAddRequest.ToPerson();
            
            _people.Add(personToAdd);  

            return ConvertPersonToPersonResponse(personToAdd);
        }

        public List<PersonResponse> GetAllPersons()
        {
            
            return _people.Select(person => ConvertPersonToPersonResponse(person))
                         .ToList(); 
        }

        public PersonResponse? GetPersonByPersonID(Guid? personID)
        {
            if (personID is null)
            {
                return null;
            }

            Person? person = _people.FirstOrDefault(person => person.PersonID.Equals(personID));

            if (person is null)
            {
                return null;
            }

            return ConvertPersonToPersonResponse(person);
        }

        public List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString)
        {

            List<PersonResponse> allPersons = GetAllPersons();
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

        public List<PersonResponse> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
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

        public PersonResponse UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            if (personUpdateRequest is null)
            {
                throw new ArgumentNullException(nameof(Person));
            }

            // Validation all fields
            ValidationHelper.ModelValidation(personUpdateRequest);

            // get matching person object to update
            Person? matchingPerson = _people.FirstOrDefault(p => p.PersonID == personUpdateRequest.PersonID);

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

            return ConvertPersonToPersonResponse(matchingPerson);
        }

        public bool DeletePerson(Guid? personID)
        {
            if (personID is null)
            {
                throw new ArgumentNullException(nameof(personID));
            }

            Person? person = _people.FirstOrDefault(p => p.PersonID == personID);
        
            if (person is null)
            {
                return false;
            }

            // Just remove one and except for duplication personID
            _people.RemoveAll(p => p.PersonID == personID);

            return true;
        }
    }
}
