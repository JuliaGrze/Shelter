import { Component, inject, OnInit } from '@angular/core';
import { AnimalDto } from '../../../core/models/animal';
import { AnimalService } from '../../../core/services/animal.service';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-animal-list',
  imports: [RouterLink],
  templateUrl: './animal-list.html',
  styleUrl: './animal-list.css'
})
export class AnimalList implements OnInit {
  private animalService = inject(AnimalService)

  animals: AnimalDto[] = []
  loading = true
  error = ''

  ngOnInit(): void {
    this.getAnimals()
  }

  //get all animals: backend => service => now
  getAnimals(){
    this.animalService.getAllAnimals().subscribe({
      next: data => {
        this.animals = data
        this.loading = false
      },
      error: err => {
        this.error = 'Błąd podczas pobierania zwierząt';
        this.loading = false;
      }
    })
  }

}
