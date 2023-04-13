using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public async Task<IActionResult> Index( string searchBy,
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

            List<PersonResponse> allPersons = await _personService.GetFilteredPersons(searchBy, searchString);

            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;

            // Sort
            List<PersonResponse> sortedAllPersons = await _personService.GetSortedPersons(allPersons, sortBy, sortOrder);

            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder.ToString();

            return View(sortedAllPersons);
        }

        // Executes when the user clicks on "Create Person" hyperlink (while opening the create view)
        [HttpGet("[action]")]
        public async Task<IActionResult> Create()
        {
            List<CountryResponse> countriesResponse = await _countriesService.GetAllCountries();

            ViewBag.CountryList = countriesResponse.Select(c => new SelectListItem()
            {
                Text = c.CountryName,
                Value = c.CountryID.ToString(),
            });

            // <option value="1">An</option>
            //new SelectListItem()
            //{
            //    Text="An", Value="1"
            //};
            

            return View();
        }

        [HttpPost("[action]")] // route token +  attribute routing
        public async Task<IActionResult> Create(PersonAddRequest personAddRequest)
        {
            if (!ModelState.IsValid)
            {
                List<CountryResponse> countriesResponse = await _countriesService.GetAllCountries();
                ViewBag.CountryList = countriesResponse;    

                ViewBag.Errors  = ModelState.Values
                                            .SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();  
                return View(personAddRequest);
            }

            PersonResponse personResponse = await _personService.AddPerson(personAddRequest);

            // navigate to Index() action method (it makes another get request to "persons/index")
            return RedirectToAction("Index", "Persons");
        }

        // Eg: /person/edit/1
        [HttpGet("[action]/{personID}")] 
        public async Task<IActionResult> Edit(Guid personID)
        {
            PersonResponse? personResponse = await _personService.GetPersonByPersonID(personID);

            if (personResponse == null) 
            {
                return RedirectToAction("Index", "Persons");
            }

            // nên để ở dưới Service tạo thêm 1 function như GetPersonByPersonID generic
            // hoặc tạo thêm 1 method để return về như GetPersonUpdateByPersonID
            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            List<CountryResponse> countriesResponse = await _countriesService.GetAllCountries();

            ViewBag.CountryList = countriesResponse.Select(c => new SelectListItem()
            {
                Text = c.CountryName,
                Value = c.CountryID.ToString(),
            });

            return View(personUpdateRequest);
        }

        [HttpPost("[action]/{personID}")]
        public async Task<IActionResult> Edit(PersonUpdateRequest personUpdateRequest) 
        {
            PersonResponse? personResponse = await _personService.GetPersonByPersonID(personUpdateRequest.PersonID);

            if (personResponse == null)
            {
                return RedirectToAction("Index", "Persons");
            }

            if (!ModelState.IsValid)
            {
                List<CountryResponse> countriesResponse = await _countriesService.GetAllCountries();
                ViewBag.CountryList = countriesResponse;

                ViewBag.Errors = ModelState.Values
                                            .SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();

                // return the same view to remain all information of form
                return View(personResponse.ToPersonUpdateRequest());
            }

            PersonResponse personResponseAfterUpdate = await _personService.UpdatePerson(personUpdateRequest);

            return RedirectToAction("Index", "Persons");
        }

        [HttpGet("[action]/{personID}")]
        public async Task<IActionResult> Delete(Guid personID)
        {
            PersonResponse? personResponse = await _personService.GetPersonByPersonID(personID);

            if (personResponse == null)
                return RedirectToAction("Index");           

            return View(personResponse);
        }

        [HttpPost("[action]/{personID}")]
        public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateRequest)
        {
            PersonResponse? personResponse = await _personService.GetPersonByPersonID(personUpdateRequest.PersonID);

            if (personResponse == null)
                return RedirectToAction("Index");

            await _personService.DeletePerson(personResponse.PersonID);

            return RedirectToAction("Index");
        }
    }
}
