import { Component, computed, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgClass, NgIf, NgFor, DecimalPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { AnimalDto, AnimalStatus, Sex } from '../../../core/models/animal';
import { AnimalService } from '../../../core/services/animal.service';
import { SexPlPipe } from '../../../shared/pipes/sex-pl-pipe';
import { StatusPlPipe } from '../../../shared/pipes/status-pl-pipe';
import { PagedResult } from '../../../core/models/paged-result';
import { SortBy, SortDir } from '../../../core/models/animal-query';
import { SpeciesDto } from '../../../core/models/species';
import { SpeciesService } from '../../../core/services/species.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-animal-list',
  standalone: true,
  imports: [
    RouterLink, 
    SexPlPipe, 
    StatusPlPipe, 
    NgClass, 
    NgIf, NgFor, 
    FormsModule
  ],
  templateUrl: './animal-list.html',
  styleUrl: './animal-list.css'
})
export class AnimalList implements OnInit {
  private animalService = inject(AnimalService);
  private speciesService = inject(SpeciesService)
  private authService = inject(AuthService)
  protected readonly Math = Math;

  species: SpeciesDto[] = []
  animals: AnimalDto[] = [];
  total = 0;
  page = 0;
  size = 20;

  sortBy: SortBy = 'createdAt';
  sortDir: SortDir = 'desc';

  loading = true;
  error = '';

  canManage = computed(() => {
    const p = this.authService.profile();        // signal → odświeży się sam
    return !!p?.roles?.some(r => r === 'Admin' || r === 'Worker');
  });

  sortOptions: { value: SortBy; label: string }[] = [
    { value: 'createdAt', label: 'Najnowsze (Created At)' },
    { value: 'name',      label: 'Nazwa (A→Z)' },
    { value: 'species',   label: 'Gatunek (A→Z)' },
    { value: 'birthDate', label: 'Data urodzenia' },
    { value: 'sex',       label: 'Płeć' },
    { value: 'status',    label: 'Status' },
  ];

  // filters (opcjonalne)
  speciesId: number | null = null
  sex: Sex | '' = '';
  status: AnimalStatus | '' = '';
  ageMinYears?: number;
  ageMaxYears?: number;
  createdFrom?: string;   // yyyy-MM-dd
  createdTo?: string;     // yyyy-MM-dd

  ngOnInit(): void {
    this.getAllSpecies()
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';

    this.animalService.getAnimalsPaged({
      sortBy: this.sortBy,
      sortDir: this.sortDir,
      page: this.page,
      size: this.size,

      speciesId: this.speciesId ?? undefined,
      sex: this.sex || undefined,
      status: this.status || undefined,

      ageMinMonths: this.toMonths(this.ageMinYears),
      ageMaxMonths: this.toMonths(this.ageMaxYears),

      createdFrom: this.createdFrom || undefined,
      createdTo:   this.createdTo   || undefined,
    }).subscribe({
      next: (res: PagedResult<AnimalDto>) => {
        this.animals = res.items;
        this.total = res.total;
        this.page = res.page;
        this.size = res.size;
        this.loading = false;
      },
      error: _ => {
        this.error += 'Błąd podczas pobierania zwierząt';
        this.loading = false;
      }
    });
  }

  getAllSpecies(){
    this.speciesService.getAllSpecies().subscribe({
      next: (list) => this.species = list,
      error: () => {
        this.error = 'Błąd podczas pobierania gatnuknków zwierząt\n';
        this.species = []
      }
    })
  }

  onSortByChange(value: string) {
    this.sortBy = value as SortBy;
    this.page = 0;
    this.load();
  }

  toggleDir() {
    this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    this.page = 0;
    this.load();
  }

  setSize(newSize: number) {
    this.size = newSize;
    this.page = 0;
    this.load();
  }

  /** Całkowita liczba stron (>= 1) */
  get totalPages(): number {
    return Math.max(1, Math.ceil(this.total / (this.size || 1)));
  }

  /** Lista numerów do wyświetlenia (z '…'), indeksy są zero-based */
  get pageNumbers(): (number | '...')[] {
    const total = this.totalPages;
    const current = this.page;
    const delta = 2;

    if (total <= 1) return [0];

    const out: (number | '...')[] = [0];
    const left  = Math.max(1, current - delta);
    const right = Math.min(total - 2, current + delta);

    if (left > 1) out.push('...');
    for (let i = left; i <= right; i++) out.push(i);
    if (right < total - 2) out.push('...');
    out.push(total - 1);

    return out;
  }

  /** Ustaw konkretną stronę (zero-based) */
  setPage(p: number) {
    if (p < 0 || p >= this.totalPages || p === this.page) return;
    this.page = p;
    this.load();
  }
  /** Poprzednia strona */
  prevPage() {
    this.setPage(this.page - 1);
  }

  /** Następna strona */
  nextPage() {
    this.setPage(this.page + 1);
  }

  private toMonths(y?: number) { return y !== undefined && y !== null ? y * 12 : undefined; }

  // ----- filtry: apply/clear -----
  applyFilters() {
    // (opcjonalnie) walidacja zakresu dat
    if (this.createdFrom && this.createdTo && this.createdFrom > this.createdTo) {
      // zamień / wyczyść, jak wolisz; na razie czyścimy "do"
      this.createdTo = undefined;
    }
    this.page = 0;
    this.load();
  }

  clearFilters() {
    this.sex = '';
    this.status = '';
    this.ageMinYears = undefined;
    this.ageMaxYears = undefined;
    this.createdFrom = undefined;
    this.createdTo = undefined;
    this.page = 0;
    this.load();
  }

  // --- UI stanu filtrów ---
  showFilters = false;

  toggleFilters() {
    this.showFilters = !this.showFilters;
  }

  // Kropka „•” na przycisku gdy coś ustawione
  get hasActiveFilters(): boolean {
    return !!(
      this.sex ||
      this.status ||
      this.ageMinYears !== undefined ||
      this.ageMaxYears !== undefined ||
      this.createdFrom ||
      this.createdTo
    );
  }

  selectSpecies(id: number | null){
    if(id === this.speciesId) return

    this.speciesId = id
    this.page = 0
    this.load()
  }

}
