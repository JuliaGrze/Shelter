import { Component, inject, OnInit, signal } from '@angular/core';
import { AdoptionService } from '../../../core/services/adoption.service';
import { AdoptionListItemDto } from '../../../core/models/adoption';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-my-applications',
  imports: [
    DatePipe,
    RouterLink,
    CommonModule
  ],
  templateUrl: './my-applications.html',
  styleUrl: './my-applications.css'
})
export class MyApplications implements OnInit {
  private adoptionService = inject(AdoptionService)

  items: AdoptionListItemDto[] = []
  
  loading = true
  error: string | null = null

  page = 1; size = 20; total = 0;

  ngOnInit(): void {
    this.load()
  }

  load(){
    this.loading = true
    this.adoptionService.listMine(this.page, this.size).subscribe({
      next: res => {
        this.items = res.items
        this.total = res.total
        this.size = res.size
        this.page = res.page
        this.loading = false
      },
      error: _ => {
        this.error = 'Nie udało się pobrać listy.'
        this.loading = false
      }
    })
  }

  statusClass(code: string | null | undefined): string {
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
