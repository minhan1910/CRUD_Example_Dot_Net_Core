using System;
using ServiceContracts;
using Entities;
using Services;
using ServiceContracts.DTO.Countries;

namespace CRUDTests
{
    public class CountriesServiceTest
    {        

        private readonly ICountriesService _countriesService;

        // constructor
        public CountriesServiceTest()
        {
            _countriesService = new CountriesService(false);
        }

        #region AddCountry

        // When CountryAddRequest is null, it should throw ArgumentNullException
        [Fact]
        public void AddCountry_NullCountry()
        {
            // Arrange
            CountryAddRequest? request = null;
    
            // Assert
            Assert.Throws<ArgumentNullException>(() => 
            {
                // Act
                _countriesService.AddCountry(request);
            });
        }


        // When the CountryName is null, it should throw ArugumentException
        [Fact]
        public void AddCountry_CountryNameIsNull()
        {
            // Arrange
            CountryAddRequest? request = new() { CountryName = null };

            // Assert
            Assert.Throws<ArgumentException>(() =>
            {
                // Act
                _countriesService.AddCountry(request);
            });
        }

        // When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public void AddCountry_DuplicateCountryName()
        {
            // Arrange
            CountryAddRequest? request1 = new() { CountryName = "USA" };
            CountryAddRequest? request2 = new() { CountryName = "USA" };

            // Assert
            Assert.Throws<ArgumentException>(() =>
            {
                // Act
                _countriesService.AddCountry(request1);
                _countriesService.AddCountry(request2);
            });
        }

        // When your supply proper country name, it should insert (add) the existing list of countries
        [Fact]
        public void AddCountry_ProperCountryDetails()
        {
            // Arrange
            CountryAddRequest? request = new() { CountryName = "Japan" };

            // Acts
            CountryResponse response = _countriesService.AddCountry(request);
            List<CountryResponse> countries_from_GetAllCountries = _countriesService.GetAllCountries();

            // Assert
            Assert.True(response.CountryID != Guid.Empty);
            Assert.Contains(response, countries_from_GetAllCountries);
        }

        #endregion

        #region GetAllCountries

        [Fact]
        
        // The list of countries should be empty by default (before adding any countries)
        public void GetAllCountries_EmptyList()
        {
            // Acts
            List<CountryResponse> acutal_countries_response_list = _countriesService.GetAllCountries();

            // Assert
            Assert.Empty(acutal_countries_response_list);
        }


        [Fact]
        public void GetAllCountries_AddFewCountries()
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
                CountryResponse countryAfterAdded = _countriesService.AddCountry(countryAddRequest);
                
                countriesListFromAddCountry.Add(countryAfterAdded);
            }

            // Get all countries by GetAllCountries method
            List<CountryResponse> actualCountriesResponseList = _countriesService.GetAllCountries();

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
        public void GetCountryByCountryID_NullCountryID()
        {
            // Arrange
            Guid? countryID = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Act
                _countriesService.GetCountryByCountryID(countryID);
            });
        }

        [Fact]
        public void GetCountryByCountryID_InvalidCountryID()
        {
            // Arrange
            Guid? invalidCountryID = Guid.NewGuid();

            // Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Act
                _countriesService.GetCountryByCountryID(invalidCountryID);
            });
        }

        [Fact]
        // If we supply a valid country id, it should return the matching country details 
        // as CountryResponse object
        public void GetCountryByCountryID_ValidCountryID()
        {
            // Arrange
            CountryAddRequest? countryAddRequest = new()
            {
                CountryName = "Thai Lan"
            };

            CountryResponse countryResponseFromAdd = _countriesService.AddCountry(countryAddRequest);

            // Act
            CountryResponse? countryResponseFromGet = _countriesService.GetCountryByCountryID(countryResponseFromAdd.CountryID);

            // Assert

            Assert.Equal(countryResponseFromAdd, countryResponseFromGet);
        }

        #endregion
    }
}
