import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { enviroment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { SpeciesDto } from '../models/species';

@Injectable({
  providedIn: 'root'
})
export class SpeciesService {
  private http = inject(HttpClient)
  private base = `${enviroment.apiUrl}/species`

  // GET /api/species
  getAllSpecies() : Observable<SpeciesDto[]>{
    return this.http.get<SpeciesDto[]>(this.base)
  }
}
