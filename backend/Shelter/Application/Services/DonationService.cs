using Application.Dtos.Donations;
using Application.Interfaces;
using Application.Settings;
using Domain.Entities;
using Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;            // AnyAsync, FirstOrDefaultAsync
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Application.Services
{
    public class DonationService : IDonationService
    {
        private readonly IGenericRepository<Donation> _donationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly StripeSettings _stripeSetting;

        public DonationService(
            IGenericRepository<Donation> donationRepository,
            IUnitOfWork unitOfWork,
            IOptions<StripeSettings> cfg)
        {
            _donationRepository = donationRepository;
            _unitOfWork = unitOfWork;
            _stripeSetting = cfg.Value;
        }

        //jednorazowa płatnosc
        public async Task<string> CreateOneTimeCheckoutSessionAsync(
            long amountMinor,
            string currency,
            string? donorPublicName,
            bool isPublic,
            string? message,
            CancellationToken ct)
        {
            var domain = "http://localhost:4200"; // TODO: przenieść do konfiguracji

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                //session_id={{CHECKOUT_SESSION_ID}} → Stripe automatycznie podmienia ten token na ID sesji Stripe Checkout
                SuccessUrl = $"{domain}/donate/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/donate/cancel",

                // LineItems – lista pozycji, które trafią na stronę Stripe Checkout.
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        // pojedyncza pozycja koszyka
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency   = currency.ToLowerInvariant(),
                            UnitAmount = amountMinor,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Darowizna dla Schroniska"
                            }
                        },
                        Quantity = 1
                    }
                },
                // słownik (key → value), w którym możesz umieścić dowolne własne dane tekstowe
                // Stripe zachowa je razem z płatnością / sesją i odeśle Ci z powrotem w webhookach
                Metadata = new Dictionary<string, string>
                {
                    ["donorPublicName"] = donorPublicName ?? string.Empty,
                    ["isPublic"] = isPublic ? "true" : "false",
                    ["message"] = message ?? string.Empty
                }
            };

            //tworzy sesje Checkout
            var service = new SessionService();
            var session = await service.CreateAsync(options, requestOptions: null, cancellationToken: ct);

            // Front: window.location = returnedUrl;
            return session.Url!;
        }

        //subskrypcja/cykliczna płatnosc
        public async Task<string> CreateRecurringCheckoutSessionAsync(
            string priceId,
            string? donorPublicName,
            bool isPublic,
            string? message,
            CancellationToken ct)
        {
            var domain = "http://localhost:4200"; 

            var options = new SessionCreateOptions
            {
                Mode = "subscription",
                SuccessUrl = $"{domain}/donate/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/donate/cancel",
                LineItems = new List<SessionLineItemOptions>
                {
                    new() { Price = priceId, Quantity = 1 }
                },
                Metadata = new Dictionary<string, string>
                {
                    ["donorPublicName"] = donorPublicName ?? string.Empty,
                    ["isPublic"] = isPublic ? "true" : "false",
                    ["message"] = message ?? string.Empty
                }
            };

            var session = await new SessionService().CreateAsync(options, requestOptions: null, cancellationToken: ct);
            return session.Url!;
        }

        public async Task HandleWebhookAsync(string json, string stripeSignatureHeader, CancellationToken ct)
        {
            // Weryfikacja podpisu webhooka (używamy WebhookSecret)
            Console.WriteLine("Stripe signature: " + stripeSignatureHeader);
            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignatureHeader, _stripeSetting.WebhookSecret);

            switch (stripeEvent.Type)
            {
                // Ukończony Stripe Checkout (jednorazowy lub subskrypcja)
                case "checkout.session.completed":
                    {
                        var session = stripeEvent.Data.Object as Session;
                        if (session is null) return;

                        var isRecurring = session.Mode == "subscription";
                        var paymentIntentId = session.PaymentIntentId;      // dla jednorazowych
                        var customerId = session.CustomerId;
                        var amount = session.AmountTotal ?? 0;
                        var currency = (session.Currency ?? "pln").ToUpperInvariant();

                        session.Metadata.TryGetValue("donorPublicName", out var name);
                        session.Metadata.TryGetValue("isPublic", out var isPublicStr);
                        session.Metadata.TryGetValue("message", out var msg);

                        if (!string.IsNullOrEmpty(paymentIntentId))
                        {
                            var already = await _donationRepository.Query()
                                .AnyAsync(d => d.StripePaymentIntentId == paymentIntentId, ct);

                            if (!already)
                            {
                                await _donationRepository.AddAsync(new Donation
                                {
                                    AmountMinor = amount,
                                    Currency = currency,
                                    DonorPublicName = string.IsNullOrWhiteSpace(name) ? null : name,
                                    IsRecurring = isRecurring,
                                    StripePaymentIntentId = paymentIntentId,
                                    StripeCustomerId = customerId,
                                    IsPublic = string.Equals(isPublicStr, "true", StringComparison.OrdinalIgnoreCase),
                                    Message = string.IsNullOrWhiteSpace(msg) ? null : msg
                                }, ct);

                                await _unitOfWork.SaveChangesAsync(ct);
                            }
                        }

                        break;
                    }

                // Pomyślnie opłacona faktura subskrypcyjna (cykliczna wpłata)
                case "invoice.paid":
                    {
                        var invoice = stripeEvent.Data.Object as Invoice;
                        if (invoice is null) break;

                        // Kwota: preferuj rzeczywiście zapłaconą; w starszych wersjach użyj Total
                        long amountMinor = invoice.AmountPaid; // jeśli masz błędy kompilacji, zamień na: var amountMinor = invoice.Total;

                        var currency = (invoice.Currency ?? "pln").ToUpperInvariant();
                        var customerId = invoice.CustomerId;
                        var invoiceId = invoice.Id; // użyjemy tego do idempotencji

                        // Idempotencja po ID faktury (zamiast PaymentIntentId)
                        var exists = await _donationRepository.Query()
                            .AnyAsync(d => d.StripePaymentIntentId == invoiceId, ct);
                        if (exists) break;

                        // Spróbuj odziedziczyć preferencje darczyńcy z ostatniej cyklicznej wpłaty
                        string? donorPublicName = null;
                        bool isPublic = false;
                        string? message = null;

                        if (!string.IsNullOrEmpty(customerId))
                        {
                            var lastForCustomer = await _donationRepository.Query()
                                .Where(d => d.StripeCustomerId == customerId && d.IsRecurring)
                                .OrderByDescending(d => d.CreatedAt) // zakładam, że masz CreatedAt
                                .FirstOrDefaultAsync(ct);

                            if (lastForCustomer is not null)
                            {
                                donorPublicName = lastForCustomer.DonorPublicName;
                                isPublic = lastForCustomer.IsPublic;
                                message = lastForCustomer.Message;
                            }
                        }

                        await _donationRepository.AddAsync(new Donation
                        {
                            AmountMinor = amountMinor,
                            Currency = currency,
                            DonorPublicName = string.IsNullOrWhiteSpace(donorPublicName) ? null : donorPublicName,
                            IsRecurring = true,
                            // Użyjemy invoice.Id w polu StripePaymentIntentId żeby zapewnić idempotencję.
                            // (Jeśli dodasz w modelu osobne pole StripeInvoiceId, przenieś to tam.)
                            StripePaymentIntentId = invoiceId,
                            StripeCustomerId = customerId,
                            IsPublic = isPublic,
                            Message = string.IsNullOrWhiteSpace(message) ? null : message
                        }, ct);

                        await _unitOfWork.SaveChangesAsync(ct);
                        break;
                    }

                // Nieudana płatność cykliczna (np. karta wygasła/odmowa banku)
                case "invoice.payment_failed":
                    {
                        var invoice = stripeEvent.Data.Object as Invoice;
                        if (invoice is null) break;

                        var customerId = invoice.CustomerId;
                        var invoiceId = invoice.Id;
                        var currency = (invoice.Currency ?? "pln").ToUpperInvariant();

                        // W wielu wersjach Stripe.NET AmountDue to long (bez ?), więc bez operatora ??
                        long amountMinor = invoice.AmountDue; // jeśli nie ma, użyj invoice.Total;

                        // Nie tworzymy wpisu Donation, żeby nie psuć statystyk.
                        // Tu możesz:
                        // - zalogować zdarzenie,
                        // - wysłać powiadomienie do darczyńcy/admina,
                        // - zapisać rekord do własnej tabeli (np. DonationCycle z Status = "Failed").

                        // Przykładowe proste logowanie:
                        // _logger.LogWarning("invoice.payment_failed: customer={CustomerId}, invoice={InvoiceId}, amountMinor={Amount}, currency={Currency}",
                        //     customerId, invoiceId, amountMinor, currency);

                        break;
                    }


                default:
                    // Na razie pomijamy pozostałe zdarzenia
                    break;
            }
        }

        public async Task<List<DonorWallItemDto>> GetPublicLatestAsync(int take, CancellationToken ct)
        {
            take = Math.Clamp(take, 1, 100);

            var query = _donationRepository
                .Query()
                .AsNoTracking()
                .Where(d => d.IsPublic)
                .OrderByDescending(d => d.CreatedAt)
                .Take(take)
                .Select(d => new DonorWallItemDto(
                    d.DonorPublicName ?? "Anonim",
                    d.AmountMinor,
                    d.Currency,
                    d.CreatedAt
                ));

            return await query.ToListAsync(ct);
        }

        public async Task<List<MonthlySumDto>> GetMonthlySummaryAsync(int year, CancellationToken ct)
        {
            if (year <= 0) year = DateTime.UtcNow.Year;

            var query = _donationRepository
                .Query()
                .AsNoTracking()
                .Where(d => d.CreatedAt.Year == year)
                .GroupBy(d => new { d.CreatedAt.Year, d.CreatedAt.Month, d.Currency })
                .Select(g => new MonthlySumDto(
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Currency,
                    g.Sum(x => x.AmountMinor),
                    g.Count()
                ))
                .OrderBy(x => x.Month);

            return await query.ToListAsync(ct);
        }
    }
}
