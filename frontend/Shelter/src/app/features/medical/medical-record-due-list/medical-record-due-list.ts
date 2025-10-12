import { Component, inject, OnInit, signal } from '@angular/core';
import { MedicalRecordService } from '../../../core/services/medical-record.service';
import { DueMedicalRecordDto } from '../../../core/models/medical-record';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CommonModule, DatePipe } from '@angular/common';

@Component({
  selector: 'app-medical-record-due-list',
  imports: [
    FormsModule,
    RouterLink,
    DatePipe,
    CommonModule
  ],
  templateUrl: './medical-record-due-list.html',
  styleUrl: './medical-record-due-list.css'
})
export class MedicalRecordDueList implements OnInit {
  private medicalRecordService = inject(MedicalRecordService)

  days = 7
  loading = false
  error?: string
  items: DueMedicalRecordDto[] = []

  ngOnInit(): void {
    this.load()
  }

  load(){
    this.loading = true
    this.error = ''
    this.medicalRecordService.gettDueMedicalRecords(this.days)
      .subscribe({
        next: data => {
          this.items = data
          this.loading = false

           // Aktualizujemy licznik w serwisie
          this.medicalRecordService.dueCount.set(data.length);
        },
        error: err =>{
          this.error = 'Nie udało się pobrać listy terminów.'
          this.loading = false
          console.error(err);
        }
      })
  }

  badge(rec: DueMedicalRecordDto){
    if(rec.overdue) return 'Zaległe'
    if(rec.daysUntilDue === 0) return 'Dziś'
    if((rec.daysUntilDue ?? 0) > 0) return `Za ${rec.daysUntilDue} d`
    return ''
  }

}
