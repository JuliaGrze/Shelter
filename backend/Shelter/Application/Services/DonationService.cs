using Application.Dtos.Donations;
using Application.Interfaces;
using Application.Settings;
using Domain.Entities;
using Infrastructure.Repositories.Abstractions;
using Microsoft.Data.SqlClient;            // <— dla SQL Server
using Microsoft.EntityFrameworkCore;
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

        // ---------------------------
        // Jednorazowa płatność
        // ---------------------------
        public async Task<string> CreateOneTimeCheckoutSessionAsync(
            long amountMinor,
            string currency,
            string? donorPublicName,
            bool isPublic,
            string? message,
            CancellationToken ct)
        {
            var domain = "http://localhost:4200"; // TODO: do configu

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = $"{domain}/donate/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/donate/cancel",
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
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

        // ---------------------------
        // Subskrypcja (miesięczna) – dynamiczna kwota
        // ---------------------------
        public async Task<string> CreateRecurringCheckoutSessionAsync(
            long amountMinor,
            string currency,
            string? donorPublicName,
            bool isPublic,
            string? message,
            CancellationToken ct)
        {
            var domain = "http://localhost:4200"; // TODO: do configu

            var commonMeta = new Dictionary<string, string>
            {
                ["donorPublicName"] = donorPublicName ?? string.Empty,
                ["isPublic"] = isPublic ? "true" : "false",
                ["message"] = message ?? string.Empty
            };

            var options = new SessionCreateOptions
            {
                Mode = "subscription",
                SuccessUrl = $"{domain}/donate/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/donate/cancel",
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency   = currency.ToLowerInvariant(),
                            UnitAmount = amountMinor,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Miesięczna darowizna dla Schroniska"
                            },
                            Recurring = new SessionLineItemPriceDataRecurringOptions
                            {
                                Interval = "month"
                            }
                        },
                        Quantity = 1
                    }
                },
                Metadata = commonMeta, // (opcjonalnie)
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = commonMeta // <- ląduje na subskrypcji
                }
            };

            var session = await new SessionService().CreateAsync(options, requestOptions: null, cancellationToken: ct);
            return session.Url!;
        }

        // ---------------------------
        // Webhook Stripe
        // ---------------------------
        public async Task HandleWebhookAsync(string json, string stripeSignatureHeader, CancellationToken ct)
        {
            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignatureHeader, _stripeSetting.WebhookSecret);

            switch (stripeEvent.Type)
            {
                // Jednorazowe – zapisujemy tylko tutaj
                case "checkout.session.completed":
                    {
                        var session = stripeEvent.Data.Object as Session;
                        if (session is null) return;

                        var isRecurring = session.Mode == "subscription";
                        var paymentIntentId = session.PaymentIntentId; // tylko one-time zapisujemy
                        var customerId = session.CustomerId;
                        var amount = session.AmountTotal ?? 0;
                        var currency = (session.Currency ?? "pln").ToUpperInvariant();

                        session.Metadata.TryGetValue("donorPublicName", out var name);
                        session.Metadata.TryGetValue("isPublic", out var isPublicStr);
                        session.Metadata.TryGetValue("message", out var msg);

                        // Utrwal preferencje na kliencie + ewentualnie na subskrypcji
                        try
                        {
                            if (!string.IsNullOrEmpty(customerId))
                            {
                                var custSvc = new CustomerService();
                                await custSvc.UpdateAsync(customerId, new CustomerUpdateOptions
                                {
                                    Metadata = new Dictionary<string, string>
                                    {
                                        ["donorPublicName"] = name ?? string.Empty,
                                        ["isPublic"] = string.IsNullOrEmpty(isPublicStr) ? "false" : isPublicStr,
                                        ["message"] = msg ?? string.Empty
                                    }
                                }, cancellationToken: CancellationToken.None);
                            }

                            var sessionSubId =
                                session.GetType().GetProperty("SubscriptionId")?.GetValue(session) as string
                                ?? session.GetType().GetProperty("Subscription")?.GetValue(session) as string;

                            if (!string.IsNullOrEmpty(sessionSubId))
                            {
                                var subSvc = new SubscriptionService();
                                await subSvc.UpdateAsync(sessionSubId, new SubscriptionUpdateOptions
                                {
                                    Metadata = new Dictionary<string, string>
                                    {
                                        ["donorPublicName"] = name ?? string.Empty,
                                        ["isPublic"] = string.IsNullOrEmpty(isPublicStr) ? "false" : isPublicStr,
                                        ["message"] = msg ?? string.Empty
                                    }
                                }, cancellationToken: CancellationToken.None);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[Donations] Persisting metadata failed: " + ex.Message);
                        }

                        // ⬇️ Zapis do DB tylko dla one-time
                        if (!isRecurring && !string.IsNullOrEmpty(paymentIntentId))
                        {
                            // szybki „soft check” (może nic nie dać przy wyścigu – właściwa ochrona jest w DB)
                            var already = await _donationRepository.Query()
                                .AnyAsync(d => d.StripePaymentIntentId == paymentIntentId, ct);
                            if (!already)
                            {
                                await _donationRepository.AddAsync(new Donation
                                {
                                    AmountMinor = amount,
                                    Currency = currency,
                                    DonorPublicName = string.IsNullOrWhiteSpace(name) ? null : name,
                                    IsRecurring = false,
                                    StripePaymentIntentId = paymentIntentId,
                                    StripeCustomerId = customerId,
                                    IsPublic = string.Equals(isPublicStr, "true", StringComparison.OrdinalIgnoreCase),
                                    Message = string.IsNullOrWhiteSpace(msg) ? null : msg
                                }, ct);

                                await TrySaveChangesIgnoringUniqueAsync(ct); // <-- TUTAJ
                            }
                        }
                        break;
                    }

                // Subskrypcje – zapisujemy tylko na opłaconej fakturze
                case "invoice.paid":
                    {
                        var invoice = stripeEvent.Data.Object as Invoice;
                        if (invoice is null) break;

                        var efCt = CancellationToken.None; // unikamy RequestAborted

                        long amountMinor = invoice.AmountPaid;
                        var currency = (invoice.Currency ?? "pln").ToUpperInvariant();
                        var customerId = invoice.CustomerId;
                        var invoiceId = invoice.Id;

                        // szybki „soft check”
                        var exists = await _donationRepository.Query()
                            .AnyAsync(d => d.StripePaymentIntentId == invoiceId, efCt);
                        if (exists) break;

                        string? donorPublicName = null;
                        bool isPublic = false;
                        string? message = null;

                        // 1) Subscription.Metadata (spróbuj z invoice, a jak nie ma – najnowsza sub klienta)
                        Stripe.Subscription? sub = null;
                        try
                        {
                            var subSvc = new SubscriptionService();

                            var subscriptionId = TryGetSubscriptionId(invoice);
                            if (!string.IsNullOrEmpty(subscriptionId))
                            {
                                sub = await subSvc.GetAsync(subscriptionId, cancellationToken: efCt);
                            }
                            else if (!string.IsNullOrEmpty(customerId))
                            {
                                var list = await subSvc.ListAsync(new SubscriptionListOptions
                                {
                                    Customer = customerId,
                                    Status = "all",
                                    Limit = 1
                                }, cancellationToken: efCt);
                                sub = list?.Data?.FirstOrDefault();
                            }

                            if (sub?.Metadata != null && sub.Metadata.Count > 0)
                            {
                                if (sub.Metadata.TryGetValue("donorPublicName", out var n) && !string.IsNullOrWhiteSpace(n))
                                    donorPublicName = n;

                                if (sub.Metadata.TryGetValue("isPublic", out var p))
                                    isPublic = string.Equals(p, "true", StringComparison.OrdinalIgnoreCase);

                                if (sub.Metadata.TryGetValue("message", out var m) && !string.IsNullOrWhiteSpace(m))
                                    message = m;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[Donations] Reading subscription metadata failed: " + ex.Message);
                        }

                        // 2) Customer.Metadata
                        if ((donorPublicName is null || string.IsNullOrEmpty(message)) && !string.IsNullOrEmpty(customerId))
                        {
                            try
                            {
                                var custSvc = new CustomerService();
                                var cust = await custSvc.GetAsync(customerId, cancellationToken: efCt);
                                var meta = cust?.Metadata;

                                if (meta != null && meta.Count > 0)
                                {
                                    if (donorPublicName is null && meta.TryGetValue("donorPublicName", out var n) && !string.IsNullOrWhiteSpace(n))
                                        donorPublicName = n;

                                    if (meta.TryGetValue("isPublic", out var p))
                                        isPublic = string.Equals(p, "true", StringComparison.OrdinalIgnoreCase);

                                    if (string.IsNullOrEmpty(message) && meta.TryGetValue("message", out var m) && !string.IsNullOrWhiteSpace(m))
                                        message = m;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("[Donations] Reading customer metadata failed: " + ex.Message);
                            }
                        }

                        // 3) Fallback: ostatnia cykliczna wpłata
                        if (donorPublicName is null && !string.IsNullOrEmpty(customerId))
                        {
                            var last = await _donationRepository.Query()
                                .Where(d => d.StripeCustomerId == customerId && d.IsRecurring)
                                .OrderByDescending(d => d.CreatedAt)
                                .FirstOrDefaultAsync(efCt);

                            if (last is not null)
                            {
                                donorPublicName = last.DonorPublicName;
                                isPublic = last.IsPublic;
                                message = last.Message;
                            }
                        }

                        // 4) Miękki fallback: imię z faktury
                        if (string.IsNullOrWhiteSpace(donorPublicName))
                            donorPublicName = invoice.CustomerName ?? "Anonim";

                        await _donationRepository.AddAsync(new Donation
                        {
                            AmountMinor = amountMinor,
                            Currency = currency,
                            DonorPublicName = string.IsNullOrWhiteSpace(donorPublicName) ? null : donorPublicName,
                            IsRecurring = true,
                            StripePaymentIntentId = invoiceId, // idempotencja
                            StripeCustomerId = customerId,
                            IsPublic = isPublic,
                            Message = string.IsNullOrWhiteSpace(message) ? null : message
                        }, efCt);

                        await TrySaveChangesIgnoringUniqueAsync(efCt); // <-- TUTAJ
                        break;
                    }

                case "invoice.payment_failed":
                    {
                        // log/monitoring
                        break;
                    }

                default:
                    break;
            }
        }

        // ---------------------------
        // Read models
        // ---------------------------
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
                    d.IsRecurring,
                    d.Message,
                    d.CreatedAt
                ));

            return await query.ToListAsync(ct);
        }

        public async Task<List<MonthlySumDto>> GetMonthlySummaryAsync(int year, CancellationToken ct)
        {
            if (year <= 0)
                year = DateTime.UtcNow.Year;

            var start = new DateTime(year, 1, 1);
            var end = start.AddYears(1);

            // Pobieramy dane z bazy (filtrowanie po stronie SQL)
            var donations = await _donationRepository.Query()
                .AsNoTracking()
                .Where(d => d.CreatedAt >= start && d.CreatedAt < end)
                .Select(d => new { d.CreatedAt, d.Currency, d.AmountMinor })
                .ToListAsync(ct); // tutaj kończy się SQL

            // Grupowanie po stronie pamięci
            var result = donations
                .GroupBy(d => new { d.CreatedAt.Year, d.CreatedAt.Month, d.Currency })
                .Select(g => new MonthlySumDto(
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Currency,
                    g.Sum(x => x.AmountMinor),
                    g.Count()
                ))
                .OrderBy(r => r.Month)
                .ToList();

            return result;
        }


        // ---------------------------
        // Helpery
        // ---------------------------
        private static string? TryGetSubscriptionId(Invoice invoice)
        {
            var subIdProp = invoice.GetType().GetProperty("SubscriptionId");
            if (subIdProp?.GetValue(invoice) is string s1 && !string.IsNullOrEmpty(s1))
                return s1;

            var subProp = invoice.GetType().GetProperty("Subscription");
            if (subProp != null)
            {
                var val = subProp.GetValue(invoice);
                if (val is string s2 && !string.IsNullOrEmpty(s2)) return s2;
                if (val is Subscription subObj && !string.IsNullOrEmpty(subObj.Id)) return subObj.Id;

                var idProp = val?.GetType().GetProperty("Id");
                if (idProp?.GetValue(val) is string s3 && !string.IsNullOrEmpty(s3)) return s3;
            }

            return null;
        }

        private async Task TrySaveChangesIgnoringUniqueAsync(CancellationToken ct)
        {
            try
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // Duplikat (retry Stripe / wyścig zapisu) — ignorujemy
            }
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            // SQL Server: 2601 (duplicate index), 2627 (unique constraint)
            if (ex.InnerException is SqlException sql)
                return sql.Number == 2601 || sql.Number == 2627;

            // TODO: dopasuj jeśli używasz innej bazy (np. Postgres 23505, MySQL 1062)
            return false;
        }
    }
}
