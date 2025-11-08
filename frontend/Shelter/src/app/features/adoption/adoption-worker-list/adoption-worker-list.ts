import { Component, inject, OnInit } from '@angular/core';
import { AdoptionService } from '../../../core/services/adoption.service';
import { PagedResult } from '../../../core/models/paged-result';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { AdoptionListItemDto, AdoptionStatusCode } from '../../../core/models/adoption';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-adoption-worker-list',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    DatePipe
  ],
  templateUrl: './adoption-worker-list.html',
  styleUrl: './adoption-worker-list.css'
})
export class AdoptionWorkerList implements OnInit {
  private adoptionService = inject(AdoptionService)
  private fb = inject(FormBuilder)

  //Filtry
  form = this.fb.group({
    q: [''],
    status: [''],
    size: [10]
  })

  //Dane tabeli i paginacja
  adoptionList: AdoptionListItemDto[] = []
  page = 1
  size = 10
  total = 0

  loading = false
  error: string | null = null

  readonly STATUSES: (AdoptionStatusCode | string)[] = [
    'Submitted','InReview','Approved','Rejected',
    'HomeVisitScheduled','HomeVisitPassed','HomeVisitFailed',
    'ContractGenerated','ContractSigned'
  ];

  ngOnInit(): void {
    this.size = this.form.value.size ?? 10
    this.load()
  }

  load(){
    this.loading = true
    this.error = null

    const {q, status, size} = this.form.value
    const pageSize = Number(size ?? this.size);
    this.size = pageSize;

    this.adoptionService.listApplications({
      q: q ?? '',
      status: status ?? '',
      page: this.page,
      pageSize: pageSize
    })
     .subscribe({
      next: (res: PagedResult<AdoptionListItemDto>) => {
        this.adoptionList = res.items as AdoptionListItemDto[];
        this.total = res.total;
        this.size = res.size;
        this.loading = false;
      },
      error: () => {
        this.error = 'Nie udało się pobrać listy wniosków.';
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    this.page = 1;
    this.load();
  }

  clearFilters(): void {
    this.form.reset({ q: '', status: '', size: 10 });
    this.page = 1;
    this.load();
  }

  go(p: number): void {
    const pages = this.pages();
    if (p < 1 || p > pages) return;
    this.page = p;
    this.load();
  }

  pages(): number {
    return Math.max(1, Math.ceil(this.total / this.size));
  }
  
}
