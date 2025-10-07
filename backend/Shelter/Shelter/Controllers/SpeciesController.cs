using Application.Dtos;
using Application.Interfaces;
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
        => Ok(await _speciesService.GetSpeciesAsync());

    }
}
