import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AdoptionDetailsDto } from '../../../core/models/adoption';
import { AdoptionService } from '../../../core/services/adoption.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-application-adoption-details-worker',
  imports: [CommonModule, FormsModule],
  templateUrl: './application-adoption-details-worker.html',
  styleUrl: './application-adoption-details-worker.css'
})
export class ApplicationAdoptionDetailsWorker implements OnInit{
  private route = inject(ActivatedRoute)
  private addoptionService = inject(AdoptionService)
  private router = inject(Router)

  id = Number(this.route.snapshot.paramMap.get('id'))
  details = signal<AdoptionDetailsDto | null> (null)
  loading = signal(true)
  error = signal<string |null>(null)

  note = ''
  visitDate = ''
  visitNotes = ''
  visitResultId: number | null = null

  ngOnInit(): void {
    this.refresh()
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
      { scheduledAt: this.visitDate, notes: this.visitNotes || undefined })
      .subscribe(() => this.refresh());
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
}
