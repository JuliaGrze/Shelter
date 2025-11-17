import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AdoptionDetailsDto, HomeVisitResultDto } from '../../../core/models/adoption';
import { AdoptionService } from '../../../core/services/adoption.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-application-adoption-details-worker',
  imports: [CommonModule, FormsModule,
    RouterLink
  ],
  templateUrl: './application-adoption-details-worker.html',
  styleUrl: './application-adoption-details-worker.css'
})
export class ApplicationAdoptionDetailsWorker implements OnInit{
  private route = inject(ActivatedRoute)
  private addoptionService = inject(AdoptionService)
  private router = inject(Router)

  id = Number(this.route.snapshot.paramMap.get('id'))
  details = signal<AdoptionDetailsDto | null> (null)
  visitResults = signal<HomeVisitResultDto[]>([]);
  loading = signal(true)
  error = signal<string |null>(null)

  note = ''
  visitDate = ''
  visitNotes = ''
  visitResultId: number | null = null

  ngOnInit(): void {
    this.refresh()
    this.loadVisitResults();
    console.log(this.visitResults)
  }

  refresh(){
    this.loading.set(true)
    this.addoptionService.get(this.id).subscribe({
      next: d => {
        this.details.set(d)
        this.loading.set(false)
      },
      error: e => {
        this.error.set('Nie udało się pobrać szczegółów.')
        this.loading.set(false); 
      }
    })
  }

  inReview(){
    this.addoptionService.setInReview(this.id, 
      { notes: this.note}
    ).subscribe(() => this.refresh())
  }

  approve(){
    this.addoptionService.approve(this.id, 
      { notes: this.note}
    ).subscribe(() => this.refresh())
  }
  reject(){
    this.addoptionService.reject(this.id, 
      { notes: this.note}
    ).subscribe(() => this.refresh())
  }

  scheduleVisit() {
    if (!this.visitDate) return;

    this.addoptionService.scheduleVisit(this.id,
      { date: this.visitDate, notes: this.visitNotes || undefined }
    ).subscribe(() => this.refresh());
  }


  setVisitResult() {
    if (!this.visitResultId) return;
    this.addoptionService.setVisitResult(this.id, 
      { homeVisitResultId: this.visitResultId, notes: this.visitNotes || undefined })
      .subscribe(() => this.refresh());
  }

  generateContract() {
    this.addoptionService.generateContract(this.id).subscribe(res => {
      // po wygenerowaniu po prostu odśwież
      this.refresh();
      // można też dodać toast z linkiem: res.pdfUrl
    });
  }

  loadVisitResults() {
  this.addoptionService.getHomeVisitResults().subscribe({
    next: list => {
      console.log('HomeVisitResults z API:', list);   // <---
      this.visitResults.set(list ?? []);
    },
    error: err => {
      console.error('Błąd przy pobieraniu HomeVisitResults', err);  // <---
      this.visitResults.set([]);
    }
  });
}

  

  statusClass(code: string | null | undefined): string {
    switch (code) {
      case 'Submitted':
        return 'badge-submitted';
      case 'InReview':
        return 'badge-inreview';
      case 'HomeVisitScheduled':
        return 'badge-visit-scheduled';
      case 'HomeVisitCompleted':
        return 'badge-visit-completed';
      case 'Approved':
        return 'badge-approved';
      case 'Rejected':
        return 'badge-rejected';
      case 'Withdrawn':
        return 'badge-withdrawn';
      case 'ContractSigned':
        return 'badge-contract-signed';
      case 'ContractGenerated':
        return 'badge-contract-generated';
      default:
        return 'badge-neutral';
    }
  }

}
