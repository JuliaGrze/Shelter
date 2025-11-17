import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AdoptionService } from '../../../core/services/adoption.service';
import { AdoptionApplicationDto } from '../../../core/models/adoption';
import { CommonModule, DatePipe } from '@angular/common';

@Component({
  selector: 'app-details-application-adoption',
  imports: [
    CommonModule,
    RouterLink,
    DatePipe
  ],
  templateUrl: './details-application-adoption.html',
  styleUrl: './details-application-adoption.css'
})
export class DetailsApplicationAdoption implements OnInit{
  private route = inject(ActivatedRoute)
  private router = inject(Router)
  private adoptionService = inject(AdoptionService)

  loading = signal(true)
  error = signal<string | null>(null)
  app = signal<AdoptionApplicationDto | null>(null)

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id')
    const id = idParam ? Number(idParam) : NaN

    this.load(id)
  }

  private load(id: number){
    this.loading.set(true)
    this.error.set(null)

    this.adoptionService.getApplication(id).subscribe({
      next: a => {
        this.app.set(a)
        this.loading.set(false)
      },
      error: err => {
        console.error(err)
        this.error.set('Nie udało się załadować wniosku adopcyjnego.')
        this.loading.set(false)
      }
    })
  }

  goBack() {
    this.router.navigate(['/adoption/my/applications']);
  }

  statusClass(code: string): string {
    switch (code) {
      case 'Submitted':           return 'badge--submitted';
      case 'InReview':            return 'badge--inreview';
      case 'HomeVisitScheduled':  return 'badge--visit-scheduled';
      case 'HomeVisitCompleted':  return 'badge--visit-completed';
      case 'Approved':            return 'badge--approved';
      case 'Rejected':            return 'badge--rejected';
      case 'Withdrawn':           return 'badge--withdrawn';
      case 'ContractSigned':      return 'badge--contract-signed';
      case 'ContractGenerated':   return 'badge--contract-generated';
      default:                    return 'badge--neutral';
    }
  }

}
