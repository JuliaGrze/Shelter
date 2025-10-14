import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { enviroment } from '../../../environments/environment';
import { OneTimeDto, RecurringDto, DonorWallItemDto, MonthlySumDto } from '../models/donation';

@Injectable({
  providedIn: 'root'
})
export class DonationService {
  private http = inject(HttpClient)
  private base = `${enviroment.apiUrl}/donations`

  /** POST /api/donations/checkout/one-time -> { sessionUrl } */
  createOneTime(dto: OneTimeDto) {
    return this.http.post<{ sessionUrl: string }>(`${this.base}/checkout/one-time`, dto);
  }

  /** POST /api/donations/checkout/recurring -> { sessionUrl } */
  createRecurring(dto: RecurringDto) {
    return this.http.post<{ sessionUrl: string }>(`${this.base}/checkout/recurring`, dto);
  }

  /** GET /api/donations/public-latest?take=30 */
  getPublicLatest(take = 30) {
    const params = new HttpParams().set('take', take);
    return this.http.get<DonorWallItemDto[]>(`${this.base}/public-latest`, { params });
  }

  /** GET /api/donations/by-month?year=YYYY  (wymaga Admin/Worker) */
  getMonthlySummary(year: number) {
    const params = new HttpParams().set('year', year);
    return this.http.get<MonthlySumDto[]>(`${this.base}/by-month`, { params });
  }
}
