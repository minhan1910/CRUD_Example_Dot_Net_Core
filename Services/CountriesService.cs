using Entities;
using ServiceContracts;
using ServiceContracts.DTO.Countries;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly List<Country> _countries = new();

        public CountriesService(bool initialize = true)
        {
            if (initialize)
            {
                _countries.AddRange(GenerateMockCountriesDataList());          
            }
        }

        private List<Country> GenerateMockCountriesDataList()
        {
            return new List<Country>()
                {
                     new Country()
                     {
                        CountryID = Guid.Parse("6DB09C01-2555-4C0F-9F76-05573DD117E3"),
                        CountryName = "USA"
                     },

                    new Country()
                    {
                        CountryID = Guid.Parse("49E28576-CAD8-428E-B7D6-819DD76F3B3C"),
                        CountryName = "Canada"
                    },

                    new Country()
                    {
                        CountryID = Guid.Parse("A375ACFA-54D1-41C0-87D4-20E29A6A630A"),
                        CountryName = "India"
                    },

                    new Country()
                    {
                        CountryID = Guid.Parse("F6BB7888-FAA7-4A4F-B8BB-903E793E8C98"),
                        CountryName = "UK"
                    },

                    new Country()
                    {
                        CountryID = Guid.Parse("1E2CDD04-17BE-4D4A-9846-A22B7C823079"),
                        CountryName = "VN"
                    },

                    new Country()
                    {
                        CountryID = Guid.Parse("15889048-AF93-412C-B8F3-22103E943A6D"),
                        CountryName = "BR"
                    },
            };
        }

        public CountryResponse AddCountry(CountryAddRequest? countryAddRequest)
        {
            if (countryAddRequest is null)
            {
                throw new ArgumentNullException(nameof(countryAddRequest)); 
            }

            if (countryAddRequest.CountryName is null)
            {
                throw new ArgumentException(nameof(countryAddRequest.CountryName));
            }

            if (IsDuplicateCountryName(countryAddRequest.CountryName))
            {
                throw new ArgumentException("Given country name have already existed!");
            }

            // Convert From CountryAddRequest to Country Entity
            Country country = new()
            {
                CountryID = Guid.NewGuid(),
                CountryName = countryAddRequest.CountryName,
            };

            // of Repository
            _countries.Add(country);

            // Convert from Country to CountryResponse
            CountryResponse countryResponse = country.ToCountryResponse();

            return countryResponse;
        }

        public List<CountryResponse> GetAllCountries()
        {
            return ConvertCountryDtosToCountryEntities();
        }

        public CountryResponse? GetCountryByCountryID(Guid? countryID)
        {
            if (!countryID.HasValue)
            {
                throw new ArgumentNullException($"{countryID} is null");
            }

            Country? countryResponseFromList = _countries.FirstOrDefault(country => CompareCountryID(countryID.Value, country.CountryID));

            if (countryResponseFromList is null)
            {
                throw new ArgumentNullException($"The country by {countryID} is not existed!");
            }

            return countryResponseFromList.ToCountryResponse();
        }

        #region Utility_Methods
        private static bool CompareCountryID(Guid countryID, Guid country)
        {
            return country.Equals(countryID);
        }

        private List<CountryResponse> ConvertCountryDtosToCountryEntities()
        {
            return _countries.Select(country => country.ToCountryResponse())
                            .ToList();            
        }

        private bool IsDuplicateCountryName(string countryName)
            => _countries.Any(country => country.CountryName! == countryName);

        #endregion
    }
}