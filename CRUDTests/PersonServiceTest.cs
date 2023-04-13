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
using EntityFrameworkCoreMock;
using AutoFixture;
using FluentAssertions;
using RepositoryContracts;
using Moq;
using System.Linq.Expressions;
using System.ComponentModel;

namespace CRUDTests {
    public class PersonServiceTest
    {
        private readonly IPersonService _personService;        

        private readonly Mock<IPersonsRepository> _personsRepositoryMock;
        private readonly IPersonsRepository _personsRepository;

        private readonly Mock<ICountriesRepository> _countriesRepositoryMock; // Mock all methods in IC...
        private readonly ICountriesRepository _countriesRepository; // mocked object in ICoun....
        
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;
        public PersonServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture(); // for create dummy object
                  
            // Set up Repositories and Repositories Mock 
            _personsRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personsRepositoryMock.Object;

            _countriesRepositoryMock = new Mock<ICountriesRepository>();
            _countriesRepository = _countriesRepositoryMock.Object;
          
            // ** Set up Services **
            // Replace ApplicationDbContext to Repository Layer
            // => not need to DbContextMock
            _personService = new PersonService(_personsRepository);

            _testOutputHelper = testOutputHelper;
        }

        [Obsolete("Just Example config before learning repository mock")]
        private void ExampleConfigMockDBContext()
        {
            // ** Set up DbContext and DbContextMock (Not Needed)**
            var countriesInitialData = new List<Country>() { };
            var personsInitialData = new List<Person>() { };

            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>
                (new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            // From MockObject -> Mock ApplicationDbContext
            ApplicationDbContext dbContext = dbContextMock.Object;

            // ** Mock DbSet<Countries> and DbSet<Perons> **
            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);
            dbContextMock.CreateDbSetMock(temp => temp.Persons, personsInitialData);

        }

        #region AddPerson

        // When we supply null value as PersonAddRequest,
        // it should be throw ArgumentNullException
        [Fact]
        public async Task AddPerson_NullPerson_ToBeArgumentNullException()
        {
            // Arrange
            PersonAddRequest? personAddRequest = null;

            // Assert
            Func<Task> action = async () =>
            {
                //Act
                // it does not call direct repository method so it does not need mock
                await _personService.AddPerson(personAddRequest);
            };

            await action.Should().ThrowAsync<ArgumentNullException>();

            //await Assert.ThrowsAsync<ArgumentNullException>( );
        }

        // When we supply null value as PersonName,
        // it should be throw ArgumentException
        [Fact]
        public async Task AddPerson_NullPersonName_ToBeArgumentException()
        {
            // Arrange
            PersonAddRequest? personAddRequest = 
                _fixture.Build<PersonAddRequest>()
                        .With(temp => temp.PersonName, null as string) // string dataType not null dataType => null as string 
                        .Create();

            Person personFromRepository = personAddRequest.ToPerson();

            // When PersonsRepository.AddPerson is called, it has to return the same "person" object
            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                                  .ReturnsAsync(personFromRepository);

            // Assert
            Func<Task> action = async () =>
            {
                await _personService.AddPerson(personAddRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();

            //await Assert.ThrowsAsync<ArgumentException>(action);
        }

        /*
            When we supply proper person details, it should insert the person
            into the persons list; and it should return an object of PersonResponse, which
            includes with the newly generated PersonID.
        */
        [Fact]
        // AddPerson_ProperPersonDetails -> AddPerson_FullPersonDetails_ToBeSuccessful     
        public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
        {

            // Arrange
            CountryResponse countryResponse = _fixture.Create<CountryResponse>();

            //PersonAddRequest? personAddRequest = new()
            //{
            //    PersonName = "Person Test",
            //    Email = "example@gmail.com",
            //    Address = "sample address",
            //    CountryID = countryResponse.CountryID,
            //    Gender = GenderOptions.MALE,
            //    DateOfBirth = DateTime.Parse("2000-01-01"),
            //    ReceiveNewsLetters = true,
            //};

            PersonAddRequest? personAddRequest =
                _fixture.Build<PersonAddRequest>()
                    .With(temp => temp.Email, "someone@example.com")
                    .With(temp => temp.CountryID, countryResponse.CountryID)
                    .Create();

            Person person = personAddRequest.ToPerson(); // argument
            PersonResponse personResponseExpected = person.ToPersonResponse(); // return value

            // If we supply any argument value to the AddPerson method,
            // it should return the same return value
            // Executing Mock process.
            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                                  .ReturnsAsync(person);

            // Acts
            PersonResponse personResponseFromAdd = await _personService.AddPerson(personAddRequest);
            personResponseExpected.PersonID = personResponseFromAdd.PersonID;

            // Best practice - just single method is tested -> not calling GetAllPersons()
            //List<PersonResponse> persons_From_GetAllPersons = await _personService.GetAllPersons();

            // Assert
            //Assert.True(personResponseFromAdd.PersonID != Guid.Empty);
            personResponseFromAdd.PersonID.Should().NotBe(Guid.Empty);

            personResponseFromAdd.Should().Be(personResponseExpected);

            //persons_From_GetAllPersons.Should().Contain(personResponseFromAdd);            
            //Assert.Contains(personResponseFromAdd, persons_From_GetAllPersons);
        }

        #endregion

        #region GetPersonByPersonID

        // If we supply as PersonID, it should return null as PersonResponse
        [Fact]
        public async Task GetPersonByPersonID_NullPersonID_ToBeNull()
        {
            // Arrange
            Guid? nullPersonID = null;

            // Acts
            PersonResponse? personResponseFromGet = await _personService.GetPersonByPersonID(nullPersonID);
        
            // Assert
            //Assert.Null(personResponseFromGet);
            personResponseFromGet.Should().BeNull();
        }

        // If we supply a valid person id, it should return the valid person details
        // as PersonResponse object

        [Fact]
        public async Task GetPersonByPersonID_WithPersonID_ToBeSuccessful()
        {
            // Arrange - we should not test more than one method -> remove AddCountry
            //CountryAddRequest countryRequest = _fixture.Create<CountryAddRequest>();
            //CountryResponse countryResponse = await _countriesService.AddCountry(countryRequest);

            // Person from GetPersonByPersonID in services PersonsRepository.GetPeronByPersonId(personID)
            Person? person =
                _fixture.Build<Person>()
                        .With(temp => temp.Email, "someone@sample.com")
                        .With(temp => temp.Country, null as Country) // avoid circular reference
                        .Create();

            // Expected
            PersonResponse personResponseExpected = person.ToPersonResponse();

            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonId(It.IsAny<Guid>()))
                                .ReturnsAsync(person);

            // Just test single method => assuming the add person return the person object successfully.
            //PersonResponse personResponseFromAdd = await _personService.AddPerson(personAddRequest);

            // Acutal
            PersonResponse? personResponseFromGet = await _personService.GetPersonByPersonID(person.PersonID);
            personResponseExpected.PersonID = personResponseFromGet!.PersonID;

            // Assert
            //Assert.Equal(personResponseFromAdd, personResponseFromGet);
            personResponseFromGet.Should().Be(personResponseExpected);
        }

        #endregion

        #region GetAllPersons

        // The GetAllPersons() should return an empty list by default
        [Fact]
        public async Task GetAllPersons_ToBeEmptyList()
        {
            // Arrange
            var persons = new List<Person>();

            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                                  .ReturnsAsync(persons);

            // Act
            List<PersonResponse> people = await _personService.GetAllPersons();

            // Assert
            //Assert.Empty(people);
            people.Should().BeEmpty();
        }

        // First, we will add few persons; and then when we call GetAllPersons(),
        // it should return the same persons that were added
        [Fact]
        public async Task GetAllPersons_WithFewPersons_ToBeSuccessful()
        {
            //List<CountryAddRequest> countryAddRequests = new()
            //{
            //    _fixture.Create<CountryAddRequest>(),
            //    _fixture.Create<CountryAddRequest>(),
            //    _fixture.Create<CountryAddRequest>(),
            //};

            //List<PersonAddRequest> personAddRequests = new()
            //{
            //    _fixture.Build<PersonAddRequest>()
            //            .With(temp => temp.Email, "someone_1@example.com")
            //            .Create(),
            //    _fixture.Build<PersonAddRequest>()
            //            .With(temp => temp.Email, "someone_2@example.com")
            //            .Create(),
            //    _fixture.Build<PersonAddRequest>()
            //            .With(temp => temp.Email, "someone_3@example.com")
            //            .Create(),
            //};

            List<Person> persons = new()
            {
                _fixture.Build<Person>()
                        .With(temp => temp.Email, "someone_1@example.com")
                        // Vì thằng này dùng navigation property nên cả 2 class đều qua lại -> ko biết tạo ra object sao
                        .With(temp => temp.Country, null as Country) // avoid circular reference
                        .Create(),
                _fixture.Build<Person>()
                        .With(temp => temp.Email, "someone_2@example.com")
                        .With(temp => temp.Country, null as Country)
                        .Create(),
                _fixture.Build<Person>()
                        .With(temp => temp.Email, "someone_3@example.com")
                        .With(temp => temp.Country, null as Country)
                        .Create(),
            };

            // Add Country and Supply Country ID for each Person
            //for (int i = 0; i < personAddRequests.Count; i++)
            //{
            //    CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequests[i]);
            //    personAddRequests[i].CountryID = countryResponse.CountryID;
            //}

            // personResponsesListExpected from repository mock
            List<PersonResponse> personResponsesListExpected = persons.Select(p => p.ToPersonResponse())
                                                                     .ToList();

            //foreach (PersonAddRequest personToAdd in personAddRequests)
            //{
            //    PersonResponse personResponse = await _personService.AddPerson(personToAdd);
            //    personResponsesListFromAdd.Add(personResponse);
            //}

            // print personResponseListFromAdd by testOutputHelper
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse personResponseFromAdd in personResponsesListExpected)
            {
                _testOutputHelper.WriteLine(personResponseFromAdd.ToString());
            }

            // Mock GetAllPersons method
            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                                  .ReturnsAsync(persons);
                        
            // Get All Persons - Actual
            List<PersonResponse> personResponsesListFromGet = await _personService.GetAllPersons();

            // print personResponseListFromAdd by testOutputHelper
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse personResponseFromGet in personResponsesListFromGet)
            {
                _testOutputHelper.WriteLine(personResponseFromGet.ToString());
            }

            // Assert
            //Assert.True(personResponsesListFromAdd.Count == personResponsesListFromGet.Count);
            //personResponsesListFromGet.Count.Should().Be(personResponsesListFromAdd.Count);
            personResponsesListFromGet.Should().HaveSameCount(personResponsesListExpected);

            // ** Using foreach approach **
            //foreach (PersonResponse expectedPerson in personResponsesListFromAdd)
            //{
            //    //Assert.Contains(expectedPerson, personResponsesListFromGet);
            //    personResponsesListFromGet.Should().Contain(expectedPerson);
            //}

            // Using Fluent Assertion with one statment
            personResponsesListFromGet.Should().BeEquivalentTo(personResponsesListExpected);
        }

        #endregion

        #region GetFilteredPersons

        // If the search text is empty and search by is "PersonName",
        // it should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText_ToBeSuccessful()
        {
            // Arrange
            List<Person> persons = new()
            {
                _fixture.Build<Person>()
                        .With(temp => temp.Email, "someone_1@example.com")
                        .With(temp => temp.Country, null as Country)
                        .Create(),
                _fixture.Build<Person>()
                        .With(temp => temp.Email, "someone_2@example.com")
                        .With(temp => temp.Country, null as Country)
                        .Create(),
                _fixture.Build<Person>()
                        .With(temp => temp.Email, "someone_3@example.com")
                        .With(temp => temp.Country, null as Country)
                        .Create(),
            };
          
            List<PersonResponse> personResponsesListExpected = persons.Select(p => p.ToPersonResponse())
                                                                      .ToList();

            

            // print personResponseListFromAdd by testOutputHelper
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse personResponseFromAdd in personResponsesListExpected)
            {
                _testOutputHelper.WriteLine(personResponseFromAdd.ToString());
            }

            // Mock GetFilterdPersons in service call repo
            //_personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
            //                      .ReturnsAsync(persons);

            _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                                  .ReturnsAsync(persons);

            // Get Filtered Persons
          List <PersonResponse> personResponsesListFromSearch = await _personService.GetFilteredPersons(nameof(Person.PersonName), string.Empty);

            // print personResponseListFromAdd by testOutputHelper
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse personResponseFromSearch in personResponsesListFromSearch)
            {
                _testOutputHelper.WriteLine(personResponseFromSearch.ToString());
            }

            // Assert
            //Assert.True(personResponsesListFromAdd.Count == personResponsesListFromSearch.Count);
            personResponsesListFromSearch.Should().HaveSameCount(personResponsesListExpected);

            //foreach (PersonResponse expectedPerson in personResponsesListFromAdd)
            //{
            //    //Assert.Contains(expectedPerson, personResponsesListFromSearch);
            //    personResponsesListFromSearch.Should().Contain(expectedPerson);
            //}

            personResponsesListFromSearch.Should().BeEquivalentTo(personResponsesListExpected);
        }

        /*
            We will search based on person name with some searchg string. 
            It should return the matching persons. 
         */   
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName_ToBeSuccessful()
        {
            const string searchString = "An";

            List<Person> persons = new()
            {
                _fixture.Build<Person>()
                        .With(temp => temp.Email, "someone_1@example.com")
                        .With(temp => temp.Country, null as Country)
                        .Create(),
                _fixture.Build<Person>()
                        .With(temp => temp.Email, "someone_2@example.com")
                        .With(temp => temp.Country, null as Country)
                        .Create(),
                _fixture.Build<Person>()
                        .With(temp => temp.Email, "someone_3@example.com")
                        .With(temp => temp.Country, null as Country)
                        .Create(),
            };

            // Add Persons
            List<PersonResponse> personResponsesListExpected = persons.Select(p => p.ToPersonResponse())
                                                                      .ToList();

            // print personResponseListFromAdd by testOutputHelper
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse personResponseFromAdd in personResponsesListExpected)
            {
                _testOutputHelper.WriteLine(personResponseFromAdd.ToString());
            }

            // Mock GetFilterdPersons in service call repo
            _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                                  .ReturnsAsync(persons);

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

                //foreach (PersonResponse expectedPerson in personResponsesListFromAdd)
                //{
                //    var personName = expectedPerson.PersonName;

                //    if (personName is not null)
                //    {
                //        if (personName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                //        {
                //            //Assert.Contains(expectedPerson, personResponsesListFromSearch);
                //            personResponsesListFromSearch.Should().Contain(expectedPerson);
                //        }
                //    }
                //}


                //personResponsesListFromSearch
                //    .Should()
                //    .OnlyContain(
                //        temp => temp.PersonName != null && temp.PersonName.Contains(searchString, StringComparison.OrdinalIgnoreCase));

                personResponsesListFromSearch.Should().BeEquivalentTo(personResponsesListExpected);
            } else
            {
                /*There are not any elements in personResponsesListFromSearch*/
                
                _testOutputHelper.WriteLine($"There are not elements by searchString: {searchString}");
                //Assert.True(!personResponsesListFromSearch.Any());
                personResponsesListFromSearch.Should().BeEmpty();
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
        public async Task GetSortedPersons_ToBeSuccessful()
        {
            const SortOrderOptions sortOrderOptions = SortOrderOptions.DESC;
            const string sortBy = nameof(Person.PersonName);

            List<Person> persons = new()
            {
                _fixture.Build<Person>()
                        .With(temp => temp.Email, "someone_1@example.com")
                        .With(temp => temp.Country, null as Country)
                        .Create(),
                _fixture.Build<Person>()
                        .With(temp => temp.Email, "someone_2@example.com")
                        .With(temp => temp.Country, null as Country)
                        .Create(),
                _fixture.Build<Person>()
                        .With(temp => temp.Email, "someone_3@example.com")
                        .With(temp => temp.Country, null as Country)
                        .Create(),
            };

            List<PersonResponse> personResponsesListExpected = persons.Select(p => p.ToPersonResponse())
                                                                      .ToList();

            // print personResponseListFromAdd by testOutputHelper
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse personResponseFromExpected in personResponsesListExpected)
            {
                _testOutputHelper.WriteLine(personResponseFromExpected.ToString());
            }

            // --- Start Acts ---

            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                                  .ReturnsAsync(persons);

            // Get all persons
            List<PersonResponse> allPersons = await _personService.GetAllPersons();

            // Get Filtered Persons
            List<PersonResponse> personResponsesListFromSort = await _personService.GetSortedPersons(personResponsesListExpected, sortBy, sortOrderOptions);
            
            // print personResponseListFromAdd by testOutputHelper
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse personResponseFromSearch in personResponsesListFromSort)
            {
                _testOutputHelper.WriteLine(personResponseFromSearch.ToString());
            }

            // Sort order by original persons add to Assert 
            //personResponsesListFromAdd = personResponsesListFromAdd.OrderByDescending(p => ReflectionUtils.GetPropertyValue(p, sortBy)).ToList();

            // --- End Acts ---
            
            // --- Start Assert ---
            // Results - Assert
            PrintCharacter("-", 10, "RESULT");

            if (personResponsesListFromSort.Any())
            {
                _testOutputHelper.WriteLine("Successful Testing !!");

                // Cách 1:
                //for (int i = 0; i < personResponsesListFromAdd.Count(); ++i)
                //{
                //    //Assert.Equal(personResponsesListFromAdd[i], personResponsesListFromSort[i]);
                //    personResponsesListFromSort[i].Should().Be(personResponsesListFromAdd[i]);
                //}

                // Cách 2:
                //personResponsesListFromSort.Should().BeEquivalentTo(personResponsesListFromAdd);

                // Cách 3:
                personResponsesListFromSort.Should().BeInDescendingOrder(temp => temp.PersonName);
            }
            else
            {
                /*There are not any elements in personResponsesListFromSearch*/
                _testOutputHelper.WriteLine($"There are not elements");
                //Assert.True(!personResponsesListFromSort.Any());
                personResponsesListFromSort.Should().BeEmpty();
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
        public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
        {
            // Arrange
            PersonUpdateRequest nullPersonUpdateRequest = null;

            // Assert
            Func<Task> action = async () =>
            {
                // Act
                PersonResponse personResponse = await _personService.UpdatePerson(nullPersonUpdateRequest);
            };

            await action.Should().ThrowAsync<ArgumentNullException>();
            //await Assert.ThrowsAsync<ArgumentNullException>(action);
        }
        
        // When we supply invalid person id, it should throw ArgumentNullException
        [Fact]
        public async Task UpdatePerson_InvalidPerson_ToBeArgumentException()
        {
            // Arrange
            // Id not exist in person collection
            // Each time _fixture create a new PersonID
            PersonUpdateRequest invalidPersonIdInPersonUpdateRequest = _fixture.Create<PersonUpdateRequest>();

            // Assert
            Func<Task> action = async () =>
            {
                // Act
                PersonResponse personResponse = await _personService.UpdatePerson(invalidPersonIdInPersonUpdateRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();
            //await Assert.ThrowsAsync<ArgumentException>(action);
        }

        // When PersonName is null, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException()
        {
            // Arrange
            Person person = _fixture.Build<Person>()
                                    .With(temp => temp.PersonName, null as string)
                                    .With(temp => temp.Gender, GenderOptions.MALE.ToString())
                                    .With(temp => temp.Email, "someone@example.com")
                                    .With(temp => temp.Country, null as Country)
                                    .Create();

            PersonResponse personResponse = person.ToPersonResponse();

            // cái ToPersonUpdateRequest để dùng trong test
            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            // Assert
            Func<Task> action = async () =>
            {
                // Act
                await _personService.UpdatePerson(personUpdateRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();
            //await Assert.ThrowsAsync<ArgumentException>(action);
        }

        // First, add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_PersonFullDetails_ToBeSuccessful()
        {
            // Arrange

            Person person = _fixture.Build<Person>()
                                   .With(temp => temp.Gender, GenderOptions.MALE.ToString())
                                   .With(temp => temp.Email, "someone@example.com")
                                   .With(temp => temp.Country, null as Country)
                                   .Create();

            PersonResponse personResponseExpected = person.ToPersonResponse();

            // cái ToPersonUpdateRequest để dùng trong test
            PersonUpdateRequest personUpdateRequest = personResponseExpected.ToPersonUpdateRequest();

            // Act

            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonId(It.IsAny<Guid>()))
                                  .ReturnsAsync(person);
            
            _personsRepositoryMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>()))
                                  .ReturnsAsync(person);

            PersonResponse personResponseFromUpdate = await _personService.UpdatePerson(personUpdateRequest);

            //PersonResponse? personResponseFromGet = await _personService.GetPersonByPersonID(personResponseFromUpdate.PersonID);

            // Assert
            personResponseFromUpdate.Should().Be(personResponseExpected);
            //Assert.Equal(personResponseFromGet, personResponseFromUpdate);
        }

        #endregion

        #region DeletePerson

        // If you supply a valid PersonID, it should return true
        [Fact]
        public async Task DeletePerson_ValidPersonID_ToBeSuccessful()
        {
            // Arrange
            Person person = _fixture.Build<Person>()
                                   .With(temp => temp.Gender, GenderOptions.MALE.ToString())
                                   .With(temp => temp.Email, "someone@example.com")
                                   .With(temp => temp.Country, null as Country)
                                   .Create();

            bool isDeletedExpected = true;

            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonId(It.IsAny<Guid>()))
                                  .ReturnsAsync(person);

            _personsRepositoryMock.Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>()))
                                  .ReturnsAsync(isDeletedExpected);

            // Act
            bool isDeleted = await _personService.DeletePerson(person.PersonID);

            // Assert
            //Assert.True(isDeleted);
            isDeleted.Should().BeTrue();
        }

        // If you supply a valid PersonID, it should return true
        [Fact]
        public async Task DeletePerson_InvalidPersonID_ToBeFalse()
        {
            // Arrange
            Guid invalidPersonID = Guid.NewGuid();

            bool isDeletedExpected = false;

            _personsRepositoryMock.Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>()))
                                  .ReturnsAsync(isDeletedExpected);

            // Act
            bool isDeleted = await _personService.DeletePerson(invalidPersonID);

            // Assert
            //Assert.False(isDeleted);
            isDeleted.Should().BeFalse();
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
