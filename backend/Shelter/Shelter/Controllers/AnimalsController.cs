using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Shelter.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnimalsController : ControllerBase
    {
        private readonly IAnimalService _animalService;
        public AnimalsController(IAnimalService animalService) => _animalService = animalService;

        // GET: api/animals
        [HttpGet]
        public async Task<ActionResult<List<AnimalDto>>> GetAnimals(CancellationToken ct)
            => Ok(await _animalService.GetAnimalsAsync(ct));

        // GET: api/animals/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AnimalDto>> GetAnimalById(int id, CancellationToken ct)
        {
            try
            {
                var dto = await _animalService.GetAnimalByIdAsync(id, ct);
                return Ok(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: api/animals
        [HttpPost]
        public async Task<ActionResult<int>> CreateAnimal([FromBody] CreateAnimalDto animalDto, CancellationToken ct)
        {
            var id = await _animalService.AddAnimalAsync(animalDto, ct);
            return Ok(id); // najprościej: 200 + { id }
        }

        // PUT: api/animals/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAnimal(int id, [FromBody] CreateAnimalDto animalDto, CancellationToken ct)
        {
            var ok = await _animalService.UpdateAnimalAsync(id, animalDto, ct);
            return ok ? NoContent() : NotFound();
        }

        // DELETE: api/animals/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAnimal(int id, CancellationToken ct)
        {
            var ok = await _animalService.DeleteAsync(id, ct); 
            return ok ? NoContent() : NotFound();
        }
    }
}
