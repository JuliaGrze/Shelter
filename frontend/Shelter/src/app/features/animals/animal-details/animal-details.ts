import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe, NgClass, NgIf, NgFor } from '@angular/common';

import { AnimalService } from '../../../core/services/animal.service';
import { AnimalDto } from '../../../core/models/animal';
import { SexPlPipe } from '../../../shared/pipes/sex-pl-pipe';
import { StatusPlPipe } from '../../../shared/pipes/status-pl-pipe';

import { MedicalRecordService } from '../../../core/services/medical-record.service';
import { MedicalRecord } from '../../../core/models/medical-record';

@Component({
  selector: 'app-animal-details',
  imports: [NgIf, NgFor, DatePipe, RouterLink, SexPlPipe, StatusPlPipe, NgClass],
  templateUrl: './animal-details.html',
  styleUrl: './animal-details.css',
  standalone: true
})
export class AnimalDetails implements OnInit {
  private animalService = inject(AnimalService);
  private medicalService = inject(MedicalRecordService);
  private route = inject(ActivatedRoute);

  loading = true;
  error = '';
  animal?: AnimalDto;

  // Medical records
  recLoading = false;
  recError = '';
  records: MedicalRecord[] = [];

  // computed flags
  vaccinated = false;
  neutered = false;

  // UI toggle for history
  showMedicalHistory = false;

  ngOnInit(): void {
    this.getAnimalDetails();
  }

  getAnimalDetails() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.animalService.getAnimalById(id).subscribe({
      next: data => {
        this.animal = data;
        this.loading = false;
        this.loadMedicalRecords();
      },
      error: () => {
        this.error = 'Nie znaleziono zwierzęcia';
        this.loading = false;
      }
    });
  }

  loadMedicalRecords() {
    if (!this.animal) return;
    this.recLoading = true;
    this.recError = '';
    this.medicalService.getMedicalRecordByAnimal(this.animal.id).subscribe({
      next: list => {
        // stabilne sortowanie: DESC po dacie + tie-break po id
        this.records = [...list].sort((a, b) => {
          const da = new Date(a.date).getTime();
          const db = new Date(b.date).getTime();
          if (db !== da) return db - da;
          return b.id - a.id;
        });

        // computed status
        this.vaccinated = this.records.some(r => r.type === 'Vaccination');
        this.neutered   = this.records.some(r => r.type === 'Sterilization');

        this.recLoading = false;
      },
      error: () => {
        this.recError = 'Nie udało się pobrać karty medycznej';
        this.recLoading = false;
      }
    });
  }

  toggleMedicalHistory() {
    this.showMedicalHistory = !this.showMedicalHistory;
  }
}
