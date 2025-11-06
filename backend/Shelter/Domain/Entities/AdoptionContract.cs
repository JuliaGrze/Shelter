using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    /// <summary>
    /// Umowa adopcyjna powiązana 1–1 z wnioskiem AdoptionApplication.
    /// Przechowuje ścieżkę do PDF, hash do weryfikacji i info o podpisie (mock e-podpis).
    /// </summary>
    public class AdoptionContract
    {
        public int Id { get; set; }

        // FK do wniosku (relacja 1–1)
        public int AdoptionApplicationId { get; set; }
        public AdoptionApplication AdoptionApplication { get; set; } = default!;

        // Plik i weryfikacja
        public string PdfUrl { get; set; } = default!;           // np. /contracts/{appId}/contract_{appId}.pdf
        public string PdfHash { get; set; } = default!;          // SHA-256 treści PDF
        public string VerificationQrContent { get; set; } = default!; // pełny adres do verify endpoint (z hash)

        // Podpis (mock e-podpis)
        public DateTime? SignedAt { get; set; }
        public string? SignedByUserId { get; set; }              // kto podpisał (ApplicationUser.Id)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }                        // opcjonalne uwagi (np. wariant umowy)
    }
}
