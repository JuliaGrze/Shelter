using Application.Dtos.Donations;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shelter.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonationsController : ControllerBase
    {
        private readonly IDonationService _svc;

        public DonationsController(IDonationService svc)
        {
            _svc = svc;
        }

        [HttpPost("checkout/one-time")]
        [AllowAnonymous]
        public async Task<ActionResult> CreateOneTime([FromBody] OneTimeDto dto, CancellationToken ct)
        {
            if (dto.AmountMinor <= 0) return BadRequest(new { message = "Amount must be > 0" });

            var url = await _svc.CreateOneTimeCheckoutSessionAsync(
                dto.AmountMinor,
                dto.Currency ?? "PLN",
                dto.DonorPublicName,
                dto.IsPublic,
                dto.Message,
                ct);

            return Ok(new { sessionUrl = url });
        }

        [HttpPost("checkout/recurring")]
        [AllowAnonymous]
        public async Task<ActionResult> CreateRecurring([FromBody] RecurringDto dto, CancellationToken ct)
        {
            if (dto.AmountMinor <= 0) return BadRequest(new { message = "Amount must be > 0" });

            var url = await _svc.CreateRecurringCheckoutSessionAsync(
                dto.AmountMinor,
                dto.Currency ?? "PLN",
                dto.DonorPublicName,
                dto.IsPublic,
                dto.Message,
                ct);

            return Ok(new { sessionUrl = url });
        }

        // teraz serwis czyta z bazy
        [HttpGet("public-latest")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DonorWallItemDto>>> PublicLatest([FromQuery] int take = 30, CancellationToken ct = default)
            => Ok(await _svc.GetPublicLatestAsync(take, ct));

        [HttpGet("by-month")]
        [Authorize(Roles = "Admin,Worker")]
        public async Task<ActionResult<IEnumerable<MonthlySumDto>>> ByMonth([FromQuery] int year, CancellationToken ct = default)
            => Ok(await _svc.GetMonthlySummaryAsync(year, ct));
    }
}
