using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO.Countries;
using ServiceContracts.DTO.Persons;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
    [Route("[controller]")] // route token get before Controller class like PersonsController => /persons/
    public class PersonsController : Controller
    {
        private readonly IPersonService _personService;
        private readonly ICountriesService _countriesService;

        public PersonsController(IPersonService personService, ICountriesService countriesService)
        {
            _personService = personService;
            _countriesService = countriesService;
        }


        // Url: persons/index or /
        // The route can be sperate with overload method - HttpPost, HttpGet
        [HttpGet("[action]")] // the same name as action name
        [HttpGet("/")]
        public IActionResult Index(string searchBy,
                                    string? searchString,
                                    string sortBy = nameof(PersonResponse.PersonName),
                                    SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            // The SearchFields can improve with Factory Method Pattern
            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                { nameof(PersonResponse.PersonName), "Person Name" },
                { nameof(PersonResponse.Email), "Email" },
                { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
                { nameof(PersonResponse.Gender), "Gender" },
                { nameof(PersonResponse.CountryID), "Country" },
                { nameof(PersonResponse.Address), "Address" },
            };

            List<PersonResponse> allPersons = _personService.GetFilteredPersons(searchBy, searchString);

            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;

            // Sort
            List<PersonResponse> sortedAllPersons = _personService.GetSortedPersons(allPersons, sortBy, sortOrder);

            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder.ToString();

            return View(sortedAllPersons);
        }

        // Executes when the user clicks on "Create Person" hyperlink (while opening the create view)
        [HttpGet("[action]")]
        public IActionResult Create()
        {

            List<CountryResponse> countriesResponse = _countriesService.GetAllCountries();

            ViewBag.CountryList = countriesResponse;

            return View();
        }

        [HttpPost("[action]")] // route token +  attribute routing
        public IActionResult Create(PersonAddRequest personAddRequest)
        {
            if (!ModelState.IsValid)
            {
                List<CountryResponse> countriesResponse = _countriesService.GetAllCountries();
                ViewBag.CountryList = countriesResponse;    

                ViewBag.Errors  = ModelState.Values
                                            .SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();  
                return View();
            }

            PersonResponse personResponse = _personService.AddPerson(personAddRequest);

            // navigate to Index() action method (it makes another get request to "persons/index")
            return RedirectToAction("Index", "Persons");
        }
    }
}
