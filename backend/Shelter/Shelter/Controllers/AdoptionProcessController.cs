using Application.Dtos.Adoptions;
using Application.Dtos.Adoptions.HomeVisit;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/adoptions")]
[Produces("application/json")]
public class AdoptionProcessController : ControllerBase
{
    private readonly IAdoptionProcessService _svc;
    public AdoptionProcessController(IAdoptionProcessService svc) => _svc = svc;

    private string? GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)   // "nameid"
        ?? User.FindFirst("sub")?.Value                  // "sub"
        ?? User.FindFirst("uid")?.Value                  // "uid"
        ?? User.FindFirst("userId")?.Value;              // "userId"

    // ===== Client: submit =====
    [HttpPost("{animalId:int}/apply")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(SubmitApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Submit(int animalId, [FromBody] SubmitApplicationRequest body, CancellationToken ct)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { error = "Missing user id claim in JWT." });

        body.AnimalId = animalId; // id z route
        var res = await _svc.SubmitAsync(body, userId, ct);
        return CreatedAtAction(nameof(GetApplication), new { id = res.ApplicationId }, res);
    }

    // ===== (optional) preview =====
    [HttpGet("applications/{id:int}")]
    public IActionResult GetApplication(int id) => NoContent(); // TODO

    // ===== Worker/Admin: statuses =====
    [HttpPost("applications/{id:int}/in-review")]
    [Authorize(Roles = "Worker,Admin")]
    public async Task<IActionResult> InReview(int id, [FromBody] UpdateAdoptionStatusRequest body, CancellationToken ct)
    {
        var reviewerId = GetUserId();
        if (string.IsNullOrWhiteSpace(reviewerId))
            return Unauthorized(new { error = "Missing user id claim in JWT." });

        await _svc.SetInReviewAsync(id, body, reviewerId, ct);
        return NoContent();
    }

    [HttpPost("applications/{id:int}/approve")]
    [Authorize(Roles = "Worker,Admin")]
    public async Task<IActionResult> Approve(int id, [FromBody] UpdateAdoptionStatusRequest body, CancellationToken ct)
    {
        var reviewerId = GetUserId();
        if (string.IsNullOrWhiteSpace(reviewerId))
            return Unauthorized(new { error = "Missing user id claim in JWT." });

        await _svc.ApproveAsync(id, body, reviewerId, ct);
        return NoContent();
    }

    [HttpPost("applications/{id:int}/reject")]
    [Authorize(Roles = "Worker,Admin")]
    public async Task<IActionResult> Reject(int id, [FromBody] UpdateAdoptionStatusRequest body, CancellationToken ct)
    {
        var reviewerId = GetUserId();
        if (string.IsNullOrWhiteSpace(reviewerId))
            return Unauthorized(new { error = "Missing user id claim in JWT." });

        await _svc.RejectAsync(id, body, reviewerId, ct);
        return NoContent();
    }

    // ===== Home visit =====
    [HttpPost("applications/{id:int}/home-visit/schedule")]
    [Authorize(Roles = "Worker,Admin")]
    public async Task<IActionResult> ScheduleHomeVisit(int id, [FromBody] ScheduleHomeVisitRequest body, CancellationToken ct)
    {
        await _svc.ScheduleHomeVisitAsync(id, body, ct);
        return NoContent();
    }

    [HttpPost("applications/{id:int}/home-visit/result")]
    [Authorize(Roles = "Worker,Admin")]
    public async Task<IActionResult> SetHomeVisitResult(int id, [FromBody] SetHomeVisitResultRequest body, CancellationToken ct)
    {
        await _svc.SetHomeVisitResultAsync(id, body, ct);
        return NoContent();
    }

    // ===== Contract =====
    [HttpPost("applications/{id:int}/contract/generate")]
    [Authorize(Roles = "Worker,Admin")]
    public async Task<IActionResult> GenerateContract(int id, CancellationToken ct)
    {
        var res = await _svc.GenerateContractAsync(id, ct);
        return Ok(res);
    }

    [HttpPost("applications/{id:int}/contract/sign")]
    [Authorize(Roles = "Client,Worker,Admin")]
    public async Task<IActionResult> Sign(int id, CancellationToken ct)
    {
        var signerId = GetUserId();
        if (string.IsNullOrWhiteSpace(signerId))
            return Unauthorized(new { error = "Missing user id claim in JWT." });

        await _svc.SignContractAsync(id, signerId, ct);
        return NoContent();
    }
}
