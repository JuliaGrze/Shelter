import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AdoptionService } from '../../../core/services/adoption.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-apply-for-adoption',
  imports: [
    RouterLink,
    CommonModule, FormsModule
  ],
  templateUrl: './apply-for-adoption.html',
  styleUrl: './apply-for-adoption.css'
})
export class ApplyForAdoption {
  private route = inject(ActivatedRoute)
  private adoptionService = inject(AdoptionService)
  private router = inject(Router)

  animalId = Number(this.route.snapshot.paramMap.get("id"))
  notes = ''
  loading = false
  error: string | null = null
  successId: number | null = null

  submit(){
    this.adoptionService.submit(this.animalId, 
      {animalId: this.animalId, notes: this.notes || undefined})
      .subscribe({
        next: res => {
        this.successId = res.applicationId
        this.loading = false
        this.router.navigate(['/adoption/my/application']);
      },
      error: _ => { 
        this.error = 'Nie udało się złożyć wniosku.'
        this.loading = false
      }
      })
  }
}

