using Entities;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO.Countries;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly ICountriesRepository _countriesRepository;

        public CountriesService(ICountriesRepository countriesRepository)
        {
            _countriesRepository = countriesRepository;
        }

        public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
            if (countryAddRequest is null)
            {
                throw new ArgumentNullException(nameof(countryAddRequest)); 
            }

            if (countryAddRequest.CountryName is null)
            {
                throw new ArgumentNullException(nameof(countryAddRequest.CountryName));
            }

            if (await _countriesRepository.GetCountryByName(countryAddRequest.CountryName) != null)
            {
                throw new ArgumentException("Given country name have already existed!");
            }

            // Convert From CountryAddRequest to Country Entity

            // of Repository
            Country country = await _countriesRepository.AddCountry(countryAddRequest.ToCountry());

            // Convert from Country to CountryResponse
            CountryResponse countryResponse = country.ToCountryResponse();

            return countryResponse;
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            List<Country> countries = await _countriesRepository.GetAllCountries();
            
            return countries.Select(c => c.ToCountryResponse())
                            .ToList();
        }

        public async Task<CountryResponse?> GetCountryByCountryID(Guid? countryID)
        {
            if (!countryID.HasValue)
            {
                throw new ArgumentNullException($"{countryID} is null");
            }

            Country? countryResponseFromList = await _countriesRepository.GetCountryByCountryId(countryID.Value);  

            if (countryResponseFromList is null)
            {
                throw new ArgumentNullException($"The country by {countryID} is not existed!");
            }

            return countryResponseFromList.ToCountryResponse();
        }  
    }
}