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
    return this.http.get<AnimalDto[]>(`${this.base}/all`)
  }

  // GET: /api/animals?sortBy=name&sortDir=asc&page=0&size=20&...
  getAnimalsPaged(q: AnimalQuery = {}): Observable<PagedResult<AnimalDto>> {
    let params = new HttpParams();

    // helper: ustaw tylko, gdy wartość nie jest undefined/null/'' (puste stringi omijamy)
    const set = (k: string, v: any) => {
      if (v === undefined || v === null || v === '') return;
      params = params.set(k, String(v));
    };

    // sort + paging
    set('sortBy', q.sortBy);
    set('sortDir', q.sortDir);
    set('page', q.page);
    set('size', q.size);

    // filters
    // liczby: nie używamy truthy (0 jest valid), sprawdzamy null/undefined
    set('ageMinMonths', q.ageMinMonths);
    set('ageMaxMonths', q.ageMaxMonths);

    // string-uniony: pusty string traktuj jako „brak filtra”
    set('speciesId', q.speciesId)
    set('sex', q.sex);
    set('status', q.status);

    // daty
    set('createdFrom', q.createdFrom);
    set('createdTo', q.createdTo);

    // booleany: MUSI iść zarówno true, jak i false → nie używać if (q.vaccinated)
    if (q.vaccinated !== undefined) params = params.set('vaccinated', String(q.vaccinated));
    if (q.neutered   !== undefined) params = params.set('neutered',   String(q.neutered));

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
