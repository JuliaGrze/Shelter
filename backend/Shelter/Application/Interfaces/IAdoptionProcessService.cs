using Application.Common;
using Application.Dtos.Adoptions;
using Application.Dtos.Adoptions.HomeVisit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAdoptionProcessService
    {
        //Złożenie wniosku przez użytkownika
        Task<SubmitApplicationResponse> SubmitAsync(SubmitApplicationRequest dto, string applicantUserId, CancellationToken ct = default);

        //Zmiany statusów (pracownik)
        Task SetInReviewAsync(int appId, UpdateAdoptionStatusRequest dto, string reviewerUserId, CancellationToken ct = default);
        Task ApproveAsync(int appId, UpdateAdoptionStatusRequest dto, string reviewerUserId, CancellationToken ct = default);
        Task RejectAsync(int appId, UpdateAdoptionStatusRequest dto, string reviewerUserId, CancellationToken ct = default);

        //Wizyta domowa
        Task ScheduleHomeVisitAsync(int appId, ScheduleHomeVisitRequest dto, CancellationToken ct = default);
        Task SetHomeVisitResultAsync(int appId, SetHomeVisitResultRequest dto, CancellationToken ct = default);

        //Umowa adopcyjna
        Task<GenerateContractResponse> GenerateContractAsync(int appId, CancellationToken ct = default);
        Task SignContractAsync(int appId, string signerUserId, CancellationToken ct = default);

        //Lista wnioskow adopcyjnych
        Task<PagedResult<AdoptionListItemDto>> ListAsync(
            string? status = null,
            string? q = null,
            int page = 1,
            int size = 20,
            CancellationToken ct = default);
    }
}
