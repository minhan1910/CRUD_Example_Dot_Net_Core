﻿using ServiceContracts.DTO.Countries;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Country entity
    /// </summary>
    public interface ICountriesService
    {
        /// <summary>
        /// Adds a country object to the list of countries
        /// </summary>
        /// <param name="countryAddRequest">Country object to add</param>
        /// <returns>Returns the country object after adding it (
        /// including newly generated country id)</returns>
        Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest);
        
        /// <summary>
        /// Returns all countries from the list
        /// </summary>
        /// <returns>All countries from the list as List<CountryResponse></returns>
        Task<List<CountryResponse>> GetAllCountries();

        /// <summary>
        /// Return a country object based on the given country id
        /// </summary>
        /// <param name="countryID">countryID (guid) to search</param>
        /// <returns>Matching country as CountryResponse object</returns>
        Task<CountryResponse?> GetCountryByCountryID(Guid? countryID);
    }
}