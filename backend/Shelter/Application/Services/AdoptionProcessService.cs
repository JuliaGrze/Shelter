using Application.Common;
using Application.Dtos.Adoptions;
using Application.Dtos.Adoptions.HomeVisit;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums; // AdoptionStatusCodes
using Infrastructure.Repositories.Abstractions.Adoptions;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Stripe;
using System.IO;
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
            var app = await RequireAppAsync(appId, ct);   // z załadowanym AdoptionStatus
            EnsureNotFinal(app);

            var inReview = await RequireStatusAsync(AdoptionStatusCodes.InReview, ct);
            app.AdoptionStatusId = inReview.Id;
            app.AdoptionStatus = inReview;
            app.UpdatedAt = DateTime.UtcNow;
            app.Notes = MergeNotes(app.Notes, dto.Notes, "W TRAKCIE WERYFIKACJI");

            _uow.AdoptionApplications.Update(app);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task ApproveAsync(int appId, UpdateAdoptionStatusRequest dto, string reviewerUserId, CancellationToken ct = default)
        {
            var app = await RequireAppAsync(appId, ct);
            EnsureNotFinal(app);

            var approved = await RequireStatusAsync(AdoptionStatusCodes.Approved, ct);
            app.AdoptionStatusId = approved.Id;
            app.AdoptionStatus = approved;
            app.UpdatedAt = DateTime.UtcNow;
            app.Notes = MergeNotes(app.Notes, dto.Notes, "ZATWIERDZONO");

            _uow.AdoptionApplications.Update(app);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task RejectAsync(int appId, UpdateAdoptionStatusRequest dto, string reviewerUserId, CancellationToken ct = default)
        {
            var app = await RequireAppAsync(appId, ct);
            EnsureNotFinal(app);

            var rejected = await RequireStatusAsync(AdoptionStatusCodes.Rejected, ct);
            app.AdoptionStatusId = rejected.Id;
            app.AdoptionStatus = rejected;
            app.UpdatedAt = DateTime.UtcNow;
            app.Notes = MergeNotes(app.Notes, dto.Notes, "ODRZUCONO");

            _uow.AdoptionApplications.Update(app);
            await _uow.SaveChangesAsync(ct);
        }

        // ============= WIZYTA DOMOWA =============
        public async Task ScheduleHomeVisitAsync(int appId, ScheduleHomeVisitRequest dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var app = await RequireAppAsync(appId, ct);
            EnsureNotFinal(app);

            if (app.HomeVisit != null)
                throw new InvalidOperationException("Home visit already scheduled.");

            var scheduled = await RequireStatusAsync(AdoptionStatusCodes.HomeVisitScheduled, ct);

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

            var app = await RequireAppFullAsync(appId, ct);   // HomeVisit + HomeVisitResult załadowane
            EnsureNotFinal(app);

            if (app.HomeVisit == null)
                throw new InvalidOperationException("Home visit has not been scheduled.");

            var result = await _uow.HomeVisitResults.GetByIdAsync(dto.HomeVisitResultId, ct)
                ?? throw new KeyNotFoundException($"HomeVisitResult id={dto.HomeVisitResultId} not found.");

            app.HomeVisit.HomeVisitResultId = result.Id;
            app.HomeVisit.Notes = MergeNotes(app.HomeVisit.Notes, dto.Notes, "WYNIK WIZYTY");

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

            var now = DateTime.UtcNow;
            var hash = ComputeSha256($"{app.Id}|{app.ApplicationUserId}|{now:o}");
            var verification = $"/api/adoptions/contracts/verify?hash={hash}";

            var contract = new AdoptionContract
            {
                AdoptionApplicationId = app.Id,
                PdfUrl = string.Empty,  // ustawimy po zapisie pliku
                PdfHash = hash,
                VerificationQrContent = verification,
                CreatedAt = now
            };

            // 1) Wygeneruj PDF w pamięci
            var pdfBytes = BuildContractPdf(app, contract);

            // 2) Zapisz plik do wwwroot/contracts/{app.Id}/contract_{app.Id}.pdf
            var relUrl = $"/contracts/{app.Id}/contract_{app.Id}.pdf";
            var root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "contracts", app.Id.ToString());
            Directory.CreateDirectory(root);
            var physicalPath = Path.Combine(root, $"contract_{app.Id}.pdf");
            if (!System.IO.File.Exists(physicalPath))
                await System.IO.File.WriteAllBytesAsync(physicalPath, pdfBytes, ct);

            // 3) Zaktualizuj url i zapisz kontrakt
            contract.PdfUrl = relUrl;

            await _uow.AdoptionContracts.AddAsync(contract, ct);

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

        public async Task<PagedResult<AdoptionListItemDto>> ListAsync(string? status = null, string? q = null, int page = 1, int size = 20, CancellationToken ct = default)
        {
            page = page <= 0 ? 1 : page;
            size = size is <= 0 or > 100 ? 20 : size;

            var query = _uow.AdoptionApplications.QueryForList();

            // Filtrowanie po statusie (np. "InReview", "Submitted")
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(a => a.AdoptionStatus.Code == status);

            // Wyszukiwanie po e-mailu lub notatkach
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(a =>
                    (a.ApplicationUser!.Email ?? "").Contains(q) ||
                    (a.Notes ?? "").Contains(q));

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(a => new AdoptionListItemDto
                {
                    Id = a.Id,
                    AnimalId = a.AnimalId,
                    AnimalName = a.Animal!.Name,
                    AnimalSpecies = a.Animal!.Species!.Name,
                    AnimalPhotoUrl = a.Animal!.PhotoUrl,
                    ApplicantEmail = a.ApplicationUser!.Email!,
                    StatusCode = a.AdoptionStatus!.Code!,
                    StatusName = a.AdoptionStatus!.Name!,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync(ct);

            return new PagedResult<AdoptionListItemDto>(items, total, page, size);
        }

        public async Task<AdoptionDetailsDto> GetDetailsAsync(int appId, string? currentUserId, bool isStaff, CancellationToken ct = default)
        {
            var app = await _uow.AdoptionApplications.GetFullAsync(appId, ct)
                      ?? throw new KeyNotFoundException("Application not found.");

            if (!isStaff)
            {
                if (string.IsNullOrWhiteSpace(currentUserId) || app.ApplicationUserId != currentUserId)
                    throw new UnauthorizedAccessException("Not allowed.");
            }

            return new AdoptionDetailsDto
            {
                Id = app.Id,
                AnimalId = app.AnimalId,
                AnimalName = app.Animal?.Name ?? "",
                AnimalSpecies = app.Animal?.Species?.Name ?? "",
                AnimalPhotoUrl = app.Animal?.PhotoUrl,
                ApplicantUserId = app.ApplicationUserId,
                ApplicantEmail = app.ApplicationUser?.Email ?? "",
                StatusCode = app.AdoptionStatus?.Code ?? "",
                StatusName = app.AdoptionStatus.Name ?? "",
                Notes = app.Notes,
                CreatedAt = app.CreatedAt,
                HomeVisit = app.HomeVisit == null
                    ? null
                    : new AdoptionDetailsDto.HomeVisitBlock(
                        app.HomeVisit.Date,
                        app.HomeVisit.HomeVisitResult?.Code,
                        app.HomeVisit.Notes
                      ),
                Contract = app.Contract == null
                    ? new AdoptionDetailsDto.ContractBlock(false, false, null)
                    : new AdoptionDetailsDto.ContractBlock(
                        true,
                        app.Contract.SignedAt != null,
                        string.IsNullOrWhiteSpace(app.Contract.PdfUrl) ? null : app.Contract.PdfUrl
                      )
            };
        }


        public async Task<PagedResult<AdoptionListItemDto>> ListMineAsync(string currentUserId, int page = 1, int size = 20, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
                throw new ArgumentException("currentUserId is required.", nameof(currentUserId));

            page = page <= 0 ? 1 : page;
            size = size is <= 0 or > 100 ? 20 : size;

            var q = _uow.AdoptionApplications.QueryForList()
                .Where(x => x.ApplicationUserId == currentUserId)
                .OrderByDescending(x => x.CreatedAt);

            var total = await q.CountAsync(ct);

            var items = await q
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new AdoptionListItemDto
                {
                    Id = x.Id,
                    AnimalId = x.AnimalId,
                    AnimalName = x.Animal!.Name,
                    AnimalSpecies = x.Animal!.Species!.Name,
                    AnimalPhotoUrl = x.Animal!.PhotoUrl,
                    ApplicantEmail = x.ApplicationUser!.Email!,
                    StatusCode = x.AdoptionStatus!.Code!,
                    StatusName = x.AdoptionStatus!.Name!,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(ct);

            return new PagedResult<AdoptionListItemDto>(items, total, page, size);
        }


        // ============= Helpers =============
        private async Task<AdoptionApplication> RequireAppAsync(int id, CancellationToken ct)
        {
            // ważne: musi ładować AdoptionStatus, bo EnsureNotFinal na nim polega
            var app = await _uow.AdoptionApplications.GetByIdWithStatusAsync(id, ct);
            if (app == null)
                throw new KeyNotFoundException($"Adoption application id={id} not found.");
            return app;
        }

        private async Task<AdoptionApplication> RequireAppFullAsync(int id, CancellationToken ct)
        {
            var app = await _uow.AdoptionApplications.GetFullAsync(id, ct);
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

        private static void EnsureNotFinal(AdoptionApplication app)
        {
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

        private static byte[] BuildContractPdf(AdoptionApplication app, AdoptionContract contract)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(36);

                    page.Header().Text("Umowa adopcyjna")
                        .FontSize(20).SemiBold();

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Wniosek ID: {app.Id}");
                        col.Item().Text($"Adoptujący (UserId): {app.ApplicationUserId}");
                        col.Item().Text($"Zwierzę (AnimalId): {app.AnimalId}");
                        col.Item().Text($"Wygenerowano: {contract.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
                        col.Item().Text($"Weryfikacja: {contract.VerificationQrContent}");
                    });

                    page.Footer().AlignRight().Text(t =>
                    {
                        t.Span("Strona ");
                        t.CurrentPageNumber();
                        t.Span(" / ");
                        t.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        
    }
}
