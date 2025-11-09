import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { enviroment } from '../../../environments/environment';
import { AdoptionApplicationDto, AdoptionDetailsDto, AdoptionListItemDto, GenerateContractResponse, ScheduleHomeVisitRequest, SetHomeVisitResultRequest, SubmitApplicationRequest, SubmitApplicationResponse, UpdateAdoptionStatusRequest } from '../models/adoption';
import { Observable } from 'rxjs';
import { PagedResult } from '../models/paged-result';

@Injectable({
  providedIn: 'root'
})
export class AdoptionService {
  private http = inject(HttpClient)
  private base = `${enviroment.apiUrl}/adoptions`

  submit(animalId: number, body: SubmitApplicationRequest): Observable<SubmitApplicationResponse> {
    return this.http.post<SubmitApplicationResponse>(`${this.base}/${animalId}/apply`, body);
  }

  getApplication(id: number): Observable<AdoptionApplicationDto> {
    return this.http.get<AdoptionApplicationDto>(`${this.base}/applications/${id}`);
  }

  listApplications(params: { status?: string; q?: string; page?: number; pageSize?: number }) {
    return this.http.get<PagedResult<AdoptionListItemDto>>(
      `${this.base}/applications`,
      {
        params: {
          status: params.status ?? '',
          q: params.q ?? '',
          page: String(params.page ?? 1),
          size: String(params.pageSize ?? 10)
        }
      }
    );
  }

  // Details
  get(id: number) {
    return this.http.get<AdoptionDetailsDto>(`${this.base}/applications/${id}`);
  }

  // Client: my applications
  listMine(page = 1, size = 20) {
    return this.http.get<PagedResult<AdoptionListItemDto>>(`${this.base}/my-applications`, {
      params: { page, size } as any
    });
  }

  // Worker/Admin actions
  setInReview(id: number, body: UpdateAdoptionStatusRequest) {
    return this.http.post<void>(`${this.base}/applications/${id}/in-review`, body);
  }
  approve(id: number, body: UpdateAdoptionStatusRequest) {
    return this.http.post<void>(`${this.base}/applications/${id}/approve`, body);
  }
  reject(id: number, body: UpdateAdoptionStatusRequest) {
    return this.http.post<void>(`${this.base}/applications/${id}/reject`, body);
  }

  // Home visit
  scheduleVisit(id: number, body: ScheduleHomeVisitRequest) {
    return this.http.post<void>(`${this.base}/applications/${id}/home-visit/schedule`, body);
  }
  setVisitResult(id: number, body: SetHomeVisitResultRequest) {
    return this.http.post<void>(`${this.base}/applications/${id}/home-visit/result`, body);
  }

  //Contract
  generateContract(id: number) {
    return this.http.post<GenerateContractResponse>(`${this.base}/applications/${id}/contract/generate`, {});
  }
  signContract(id: number) {
    return this.http.post<void>(`${this.base}/applications/${id}/contract/sign`, {});
  }
}
