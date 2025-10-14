using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Shelter.API.Controllers
{
    [ApiController]
    [Route("api/stripe")] // końcowy adres: /api/stripe/webhook
    public class StripeWebhookController : ControllerBase
    {
        private readonly IDonationService _svc;
        public StripeWebhookController(IDonationService svc) => _svc = svc;

        [HttpPost("webhook")]
        [IgnoreAntiforgeryToken] // dla pewności w dev (jeśli masz antiforgery)
        public async Task<IActionResult> Webhook(CancellationToken ct)
        {
            var signature = Request.Headers["Stripe-Signature"].ToString();
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();

            await _svc.HandleWebhookAsync(json, signature, ct);
            return Ok(); // Stripe oczekuje 2xx
        }
    }
}
