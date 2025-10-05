import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { enviroment} from '../../../environments/environment'
import { Observable } from 'rxjs';
import { AnimalDto, CreateAnimalDto } from '../models/animal';
import { AnimalQuery } from '../models/animal-query';
import { PagedResult } from '../models/paged-result';

@Injectable({
  providedIn: 'root'
})
export class AnimalService {
  private http = inject(HttpClient)
  private base = `${enviroment.apiUrl}/animals`

  // GET /api/animals/all
  getAllAnimals() : Observable<AnimalDto[]>{
    return this.http.get<AnimalDto[]>(this.base)
  }

  // GET: api/animals?sortBy=name&sortDir=asc&page=0&size=20
  getAnimalsPaged(q: AnimalQuery = {}): Observable<PagedResult<AnimalDto>> {
    let params = new HttpParams();
    if (q.sortBy)  params = params.set('sortBy', q.sortBy);
    if (q.sortDir) params = params.set('sortDir', q.sortDir);
    if (q.page !== undefined) params = params.set('page', q.page);
    if (q.size !== undefined) params = params.set('size', q.size);

    return this.http.get<PagedResult<AnimalDto>>(this.base, { params });
  }

  // GET /api/animals/{id}
  getAnimalById(id: number) : Observable<AnimalDto>{
    return this.http.get<AnimalDto>(`${this.base}/${id}`)
  }

  // POST /api/animals  -> { id }
  addAnimal(createAnimal: CreateAnimalDto) : Observable<number>{
    return this.http.post<number>(this.base, createAnimal)
  }

  // PUT /api/animals/{id} -> 204
  updateAnimal(id: number, animal: CreateAnimalDto) : Observable<void>{
    return this.http.put<void>(`${this.base}/${id}`, animal)
  }

   // DELETE /api/animals/{id} -> 204
  deleteAnimal(id: number) : Observable<void>{
    return this.http.delete<void>(`${this.base}/${id}`)
  }
}
