using Application.Dtos.Adoptions;
using Application.Dtos.Adoptions.HomeVisit;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums; // AdoptionStatusCodes
using Infrastructure.Repositories.Abstractions.Adoptions;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class AdoptionProcessService : IAdoptionProcessService
    {
        private readonly IAdoptionUnitOfWork _uow;

        public AdoptionProcessService(IAdoptionUnitOfWork uow)
        {
            _uow = uow;
        }

        // ============= SUBMIT =============

        public async Task<SubmitApplicationResponse> SubmitAsync(SubmitApplicationRequest dto, string applicantUserId, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            if (string.IsNullOrWhiteSpace(applicantUserId))
                throw new ArgumentException("Applicant user id is required.", nameof(applicantUserId));

            var status = await RequireStatusAsync(AdoptionStatusCodes.Submitted, ct);

            var app = new AdoptionApplication
            {
                AnimalId = dto.AnimalId,
                ApplicationUserId = applicantUserId,
                AdoptionStatusId = status.Id,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.AdoptionApplications.AddAsync(app, ct);
            await _uow.SaveChangesAsync(ct);

            return new SubmitApplicationResponse
            {
                ApplicationId = app.Id,
                CreatedAt = app.CreatedAt,
                StatusCode = status.Code,
                StatusName = status.Name
            };
        }

        // ============= STATUSY =============

        public async Task SetInReviewAsync(int appId, UpdateAdoptionStatusRequest dto, string reviewerUserId, CancellationToken ct = default)
        {
            var app = await RequireAppAsync(appId, ct);
            EnsureNotFinal(app);

            var inReview = await RequireStatusAsync(AdoptionStatusCodes.InReview, ct);
            app.AdoptionStatusId = inReview.Id;
            app.UpdatedAt = DateTime.UtcNow;
            app.Notes = MergeNotes(app.Notes, dto.Notes, prefix: "IN_REVIEW");

            _uow.AdoptionApplications.Update(app);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task ApproveAsync(int appId, UpdateAdoptionStatusRequest dto, string reviewerUserId, CancellationToken ct = default)
        {
            var app = await RequireAppAsync(appId, ct);
            EnsureNotFinal(app);

            var approved = await RequireStatusAsync(AdoptionStatusCodes.Approved, ct);
            app.AdoptionStatusId = approved.Id;
            app.UpdatedAt = DateTime.UtcNow;
            app.Notes = MergeNotes(app.Notes, dto.Notes, prefix: "APPROVED");

            _uow.AdoptionApplications.Update(app);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task RejectAsync(int appId, UpdateAdoptionStatusRequest dto, string reviewerUserId, CancellationToken ct = default)
        {
            var app = await RequireAppAsync(appId, ct);
            EnsureNotFinal(app);

            var rejected = await RequireStatusAsync(AdoptionStatusCodes.Rejected, ct);
            app.AdoptionStatusId = rejected.Id;
            app.UpdatedAt = DateTime.UtcNow;
            app.Notes = MergeNotes(app.Notes, dto.Notes, prefix: "REJECTED");

            _uow.AdoptionApplications.Update(app);
            await _uow.SaveChangesAsync(ct);
        }

        // ============= WIZYTA DOMOWA =============

        public async Task ScheduleHomeVisitAsync(int appId, ScheduleHomeVisitRequest dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var app = await RequireAppAsync(appId, ct);
            EnsureNotFinal(app);

            // Status -> HomeVisitScheduled
            var scheduled = await RequireStatusAsync(AdoptionStatusCodes.HomeVisitScheduled, ct);

            // Wynik wizyty startowo "Pending"
            var pendingResult = await _uow.HomeVisitResults.GetByCodeAsync("Pending", ct)
                ?? throw new InvalidOperationException("HomeVisitResult 'Pending' not found. Seed required.");

            app.HomeVisit = new HomeVisit
            {
                AdoptionApplicationId = app.Id,
                Date = dto.Date,
                HomeVisitResultId = pendingResult.Id,
                Notes = dto.Notes
            };

            app.AdoptionStatusId = scheduled.Id;
            app.UpdatedAt = DateTime.UtcNow;

            _uow.AdoptionApplications.Update(app);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task SetHomeVisitResultAsync(int appId, SetHomeVisitResultRequest dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var app = await RequireAppFullAsync(appId, ct);  
            EnsureNotFinal(app);

            if (app.HomeVisit == null)
                throw new InvalidOperationException("Home visit has not been scheduled.");

            var result = await _uow.HomeVisitResults.GetByIdAsync(dto.HomeVisitResultId, ct)
                ?? throw new KeyNotFoundException($"HomeVisitResult id={dto.HomeVisitResultId} not found.");

            app.HomeVisit.HomeVisitResultId = result.Id;
            app.HomeVisit.Notes = MergeNotes(app.HomeVisit.Notes, dto.Notes, prefix: "VISIT_RESULT");

            var completed = await RequireStatusAsync(AdoptionStatusCodes.HomeVisitCompleted, ct);
            app.AdoptionStatusId = completed.Id;
            app.UpdatedAt = DateTime.UtcNow;

            _uow.AdoptionApplications.Update(app);
            await _uow.SaveChangesAsync(ct);
        }


        // ============= UMOWA =============

        public async Task<GenerateContractResponse> GenerateContractAsync(int appId, CancellationToken ct = default)
        {
            var app = await RequireAppAsync(appId, ct);
            EnsureNotFinal(app);

            // Jeśli już istnieje to nie ge
            var existing = await _uow.AdoptionContracts.GetByApplicationIdAsync(app.Id, ct);
            if (existing != null)
            {
                return new GenerateContractResponse
                {
                    ContractId = existing.Id,
                    PdfUrl = existing.PdfUrl,
                    PdfHash = existing.PdfHash,
                    VerificationQrContent = existing.VerificationQrContent,
                    GeneratedAtUtc = existing.CreatedAt
                };
            }

            // Mock wygenerowanego PDF (tu tylko metadane i weryfikacja)
            var pdfUrl = $"/contracts/{app.Id}/contract_{app.Id}.pdf";
            var now = DateTime.UtcNow;
            var hash = ComputeSha256($"{app.Id}|{app.ApplicationUserId}|{now:o}");
            var verification = $"/api/adoptions/contracts/verify?hash={hash}";

            var contract = new AdoptionContract
            {
                AdoptionApplicationId = app.Id,
                PdfUrl = pdfUrl,
                PdfHash = hash,
                VerificationQrContent = verification,
                CreatedAt = now
            };

            await _uow.AdoptionContracts.AddAsync(contract, ct);

            // Status -> ContractGenerated
            var generated = await RequireStatusAsync(AdoptionStatusCodes.ContractGenerated, ct);
            app.AdoptionStatusId = generated.Id;
            app.UpdatedAt = now;
            _uow.AdoptionApplications.Update(app);

            await _uow.SaveChangesAsync(ct);

            return new GenerateContractResponse
            {
                ContractId = contract.Id,
                PdfUrl = contract.PdfUrl,
                PdfHash = contract.PdfHash,
                VerificationQrContent = contract.VerificationQrContent,
                GeneratedAtUtc = contract.CreatedAt
            };
        }

        public async Task SignContractAsync(int appId, string signerUserId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(signerUserId))
                throw new ArgumentException("Signer user id is required.", nameof(signerUserId));

            var app = await RequireAppAsync(appId, ct);
            EnsureNotFinal(app);

            var contract = await _uow.AdoptionContracts.GetByApplicationIdAsync(app.Id, ct)
                ?? throw new InvalidOperationException("Contract has not been generated yet.");

            if (contract.SignedAt != null)
                throw new InvalidOperationException("Contract already signed.");

            contract.SignedAt = DateTime.UtcNow;
            contract.SignedByUserId = signerUserId;

            _uow.AdoptionContracts.Update(contract);

            var signed = await RequireStatusAsync(AdoptionStatusCodes.ContractSigned, ct);
            app.AdoptionStatusId = signed.Id;
            app.UpdatedAt = DateTime.UtcNow;
            _uow.AdoptionApplications.Update(app);

            await _uow.SaveChangesAsync(ct);
        }

        // ============= Helpers =============

        private async Task<AdoptionApplication> RequireAppAsync(int id, CancellationToken ct)
        {
            var app = await _uow.AdoptionApplications.GetByIdAsync(id, ct);
            if (app == null)
                throw new KeyNotFoundException($"Adoption application id={id} not found.");
            return app;
        }

        private async Task<AdoptionStatus> RequireStatusAsync(string code, CancellationToken ct)
        {
            var status = await _uow.AdoptionStatuses.GetByCodeAsync(code, ct);
            if (status == null)
                throw new InvalidOperationException($"AdoptionStatus '{code}' not found. Seed required.");
            return status;
        }

        private async Task<AdoptionApplication> RequireAppFullAsync(int id, CancellationToken ct)
        {
            var app = await _uow.AdoptionApplications.GetFullAsync(id, ct);
            if (app == null)
                throw new KeyNotFoundException($"Adoption application id={id} not found.");
            return app;
        }


        private static void EnsureNotFinal(AdoptionApplication app)
        {
            // Jeśli aplikacja jest w stanie finalnym (np. Rejected/Withdrawn/ContractSigned),
            // nie pozwalamy na dalsze modyfikacje procesu.
            var finalCodes = new[]
            {
                AdoptionStatusCodes.Rejected,
                AdoptionStatusCodes.Withdrawn,
                AdoptionStatusCodes.ContractSigned
            };

            if (app.AdoptionStatus != null && finalCodes.Contains(app.AdoptionStatus.Code))
                throw new InvalidOperationException($"Application is in final state '{app.AdoptionStatus.Code}'.");
        }

        private static string MergeNotes(string? existing, string? append, string prefix)
        {
            if (string.IsNullOrWhiteSpace(append))
                return existing ?? string.Empty;

            var tag = $"[{prefix} {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC]";
            if (string.IsNullOrWhiteSpace(existing))
                return $"{tag} {append}";

            return $"{existing}\n{tag} {append}";
        }

        private static string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
