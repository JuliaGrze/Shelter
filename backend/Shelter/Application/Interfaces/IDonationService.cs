using Application.Dtos.Donations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IDonationService
    {
        /// <summary>
        /// Tworzy sesję Stripe Checkout dla jednorazowej darowizny.
        /// Zwraca adres URL, pod który frontend powinien przekierować użytkownika
        /// w celu dokończenia płatności (np. strona Stripe Checkout).
        /// </summary>
        Task<string> CreateOneTimeCheckoutSessionAsync(long amountMinor, string currency,
            string? donorPublicName, bool isPublic, string? message, CancellationToken ct);

        /// <summary>
        /// Tworzy sesję Stripe Checkout dla darowizny cyklicznej (subskrypcji).
        /// Wymaga, aby w Stripe istniał <c>Price</c> powiązany z produktem subskrypcyjnym.
        /// </summary>
        Task<string> CreateRecurringCheckoutSessionAsync(long amountMinor,
            string currency, string? donorPublicName, bool isPublic, string? message, CancellationToken ct);

        /// <summary>
        /// Obsługuje webhook Stripe — przetwarza zdarzenia takie jak:
        /// <list type="bullet">
        /// <item><description><c>payment_intent.succeeded</c> — udana płatność jednorazowa</description></item>
        /// <item><description><c>invoice.paid</c> — udane odnowienie subskrypcji</description></item>
        /// <item><description><c>invoice.payment_failed</c> — nieudana płatność cykliczna</description></item>
        /// </list>
        /// </summary>
        Task HandleWebhookAsync(string json, string stripeSignatureHeader, CancellationToken ct);

        Task<List<DonorWallItemDto>> GetPublicLatestAsync(int take, CancellationToken ct);
        Task<List<MonthlySumDto>> GetMonthlySummaryAsync(int year, CancellationToken ct);
    }

}
