using ServiceContracts;
using ServiceContracts.DTO.Persons;
using Services;
using ServiceContracts.Enums;
using ServiceContracts.DTO.Countries;
using Xunit.Abstractions;
using Entities;
using System.Text;
using ServiceContracts.ReflectUtils;
using Microsoft.EntityFrameworkCore;

namespace CRUDTests {
    public class PersonServiceTest
    {
        private readonly IPersonService _personService;
        private readonly ICountriesService _countriesService;
        private readonly ITestOutputHelper _testOutputHelper;
        public PersonServiceTest(ITestOutputHelper testOutputHelper)
        {
            _countriesService = new CountriesService(
                                    new PersonsDbContext(
                                        new DbContextOptionsBuilder<PersonsDbContext>().Options));
            _personService = new PersonService(
                                new PersonsDbContext(
                                    new DbContextOptionsBuilder<PersonsDbContext>().Options) , 
                             _countriesService);
            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson

        // When we supply null value as PersonAddRequest,
        // it should be throw ArgumentNullException
        [Fact]
        public async Task AddPerson_NullPerson()
        {
            // Arrange
            PersonAddRequest? personAddRequest = null;

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>( async () =>
            {
                //Act
                await _personService.AddPerson(personAddRequest);
            });
        }

        // When we supply null value as PersonName,
        // it should be throw ArgumentException
        [Fact]
        public async Task AddPerson_NullPersonName()
        {
            // Arrange
            PersonAddRequest? personAddRequest = new PersonAddRequest()
            {
                PersonName = null
            };

            // Assert
            await Assert.ThrowsAsync<ArgumentException>( async () =>
            {
                await _personService.AddPerson(personAddRequest);
            });
        }

        /*
            When we supply proper person details, it should insert the person
            into the persons list; and it should return an object of PersonResponse, which
            includes with the newly generated PersonID.
        */
        [Fact]
        public async Task AddPerson_ProperPersonDetails()
        {

            // Arrange
            CountryResponse countryResponse = await _countriesService.AddCountry(new CountryAddRequest() { CountryName = "Viet Nam"});

            PersonAddRequest? personAddRequest = new()
            {
                PersonName = "Person Test",
                Email = "example@gmail.com",
                Address = "sample address",
                CountryID = countryResponse.CountryID,
                Gender = GenderOptions.MALE,
                DateOfBirth = DateTime.Parse("2000-01-01"),
                ReceiveNewsLetters = true,
            };

            // Acts
            PersonResponse personResponseFromAdd = await _personService.AddPerson(personAddRequest);

            List<PersonResponse> persons_From_GetAllPersons = await _personService.GetAllPersons();

            // Assert
            Assert.True(personResponseFromAdd.PersonID != Guid.Empty);
            Assert.Contains(personResponseFromAdd, persons_From_GetAllPersons);
        }

        #endregion

        #region GetPersonByPersonID

        // If we supply as PersonID, it should return null as PersonResponse
        [Fact]
        public async Task GetPersonByPersonID_NullPersonID()
        {
            // Arrange
            Guid? nullPersonID = null;

            // Acts
            PersonResponse? personResponseFromGet = await _personService.GetPersonByPersonID(nullPersonID);
        
            // Assert
            Assert.Null(personResponseFromGet);
        }

        // If we supply a valid person id, it should return the valid person details
        // as PersonResponse object

        [Fact]
        public async Task GetPersonByPersonID_WithPersonID()
        {
            // Arrange
            CountryAddRequest countryRequest = new CountryAddRequest()
            {
                CountryName = "Canada"
            };

            CountryResponse countryResponse = await _countriesService.AddCountry(countryRequest);

            PersonAddRequest? personAddRequest = new()
            {
                PersonName = "Person Test",
                Email = "example@gmail.com",
                Address = "sample address",
                CountryID = countryResponse.CountryID,
                Gender = GenderOptions.MALE,
                DateOfBirth = DateTime.Parse("2000-01-01"),
                ReceiveNewsLetters = true,
            };

            PersonResponse personResponseFromAdd = await _personService.AddPerson(personAddRequest);

            PersonResponse? personResponseFromGet = await _personService.GetPersonByPersonID(personResponseFromAdd.PersonID);

            // Assert
            Assert.Equal(personResponseFromAdd, personResponseFromGet);
        }

        #endregion

        #region GetAllPersons

        // The GetAllPersons() should return an empty list by default
        [Fact]
        public async Task GetAllPersons_EmptyList()
        {
            // Act
            List<PersonResponse> people = await _personService.GetAllPersons();

            // Assert
            Assert.Empty(people);
        }

        // First, we will add few persons; and then when we call GetAllPersons(),
        // it should return the same persons that were added
        [Fact]
        public async Task GetAllPersons_AddFewPersons()
        {
            List<CountryAddRequest> countryAddRequests = new()
            {
                new CountryAddRequest() { CountryName = "Viet Nam"},
                new CountryAddRequest() { CountryName = "Finland"},
            };

            List<PersonAddRequest> personAddRequests = new()
            {
                new PersonAddRequest()
                {
                    PersonName = "Person Test 1",
                    Email = "example1@gmail.com",
                    Address = "sample address 1",
                    Gender = GenderOptions.FEMALE,
                    DateOfBirth = DateTime.Parse("2000-02-01"),
                    ReceiveNewsLetters = true,
                },
                new PersonAddRequest()
                {
                    PersonName = "Person Test 2",
                    Email = "example2@gmail.com",
                    Address = "sample address 2",
                    Gender = GenderOptions.MALE,
                    DateOfBirth = DateTime.Parse("2000-01-01"),
                    ReceiveNewsLetters = false,
                }
            };

            // Add Country and Supply Country ID for each Person
            for (int i = 0; i < personAddRequests.Count; i++)
            {
                CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequests[i]);
                personAddRequests[i].CountryID = countryResponse.CountryID;
            }

            // Add Persons
            List<PersonResponse> personResponsesListFromAdd = new();

            foreach (PersonAddRequest personToAdd in personAddRequests)
            {
                PersonResponse personResponse = await _personService.AddPerson(personToAdd);
                personResponsesListFromAdd.Add(personResponse);
            }

            // print personResponseListFromAdd by testOutputHelper
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse personResponseFromAdd in personResponsesListFromAdd)
            {
                _testOutputHelper.WriteLine(personResponseFromAdd.ToString());
            }

            // Get All Persons
            List<PersonResponse> personResponsesListFromGet = await _personService.GetAllPersons();

            // print personResponseListFromAdd by testOutputHelper
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse personResponseFromGet in personResponsesListFromGet)
            {
                _testOutputHelper.WriteLine(personResponseFromGet.ToString());
            }

            // Assert
            Assert.True(personResponsesListFromAdd.Count == personResponsesListFromGet.Count);

            foreach (PersonResponse expectedPerson in personResponsesListFromAdd)
            {
                Assert.Contains(expectedPerson, personResponsesListFromGet);
            }
        }

        #endregion

        #region GetFilteredPersons

        // If the search text is empty and search by is "PersonName",
        // it should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText()
        {
            List<CountryAddRequest> countryAddRequests = new()
            {
                new CountryAddRequest() { CountryName = "Viet Nam"},
                new CountryAddRequest() { CountryName = "Finland"},
            };

            List<PersonAddRequest> personAddRequests = new()
            {
                new PersonAddRequest()
                {
                    PersonName = "Person Test 1",
                    Email = "example1@gmail.com",
                    Address = "sample address 1",
                    Gender = GenderOptions.FEMALE,
                    DateOfBirth = DateTime.Parse("2000-02-01"),
                    ReceiveNewsLetters = true,
                },
                new PersonAddRequest()
                {
                    PersonName = "Person Test 2",
                    Email = "example2@gmail.com",
                    Address = "sample address 2",
                    Gender = GenderOptions.MALE,
                    DateOfBirth = DateTime.Parse("2000-01-01"),
                    ReceiveNewsLetters = false,
                }
            };

            // Add Country and Supply Country ID for each Person
            for (int i = 0; i < personAddRequests.Count; i++)
            {
                CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequests[i]);
                personAddRequests[i].CountryID = countryResponse.CountryID;
            }

            // Add Persons
            List<PersonResponse> personResponsesListFromAdd = new();

            foreach (PersonAddRequest personToAdd in personAddRequests)
            {
                PersonResponse personResponse = await _personService.AddPerson(personToAdd);
                personResponsesListFromAdd.Add(personResponse);
            }

            // print personResponseListFromAdd by testOutputHelper
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse personResponseFromAdd in personResponsesListFromAdd)
            {
                _testOutputHelper.WriteLine(personResponseFromAdd.ToString());
            }

            // Get Filtered Persons
            List<PersonResponse> personResponsesListFromSearch = await _personService.GetFilteredPersons(nameof(Person.PersonName), string.Empty);

            // print personResponseListFromAdd by testOutputHelper
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse personResponseFromSearch in personResponsesListFromSearch)
            {
                _testOutputHelper.WriteLine(personResponseFromSearch.ToString());
            }

            // Assert
            Assert.True(personResponsesListFromAdd.Count == personResponsesListFromSearch.Count);

            foreach (PersonResponse expectedPerson in personResponsesListFromAdd)
            {
                Assert.Contains(expectedPerson, personResponsesListFromSearch);
            }
        }

        /*
            First we add few persons,; and then we will search based on person name
            with some searchg string. It should return the matching persons. 
         */   
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName()
        {
            const string searchString = "An";

            List<CountryAddRequest> countryAddRequests = new()
            {
                new CountryAddRequest() { CountryName = "Viet Nam"},
                new CountryAddRequest() { CountryName = "Finland"},
                new CountryAddRequest() { CountryName = "Campuchia"},
            };
            
            List<PersonAddRequest> personAddRequests = new()
            {
                new PersonAddRequest()
                {
                    PersonName = "Pnu",
                    Email = "example1@gmail.com",
                    Address = "sample address 1",
                    Gender = GenderOptions.FEMALE,
                    DateOfBirth = DateTime.Parse("2000-02-01"),
                    ReceiveNewsLetters = true,
                },
                new PersonAddRequest()
                {
                    PersonName = "Hung",
                    Email = "example2@gmail.com",
                    Address = "sample address 2",
                    Gender = GenderOptions.MALE,
                    DateOfBirth = DateTime.Parse("2000-01-01"),
                    ReceiveNewsLetters = false,
                },
                new PersonAddRequest()
                {
                    PersonName = "MinAn",
                    Email = "example2@gmail.com",
                    Address = "sample address 2",
                    Gender = GenderOptions.MALE,
                    DateOfBirth = DateTime.Parse("2000-03-01"),
                    ReceiveNewsLetters = false,
                }
            };

            // Add Country and Supply Country ID for each Person
            for (int i = 0; i < personAddRequests.Count; i++)
            {
                CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequests[i]);
                personAddRequests[i].CountryID = countryResponse.CountryID;
            }

            // Add Persons
            List<PersonResponse> personResponsesListFromAdd = new();

            foreach (PersonAddRequest personToAdd in personAddRequests)
            {
                PersonResponse personResponse = await _personService.AddPerson(personToAdd);
                personResponsesListFromAdd.Add(personResponse);
            }

            // print personResponseListFromAdd by testOutputHelper
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse personResponseFromAdd in personResponsesListFromAdd)
            {
                _testOutputHelper.WriteLine(personResponseFromAdd.ToString());
            }

            // Get Filtered Persons
            List<PersonResponse> personResponsesListFromSearch = await _personService.GetFilteredPersons(nameof(Person.PersonName), searchString);

            // print personResponseListFromAdd by testOutputHelper
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse personResponseFromSearch in personResponsesListFromSearch)
            {
                _testOutputHelper.WriteLine(personResponseFromSearch.ToString());
            }

            // Results - Assert
            PrintCharacter("-", 10, "RESULT");
                
            if (personResponsesListFromSearch.Any())
            {
                _testOutputHelper.WriteLine("Successful Testing !!");

                foreach (PersonResponse expectedPerson in personResponsesListFromAdd)
                {
                    var personName = expectedPerson.PersonName;

                    if (personName is not null)
                    {
                        if (personName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                        {
                            Assert.Contains(expectedPerson, personResponsesListFromSearch);
                        }
                    }
                }
            } else
            {
                /*There are not any elements in personResponsesListFromSearch*/
                
                _testOutputHelper.WriteLine($"There are not elements by searchString: {searchString}");
                Assert.True(!personResponsesListFromSearch.Any());
            }

            PrintCharacter("-", 10 + "RESULT".Count());
        }

        #endregion

        #region GetSortedPersons

        /*
            When we sort based on PersonName in DESC, it should return persons list in 
            descending on PersonName.
         */
        [Fact]
        public async Task GetSortedPersons()
        {
            const SortOrderOptions sortOrderOptions = SortOrderOptions.DESC;
            const string sortBy = nameof(Person.PersonName);

            List<CountryAddRequest> countryAddRequests = new()
            {
                new CountryAddRequest() { CountryName = "Viet Nam"},
                new CountryAddRequest() { CountryName = "Finland"},
                new CountryAddRequest() { CountryName = "Campuchia"},
            };

            List<PersonAddRequest> personAddRequests = new()
            {
                new PersonAddRequest()
                {
                    PersonName = "Pnu",
                    Email = "example1@gmail.com",
                    Address = "sample address 1",
                    Gender = GenderOptions.FEMALE,
                    DateOfBirth = DateTime.Parse("2000-02-01"),
                    ReceiveNewsLetters = true,
                },
                new PersonAddRequest()
                {
                    PersonName = "Hung",
                    Email = "example2@gmail.com",
                    Address = "sample address 2",
                    Gender = GenderOptions.MALE,
                    DateOfBirth = DateTime.Parse("2000-01-01"),
                    ReceiveNewsLetters = false,
                },
                new PersonAddRequest()
                {
                    PersonName = "MinAn",
                    Email = "example2@gmail.com",
                    Address = "sample address 2",
                    Gender = GenderOptions.MALE,
                    DateOfBirth = DateTime.Parse("2000-03-01"),
                    ReceiveNewsLetters = false,
                }
            };

            // Add Country and Supply Country ID for each Person
            for (int i = 0; i < personAddRequests.Count; i++)
            {
                CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequests[i]);
                personAddRequests[i].CountryID = countryResponse.CountryID;
            }

            // Add Persons
            List<PersonResponse> personResponsesListFromAdd = new();

            foreach (PersonAddRequest personToAdd in personAddRequests)
            {
                PersonResponse personResponse = await _personService.AddPerson(personToAdd);
                personResponsesListFromAdd.Add(personResponse);
            }

            // print personResponseListFromAdd by testOutputHelper
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse personResponseFromAdd in personResponsesListFromAdd)
            {
                _testOutputHelper.WriteLine(personResponseFromAdd.ToString());
            }

            // --- Start Acts ---

            // Get all persons
            List<PersonResponse> allPersons = await _personService.GetAllPersons();

            // Get Filtered Persons
            List<PersonResponse> personResponsesListFromSort = await _personService.GetSortedPersons(allPersons, sortBy, sortOrderOptions);
            
            // print personResponseListFromAdd by testOutputHelper
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse personResponseFromSearch in personResponsesListFromSort)
            {
                _testOutputHelper.WriteLine(personResponseFromSearch.ToString());
            }

            // Sort order by original persons add to Assert 
            personResponsesListFromAdd = personResponsesListFromAdd.OrderByDescending(p => ReflectionUtils.GetPropertyValue(p, sortBy)).ToList();

            // --- End Acts ---
            
            // --- Start Assert ---
            // Results - Assert
            PrintCharacter("-", 10, "RESULT");

            if (personResponsesListFromSort.Any())
            {
                _testOutputHelper.WriteLine("Successful Testing !!");

                for (int i = 0; i < personResponsesListFromAdd.Count(); ++i)
                {
                    Assert.Equal(personResponsesListFromAdd[i], personResponsesListFromSort[i]);
                }
            }
            else
            {
                /*There are not any elements in personResponsesListFromSearch*/
                _testOutputHelper.WriteLine($"There are not elements");
                Assert.True(!personResponsesListFromSort.Any());
            }

            PrintCharacter("-", 10 + "RESULT".Count());

            // --- End Assert ---
        }

        #endregion

        #region UpdatePerson

        /*
            When we supply null as PersonUpdateRequest, it should throw ArgumentNullException
         */
        [Fact]
        public async Task UpdatePerson_NullPerson()
        {
            // Arrange
            PersonUpdateRequest nullPersonUpdateRequest = null;

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                // Act
                PersonResponse personResponse  = await _personService.UpdatePerson(nullPersonUpdateRequest);
            });
        }
        
        // When we supply invalid person id, it should throw ArgumentNullException
        [Fact]
        public async Task UpdatePerson_InvalidPerson()
        {


            // Arrange
            // Id not exist in person collection
            PersonUpdateRequest nullPersonUpdateRequest = new() { PersonID = Guid.NewGuid() };

            // Assert
            await Assert.ThrowsAsync<ArgumentException>( async () =>
            {
                // Act
                PersonResponse personResponse  = await _personService.UpdatePerson(nullPersonUpdateRequest);
            });
        }

        // When PersonName is null, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_PersonNameIsNull()
        {
            // Arrange
            CountryAddRequest countryAddRequest = new()
            {
                CountryName = "Viet Nam"
            };

            CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = new()
            {
                PersonName = "An",
                CountryID = countryResponse.CountryID,
                Address = "Abc road",
                DateOfBirth = DateTime.Parse("2000-01-01"),
                Email = "abc@example.com",
                Gender = GenderOptions.MALE,
                ReceiveNewsLetters = true,
            };

            PersonResponse personResponseFromAdd = await _personService.AddPerson(personAddRequest);

            // cái ToPersonUpdateRequest để dùng trong test
            PersonUpdateRequest personUpdateRequest = personResponseFromAdd.ToPersonUpdateRequest();
            personUpdateRequest.PersonName = null;


            // Assert
            await Assert.ThrowsAsync<ArgumentException>( async () =>
            {
                // Act
                await _personService.UpdatePerson(personUpdateRequest);
            });
        }

        // First, add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_PersonFullDetailsUpdation()
        {
            // Arrange
            CountryAddRequest countryAddRequest = new()
            {
                CountryName = "Viet Nam"
            };

            CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequest);

            // Full Information
            PersonAddRequest personAddRequest = new()
            {
                PersonName = "An",
                CountryID = countryResponse.CountryID,
                Address = "Abc road",
                DateOfBirth = DateTime.Parse("2000-01-01"),
                Email = "abc@example.com",
                Gender = GenderOptions.MALE,
                ReceiveNewsLetters = true,
            };

            PersonResponse personResponseFromAdd = await _personService.AddPerson(personAddRequest);

            // cái ToPersonUpdateRequest để dùng trong test
            PersonUpdateRequest personUpdateRequest = personResponseFromAdd.ToPersonUpdateRequest();
            personUpdateRequest.PersonName = "William";
            personUpdateRequest.Email = "william@example.com";

            // Act
            PersonResponse personResponseFromUpdate = await _personService.UpdatePerson(personUpdateRequest);

            PersonResponse? personResponseFromGet = await _personService.GetPersonByPersonID(personResponseFromUpdate.PersonID);

            // Assert
            Assert.Equal(personResponseFromGet, personResponseFromUpdate);
        }

        #endregion

        #region DeletePerson

        // If you supply a valid PersonID, it should return true
        [Fact]
        public async Task DeletePerson_ValidPersonID()
        {
            // Arrange
            CountryAddRequest countryAddRequest = new()
            {
                CountryName = "Thai Lan"
            };

            CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = new()
            {
                PersonName = "MinAn",
                CountryID = countryResponse.CountryID,
                Address = "Abc road",
                DateOfBirth = DateTime.Parse("2000-01-01"),
                Email = "abc@example.com",
                Gender = GenderOptions.MALE,
                ReceiveNewsLetters = true,
            };

            PersonResponse personResponseFromAdd = await _personService.AddPerson(personAddRequest);

            // Act
            bool isDeleted = await _personService.DeletePerson(personResponseFromAdd.PersonID);

            // Assert
            Assert.True(isDeleted);
        }

        // If you supply a valid PersonID, it should return true
        [Fact]
        public async Task DeletePerson_InvalidPersonID()
        {
            // Arrange
            Guid invalidPersonID = Guid.NewGuid();

            // Act
            bool isDeleted = await _personService.DeletePerson(invalidPersonID);

            // Assert
            Assert.False(isDeleted);
        }

        #endregion

        private void PrintCharacter(string ch, int amount, string text = "")
        {
            var slashes = new StringBuilder(string.Empty);
            for (int i = 1; i <= amount / 2; ++i)
            {
                slashes.Append(ch);
            }

            if (!string.IsNullOrEmpty(text))
                slashes.Append(text);
            else
            {   
                for (int i = 0; i < text!.Count(); ++i)
                {
                    slashes.Append(ch);
                }
            }

            for (int i = 1; i <= amount / 2; ++i)
            {
                slashes.Append(ch);
            }


            _testOutputHelper.WriteLine(slashes.ToString());
        }
    }
}
