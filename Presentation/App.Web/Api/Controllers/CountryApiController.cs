using App.Core.Domain.Directory;
using App.Services.Directory;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountryApiController : ControllerBase
    {
        private readonly ICountryService _countryService;

        public CountryApiController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        /// <summary>
        /// Get all countries.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var countries = await _countryService.GetAllAsync();
            if (countries == null || !countries.Any())
                return NotFound(new { message = "No countries found." });

            return Ok(countries.Select(c => new
            {
                c.Id,
                c.Name,
                c.TwoLetterIsoCode,
                c.ThreeLetterIsoCode,
                c.NumericIsoCode,
                c.Published,
                c.DisplayOrder
            }));
        }

        /// <summary>
        /// Get a single country by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var country = await _countryService.GetByIdAsync(id);
            if (country == null)
                return NotFound(new { message = "Country not found." });

            return Ok(new
            {
                country.Id,
                country.Name,
                country.TwoLetterIsoCode,
                country.ThreeLetterIsoCode,
                country.NumericIsoCode,
                country.Published,
                country.DisplayOrder
            });
        }

        /// <summary>
        /// Create a new country.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Country model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _countryService.InsertAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }

        /// <summary>
        /// Update an existing country.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Country model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _countryService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Country not found." });

            existing.Name = model.Name;
            existing.TwoLetterIsoCode = model.TwoLetterIsoCode;
            existing.ThreeLetterIsoCode = model.ThreeLetterIsoCode;
            existing.NumericIsoCode = model.NumericIsoCode;
            existing.Published = model.Published;
            existing.DisplayOrder = model.DisplayOrder;

            await _countryService.UpdateAsync(existing);
            return Ok(existing);
        }

        /// <summary>
        /// Delete a country by ID.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var country = await _countryService.GetByIdAsync(id);
            if (country == null)
                return NotFound(new { message = "Country not found." });

            await _countryService.DeleteAsync(id);
            return NoContent();
        }
    }
}
