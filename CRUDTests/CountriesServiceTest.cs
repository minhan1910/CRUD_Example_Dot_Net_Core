using System;
using ServiceContracts;
using Entities;
using Services;
using ServiceContracts.DTO.Countries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using EntityFrameworkCoreMock;
using FluentAssertions;
using RepositoryContracts;
using Moq;
using AutoFixture;

namespace CRUDTests
{
    public class CountriesServiceTest
    {
        IFixture _fixture;

        private readonly ICountriesRepository _countriesRepository;
        private readonly Mock<ICountriesRepository> _countriesRepositoryMock;

        private readonly ICountriesService _countriesService;

        // constructor
        public CountriesServiceTest()
        {
            _fixture = new Fixture();

            _countriesRepositoryMock = new Mock<ICountriesRepository>();    
            _countriesRepository = _countriesRepositoryMock.Object;

            //var countriesInitialData = new List<Country>() { };
  
            //DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>
            //    (new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            //// From MockObject -> Mock  ApplicationDbContext
            //ApplicationDbContext dbContext = dbContextMock.Object;

            //// Mock DbSet<Countries>
            //dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);
  
            // Countries Service
            _countriesService = new CountriesService(_countriesRepository);
        }

        #region AddCountry

        // When CountryAddRequest is null, it should throw ArgumentNullException
        [Fact]
        public async Task AddCountry_GivenNullCountry_ShouldBeThrow_ArumentNullException()
        {
            // Arrange
            CountryAddRequest? request = null;

            // Assert
            Func<Task> action = async () =>
            {
                // Act
                await _countriesService.AddCountry(request);
            };
            await action.Should().ThrowAsync<ArgumentNullException>();
            //await Assert.ThrowsAsync<ArgumentNullException>(aciton);
        }


        // When the CountryName is null, it should throw ArugumentException
        [Fact]
        public async Task AddCountry_CountryNameIsNull_ShouldBeThrow_ArgumentNullException()
        {
            // Arrange
            CountryAddRequest? request = _fixture.Build<CountryAddRequest>()
                                                 .With(temp => temp.CountryName, null as string)
                                                 .Create();

            _countriesRepositoryMock.Setup(temp => temp.GetCountryByName(It.IsAny<string>()))
                                   .ReturnsAsync(null as Country);


            Func<Task> action = async () =>
            {
                // Act
                await _countriesService.AddCountry(request);
            };

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
            //await Assert.ThrowsAsync<ArgumentException>(action);
        }

        // When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName_ToBeArgumentException()
        {
            // Arrange
            Country? country = _fixture.Build<Country>()
                                       .With(temp => temp.Persons, null as List<Person>)
                                       .Create();

            CountryAddRequest countryAddRequest = country.ToCountryAddRequest();

            _countriesRepositoryMock.Setup(temp => temp.GetCountryByName(It.IsAny<string>()))
                                    .ReturnsAsync(country);

            // Assert
            Func<Task> action = async () =>
            {
                // Act
                await _countriesService.AddCountry(countryAddRequest);                
            };

            await action.Should().ThrowAsync<ArgumentException>();
            //await Assert.ThrowsAsync<ArgumentException>(action);
        }

        // When your supply proper country name, it should insert (add) the existing list of countries
        [Fact]
        public async Task AddCountry_ProperCountryDetails_ToBeSuccessful()
        {
            // Arrange
            Country country = _fixture.Build<Country>()
                                      .With(temp => temp.Persons, null as List<Person>)
                                      .Create();

            CountryAddRequest countryAddRequest = country.ToCountryAddRequest();

            _countriesRepositoryMock.Setup(temp => temp.GetCountryByName(It.IsAny<string>()))
                                    .ReturnsAsync(null as Country); 

            _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>()))
                                    .ReturnsAsync(country); 

            // Acts
            CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequest);
            //List<CountryResponse> countries_from_GetAllCountries = await _countriesService.GetAllCountries();

            // Assert
            countryResponse.CountryID.Should().Be(country.CountryID);

            // Assert
            //Assert.True(response.CountryID != Guid.Empty);
            //response.CountryID.Should().NotBe(Guid.Empty);

            //Assert.Contains(response, countries_from_GetAllCountries);
            //countries_from_GetAllCountries.Should().Contain(response);
        }

        #endregion

        #region GetAllCountries

        [Fact]
        
        // The list of countries should be empty by default (before adding any countries)
        public async Task GetAllCountries_GivenEmptyList_ToBeSuccessful()
        {
            // Arrange            

            List<Country> emptyCountryList = new List<Country>();
            List<CountryResponse> emptyCountryResponseListExpected = emptyCountryList.Select(c => c.ToCountryResponse()).ToList();

            _countriesRepositoryMock.Setup(temp => temp.GetAllCountries())
                                    .ReturnsAsync(emptyCountryList);

            // Acts
            List<CountryResponse> acutal_countries_response_list = await _countriesService.GetAllCountries();

            // Assert
            //Assert.Empty(acutal_countries_response_list);
            acutal_countries_response_list.Should().BeEquivalentTo(emptyCountryResponseListExpected);           
        }

        [Fact]
        public async Task GetAllCountries_AddFewCountries_ToBeSuccessful()
        {
            List<Country> countries = new()
            {
                _fixture.Build<Country>()
                        .With(temp => temp.Persons, null as List<Person>)
                        .Create(),
                
                _fixture.Build<Country>()
                        .With(temp => temp.Persons, null as List<Person>)
                        .Create(),

                _fixture.Build<Country>()
                        .With(temp => temp.Persons, null as List<Person>)
                        .Create()
            };

            List<CountryResponse> countriesExpected = countries.Select(c => c.ToCountryResponse()).ToList();

            _countriesRepositoryMock.Setup(temp => temp.GetAllCountries())
                                    .ReturnsAsync(countries);   

            // Get all countries by GetAllCountries method
            List<CountryResponse> actualCountriesResponseList = await _countriesService.GetAllCountries();

            // **Using xUnit - Normal Assert**
            // Read each element from countries_list_from_add_country
            //foreach (CountryResponse expectedCountry in countriesListFromAddCountry)
            //{
            //    Assert.Contains(expectedCountry, actualCountriesResponseList);
            //}


            // Using Fluent Assertion
            //actualCountriesResponseList.Should().BeEquivalentTo(countriesListFromAddCountry);


            actualCountriesResponseList.Should().BeEquivalentTo(countriesExpected);
        }

        #endregion

        #region GetCountryByCountryID

        [Fact]
        // If we supply null as CountryID, it should return the null as CountryResponse
        public async Task GetCountryByCountryID_NullCountryID_ToBeArgumentNullException()
        {
            // Arrange
            Guid? countryID = null;

            // Assert
            Func<Task> action = async () =>
            {
                // Act
                await _countriesService.GetCountryByCountryID(countryID);
            };

            await action.Should().ThrowAsync<ArgumentNullException>();  
            //await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        public async Task GetCountryByCountryID_InvalidCountryID_ToBeArgumentNullException()
        {
            // Arrange
            Guid? invalidCountryID = Guid.NewGuid();

            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryId(It.IsAny<Guid>()))
                                    .ReturnsAsync(null as Country);

            // Assert
            Func<Task> action = async () =>
            {
                // Act
                await _countriesService.GetCountryByCountryID(invalidCountryID);
            };

            await action.Should().ThrowAsync<ArgumentNullException>();  
            //await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        // If we supply a valid country id, it should return the matching country details 
        // as CountryResponse object
        public async Task GetCountryByCountryID_ValidCountryID_ToBeSuccessful()
        {
            // Arrange
            Country country = _fixture.Build<Country>()
                                      .With(temp => temp.Persons, null as List<Person>)
                                      .Create();

            CountryResponse countryExpected = country.ToCountryResponse();

            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryId(It.IsAny<Guid>()))
                            .ReturnsAsync(country);

            // Act
            CountryResponse? countryResponseFromGet = await _countriesService.GetCountryByCountryID(country.CountryID);

            // Assert

            //Assert.Equal(countryResponseFromAdd, countryResponseFromGet);
            countryResponseFromGet.Should().Be(countryExpected);         
        }

        #endregion
    }
}
