using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO.Countries;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly PersonsDbContext _db;

        public CountriesService(PersonsDbContext personsDbContext, bool initialize = true)
        {
            _db = personsDbContext;

            if (initialize)
            {
                //_db.AddRange(GenerateMockCountriesDataList());          
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

        public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
            if (countryAddRequest is null)
            {
                throw new ArgumentNullException(nameof(countryAddRequest)); 
            }

            if (countryAddRequest.CountryName is null)
            {
                throw new ArgumentException(nameof(countryAddRequest.CountryName));
            }

            if (await _db.Countries.CountAsync(temp => temp.CountryName == countryAddRequest.CountryName) > 0)
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
            _db.Countries.Add(country);
            await _db.SaveChangesAsync();

            // Convert from Country to CountryResponse
            CountryResponse countryResponse = country.ToCountryResponse();

            return countryResponse;
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
           return await _db.Countries.Select(country => country.ToCountryResponse())
                            .ToListAsync();
        }

        public async Task<CountryResponse?> GetCountryByCountryID(Guid? countryID)
        {
            if (!countryID.HasValue)
            {
                throw new ArgumentNullException($"{countryID} is null");
            }

            Country? countryResponseFromList = await _db.Countries
                                                  .FirstOrDefaultAsync(country => countryID.Value.Equals(country.CountryID));

            if (countryResponseFromList is null)
            {
                throw new ArgumentNullException($"The country by {countryID} is not existed!");
            }

            return countryResponseFromList.ToCountryResponse();
        }  
    }
}