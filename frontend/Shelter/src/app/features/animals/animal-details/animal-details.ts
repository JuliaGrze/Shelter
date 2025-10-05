import { Component, inject, OnInit } from '@angular/core';
import { AnimalService } from '../../../core/services/animal.service';
import { AnimalDto } from '../../../core/models/animal';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe, NgClass, NgIf } from '@angular/common';
import { SexPlPipe } from '../../../shared/pipes/sex-pl-pipe';
import { StatusPlPipe } from '../../../shared/pipes/status-pl-pipe';

@Component({
  selector: 'app-animal-details',
  imports: [
    NgIf,
    DatePipe,
    RouterLink,
    SexPlPipe,
    StatusPlPipe,
    NgClass
],
  templateUrl: './animal-details.html',
  styleUrl: './animal-details.css'
})
export class AnimalDetails implements OnInit {
  private animalService = inject(AnimalService)
  private route = inject(ActivatedRoute)

  loading = true
  error = ''
  animal?: AnimalDto

  ngOnInit(): void {
    this.getAnimalDetails()
  }

  getAnimalDetails(){
    const id = Number(this.route.snapshot.paramMap.get('id'))
    this.animalService.getAnimalById(id).subscribe({
      next: data => {
        this.animal = data
        this.loading = false
      },
      error: () => {
        this.error = 'Nie znaleziono zwierzęcia'
        this.loading = false
      }
    })
  }

}
