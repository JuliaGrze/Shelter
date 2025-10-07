using Application.Dtos.Species;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Shelter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpeciesController : ControllerBase
    {
        private readonly ISpeciesService _speciesService;

        public SpeciesController(ISpeciesService speciesService)
        {
            _speciesService = speciesService;
        }

        // GET: api/species
        [HttpGet]
        public async Task<ActionResult<List<SpeciesDto>>> GetSpecies(CancellationToken ct)
            => Ok(await _speciesService.GetSpeciesAsync(ct));

        // GET: api/species/2
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SpeciesDto>> GetSpeciesById(int id, CancellationToken ct)
        {
            try
            {
                var dto = await _speciesService.GetSpecieByIdAsync(id, ct);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateSpecies([FromBody] CreateSpeciesDto speciesDto, CancellationToken ct)
        {
            try
            {
                var id = await _speciesService.AddSpecieAsync(speciesDto, ct);
                // 201 + Location: api/species/{id}
                return CreatedAtAction(nameof(GetSpeciesById), new { id }, id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }

        }

        // PUT: api/species/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSpecies(int id, [FromBody] CreateSpeciesDto speciesDto, CancellationToken ct)
        {
            try
            {
                var ok = await _speciesService.UpdateSpecieAsync(id, speciesDto, ct);
                return ok ? NoContent() : NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }

        }

        // DELETE: api/species/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSpecies(int id, CancellationToken ct)
        {
            var ok = await _speciesService.DeleteSpecieAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }


    }
}
