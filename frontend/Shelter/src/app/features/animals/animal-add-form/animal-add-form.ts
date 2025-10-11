import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { AnimalService } from '../../../core/services/animal.service';
import { SpeciesService } from '../../../core/services/species.service';
import { SpeciesDto } from '../../../core/models/species';
import { AnimalStatus, Sex } from '../../../core/models/animal';
import { enviroment } from '../../../../environments/environment';

import {
  CreateMedicalRecord,
  MedicalRecordType,
} from '../../../core/models/medical-record';
import { MedicalRecordService } from '../../../core/services/medical-record.service';

@Component({
  selector: 'app-animal-add-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './animal-add-form.html',
  styleUrls: ['./animal-add-form.css']
})
export class AnimalAddForm implements OnInit {
  private fb = inject(FormBuilder);
  private animalService = inject(AnimalService);
  private speciesService = inject(SpeciesService);
  private medicalService = inject(MedicalRecordService);
  private http = inject(HttpClient);
  private router = inject(Router);

  speciesList: SpeciesDto[] = [];
  loading = false;
  submitted = false;

  // upload
  private pickedFile: File | null = null;
  uploadError: string | null = null;

  // panel „Dodaj wpis medyczny”
  showMedicalForm = false;
  disableNextDueDate = false;
  pendingMedical: Omit<CreateMedicalRecord, 'animalId'>[] = [];

  form = this.fb.group({
    name: ['', Validators.required],
    speciesId: (null as number | null),
    birthDate: ['', Validators.required],
    sex: ('Unknown' as Sex),
    status: ('Available' as AnimalStatus),
    description: ['']
  });

  // formularz pojedynczego wpisu medycznego
  medicalRecordForm = this.fb.nonNullable.group({
    type: new FormControl<MedicalRecordType>('Vaccination', { nonNullable: true }),
    date: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    nextDueDate: new FormControl<string | null>(null),
    vet: new FormControl<string | null>(null),
    notes: new FormControl<string | null>(null),
  });

  ngOnInit(): void {
    this.speciesService.getAllSpecies().subscribe(list => (this.speciesList = list));

    // Dla typu Sterilization — „Następny termin” nie dotyczy
    this.medicalRecordForm.controls.type.valueChanges.subscribe(t => {
      this.disableNextDueDate = t === 'Sterilization';
      if (this.disableNextDueDate) {
        this.medicalRecordForm.controls.nextDueDate.setValue(null);
      }
    });
  }

  toggleMedicalForm() {
    this.showMedicalForm = !this.showMedicalForm;
  }

  resetMedicalForm() {
    this.medicalRecordForm.reset({
      type: 'Vaccination',
      date: '',
      nextDueDate: null,
      vet: null,
      notes: null
    });
    this.disableNextDueDate = false;
  }

  addMedicalRecord() {
    if (!this.medicalRecordForm.valid) return;
    const v = this.medicalRecordForm.getRawValue();

    this.pendingMedical.push({
      type: v.type,
      date: v.date,
      nextDueDate: this.disableNextDueDate ? null : (v.nextDueDate || null),
      vet: (v.vet ?? '').trim() || null,
      notes: (v.notes ?? '').trim() || null,
    });

    this.resetMedicalForm();
    this.showMedicalForm = false;
  }

  removeMedicalRecord(index: number) {
    this.pendingMedical.splice(index, 1);
  }

  onLocalPhotoPicked(e: Event) {
    const input = e.target as HTMLInputElement;
    const file = input.files?.[0];
    this.uploadError = null;
    this.pickedFile = null;
    if (!file) return;

    const allowed = ['image/jpeg', 'image/png', 'image/webp'];
    const max = 5 * 1024 * 1024;
    if (!allowed.includes(file.type)) {
      this.uploadError = 'Dozwolone: JPG/PNG/WEBP.';
      return;
    }
    if (file.size > max) {
      this.uploadError = 'Plik za duży (max 5 MB).';
      return;
    }
    this.pickedFile = file;
  }

  async submit() {
    this.submitted = true;
    if (this.form.invalid || !this.form.value.speciesId) return;

    this.loading = true;
    try {
      // 1) Utwórz zwierzaka
      const dto = {
        name: (this.form.value.name ?? '').trim(),
        speciesId: this.form.value.speciesId!,     // number
        birthDate: this.form.value.birthDate!,     // 'YYYY-MM-DD'
        sex: this.form.value.sex!,                 // 'Unknown' | 'Female' | 'Male'
        status: this.form.value.status!,           // 'Available' | 'Reserved' | 'Adopted' | 'NotAvailable'
        description: (this.form.value.description ?? '').trim(),
        photoUrl: ''                                // uzupełnimy po uploadzie, jeśli jest plik
      };
      const id = await firstValueFrom(this.animalService.addAnimal(dto));

      // 2) Upload zdjęcia (opcjonalnie) → update photoUrl
      if (this.pickedFile) {
        const speciesName = this.speciesList.find(s => s.id === dto.speciesId)?.name;
        if (!speciesName) throw new Error('Nie znaleziono nazwy gatunku.');
        const url = await this.uploadPhotoInternal(speciesName, id);
        await firstValueFrom(this.animalService.updateAnimal(id, { ...dto, photoUrl: url }));
      }

      // 3) Utwórz wpisy medyczne (jeśli są w pending)
      for (const rec of this.pendingMedical) {
        const payload: CreateMedicalRecord = { animalId: id, ...rec };
        await firstValueFrom(this.medicalService.createMedicalRecord(payload));
      }

      // 4) Powrót
      this.router.navigateByUrl('/animals');
    } finally {
      this.loading = false;
    }
  }

  private async uploadPhotoInternal(speciesName: string, id: number): Promise<string> {
    if (!this.pickedFile) throw new Error('Brak pliku.');
    const fd = new FormData();
    fd.append('file', this.pickedFile);

    const res = await firstValueFrom(
      this.http.post<{ url: string }>(
        `${enviroment.apiUrl}/upload/animals/by-species-name/${encodeURIComponent(speciesName)}/${id}`,
        fd
      )
    );
    if (!res?.url) throw new Error('Brak URL po uploadzie.');
    return res.url;
  }
}
