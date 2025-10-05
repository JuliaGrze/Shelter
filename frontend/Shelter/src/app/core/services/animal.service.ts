import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { enviroment} from '../../../environments/environment'
import { Observable } from 'rxjs';
import { AnimalDto, CreateAnimalDto } from '../models/animal';

@Injectable({
  providedIn: 'root'
})
export class AnimalService {
  private http = inject(HttpClient)
  private base = `${enviroment.apiUrl}/animals`

  // GET /api/animals
  getAllAnimals() : Observable<AnimalDto[]>{
    return this.http.get<AnimalDto[]>(this.base)
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
