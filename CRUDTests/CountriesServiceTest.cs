using System;
using ServiceContracts;
using Entities;
using Services;
using ServiceContracts.DTO.Countries;
using Microsoft.EntityFrameworkCore;

namespace CRUDTests
{
    public class CountriesServiceTest
    {        

        private readonly ICountriesService _countriesService;

        // constructor
        public CountriesServiceTest()
        {
            _countriesService = new CountriesService(
                                    new PersonsDbContext(
                                        new DbContextOptionsBuilder<PersonsDbContext>().Options));
        }

        #region AddCountry

        // When CountryAddRequest is null, it should throw ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry()
        {
            // Arrange
            CountryAddRequest? request = null;
    
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>( async () => 
            {
                // Act
                await _countriesService.AddCountry(request);
            });
        }


        // When the CountryName is null, it should throw ArugumentException
        [Fact]
        public async Task AddCountry_CountryNameIsNull()
        {
            // Arrange
            CountryAddRequest? request = new() { CountryName = null };

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // Act
                await _countriesService.AddCountry(request);
            });
        }

        // When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName()
        {
            // Arrange
            CountryAddRequest? request1 = new() { CountryName = "USA" };
            CountryAddRequest? request2 = new() { CountryName = "USA" };

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // Act
                await _countriesService.AddCountry(request1);
                await _countriesService.AddCountry(request2);
            });
        }

        // When your supply proper country name, it should insert (add) the existing list of countries
        [Fact]
        public async Task AddCountry_ProperCountryDetails()
        {
            // Arrange
            CountryAddRequest? request = new() { CountryName = "Japan" };

            // Acts
            CountryResponse response = await _countriesService.AddCountry(request);
            List<CountryResponse> countries_from_GetAllCountries = await _countriesService.GetAllCountries();

            // Assert
            Assert.True(response.CountryID != Guid.Empty);
            Assert.Contains(response, countries_from_GetAllCountries);
        }

        #endregion

        #region GetAllCountries

        [Fact]
        
        // The list of countries should be empty by default (before adding any countries)
        public async Task GetAllCountries_EmptyList()
        {
            // Acts
            List<CountryResponse> acutal_countries_response_list = await _countriesService.GetAllCountries();

            // Assert
            Assert.Empty(acutal_countries_response_list);
        }


        [Fact]
        public async Task GetAllCountries_AddFewCountries()
        {
            List<CountryAddRequest> countryAddRequests = new()
            {
                new CountryAddRequest()
                {
                    CountryName = "UK"
                },
                new CountryAddRequest()
                {
                    CountryName = "VIETNAM"
                },
            };

            List<CountryResponse> countriesListFromAddCountry = new();

            // Add country to list and add each response to countries_list_add_country
            foreach (CountryAddRequest countryAddRequest in countryAddRequests)
            {
                CountryResponse countryAfterAdded = await _countriesService.AddCountry(countryAddRequest);
                
                countriesListFromAddCountry.Add(countryAfterAdded);
            }

            // Get all countries by GetAllCountries method
            List<CountryResponse> actualCountriesResponseList = await _countriesService.GetAllCountries();

            // Read each element from countries_list_from_add_country
            foreach (CountryResponse expectedCountry in countriesListFromAddCountry)
            {
                Assert.Contains(expectedCountry, actualCountriesResponseList);
            }
        }

        #endregion

        #region GetCountryByCountryID

        [Fact]
        // If we supply null as CountryID, it should return the null as CountryResponse
        public async Task GetCountryByCountryID_NullCountryID()
        {
            // Arrange
            Guid? countryID = null;

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>( async () =>
            {
                // Act
                await _countriesService.GetCountryByCountryID(countryID);
            });
        }

        [Fact]
        public async Task GetCountryByCountryID_InvalidCountryID()
        {
            // Arrange
            Guid? invalidCountryID = Guid.NewGuid();

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>( async () =>
            {
                // Act
                await _countriesService.GetCountryByCountryID(invalidCountryID);
            });
        }

        [Fact]
        // If we supply a valid country id, it should return the matching country details 
        // as CountryResponse object
        public async Task GetCountryByCountryID_ValidCountryID()
        {
            // Arrange
            CountryAddRequest? countryAddRequest = new()
            {
                CountryName = "Thai Lan"
            };

            CountryResponse countryResponseFromAdd = await _countriesService.AddCountry(countryAddRequest);

            // Act
            CountryResponse? countryResponseFromGet = await _countriesService.GetCountryByCountryID(countryResponseFromAdd.CountryID);

            // Assert

            Assert.Equal(countryResponseFromAdd, countryResponseFromGet);
        }

        #endregion
    }
}
