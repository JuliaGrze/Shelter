import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { enviroment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { CreateMedicalRecord, MedicalRecord } from '../models/medical-record';

@Injectable({
  providedIn: 'root'
})
export class MedicalRecordService {
  private http = inject(HttpClient)
  private base = `${enviroment.apiUrl}/medicalrecords`

  getMedicalRecordByAnimal(animalId: number) : Observable<MedicalRecord[]>{
    return this.http.get<MedicalRecord[]>(`${this.base}/animal/${animalId}`)
  }

  createMedicalRecord(dto: CreateMedicalRecord) : Observable<number>{
    return this.http.post<number>(this.base, dto)
  }

  updateMedicalRecord(id: number, dto: CreateMedicalRecord): Observable<void>{
    return this.http.put<void>(`${this.base}/${id}`, dto)
  }

  deleteMedicalRecord(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
