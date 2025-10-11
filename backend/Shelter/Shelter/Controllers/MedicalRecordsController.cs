using Application.Dtos.MedicalRecord;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Shelter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordService _mediicalRecordService;
        public MedicalRecordsController(IMedicalRecordService medicalRecordService)
        {
            _mediicalRecordService = medicalRecordService;
        }

        [HttpGet("animal/{animalId:int}")]
        public async Task<ActionResult<List<MedicalRecordDto>>> GetMedicalServiceByAnimal(int animalId, CancellationToken ct = default)
        {
            var  items = await _mediicalRecordService.GetMedicalRecordByAnimalAsync(animalId, ct);
            return Ok(items);
        }

        //[Authorize(Roles = "Admin,Worker")]
        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateMedicalRecordDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var id = await _mediicalRecordService.CreateMedicalRecordAsync(dto, ct);
            return Ok(id);
        }

        [Authorize(Roles = "Admin,Worker")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateMedicalRecordDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var ok = await _mediicalRecordService.UpdateAsync(id, dto, ct);
            if (!ok) return NotFound(new { message = "Medical record not found." });
            return NoContent();
        }

        [Authorize(Roles = "Admin,Worker")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var ok = await _mediicalRecordService.DeleteAsync(id, ct);
            if (!ok) return NotFound(new { message = "Medical record not found." });
            return NoContent();
        }

    }
}
