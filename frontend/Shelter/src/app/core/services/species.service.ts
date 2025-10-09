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

  //POST /api/species -> number(nowe id)
  addSpecies(name: string){
    return this.http.post<number>(this.base, {name})
  }

  // PUT /api/species/{id} -> 204
  editSpecies(id: number, name: string){
    return this.http.put<void>(`${this.base}/${id}`, {name})
  }

  // DELETE /api/species/{id} -> 204
  deleteSpecies(id: number) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
