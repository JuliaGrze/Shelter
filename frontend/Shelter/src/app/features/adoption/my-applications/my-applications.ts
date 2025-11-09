import { Component, inject, OnInit, signal } from '@angular/core';
import { AdoptionService } from '../../../core/services/adoption.service';
import { AdoptionListItemDto } from '../../../core/models/adoption';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-my-applications',
  imports: [
    DatePipe,
    RouterLink
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
}
