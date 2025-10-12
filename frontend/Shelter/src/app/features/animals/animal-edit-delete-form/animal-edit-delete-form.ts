import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { NgIf, NgFor, DatePipe } from '@angular/common';
import { firstValueFrom } from 'rxjs';

import { AnimalService } from '../../../core/services/animal.service';
import { SpeciesService } from '../../../core/services/species.service';
import { MedicalRecordService } from '../../../core/services/medical-record.service';

import { SpeciesDto } from '../../../core/models/species';
import { AnimalDto, AnimalStatus, Sex } from '../../../core/models/animal';
import { CreateMedicalRecord, MedicalRecord, MedicalRecordType } from '../../../core/models/medical-record';
import { enviroment } from '../../../../environments/environment';

type RecRowState = {
  editing: boolean;
  saving: boolean;
  deleting: boolean;
  confirmDelete: boolean;
  confirmText: string;
  disableNext: boolean;
  draft: {
    date: string;
    nextDueDate: string | null;
    vet: string | null;
    notes: string | null;
  };
};

@Component({
  selector: 'app-animal-edit-delete-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    FormsModule,
    RouterLink,
    NgIf,
    NgFor,
    DatePipe
  ],
  templateUrl: './animal-edit-delete-form.html',
  styleUrl: './animal-edit-delete-form.css'
})
export class AnimalEditDeleteForm implements OnInit {
  private formBuilder = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private animalService = inject(AnimalService);
  private speciesService = inject(SpeciesService);
  private http = inject(HttpClient);
  private medicalService = inject(MedicalRecordService);

  speciesList: SpeciesDto[] = [];
  loading = true;
  saving = false;
  error: string | null = null;

  animalId!: number;
  currentAnimal?: AnimalDto;

  // upload
  private pickedFile: File | null = null;
  uploadError: string | null = null;
  selectedFileName: string | null = null;

  // delete
  deleting = false;
  confirmOpen = false;
  confirmText = '';

  // panel dodawania wpisów medycznych
  showMedicalForm = false;
  disableNextDueDate = false;
  pendingMedical: Omit<CreateMedicalRecord, 'animalId'>[] = [];

  // istniejące wpisy
  recLoading = false;
  recError = '';
  records: MedicalRecord[] = [];
  recState = new Map<number, RecRowState>();

  form = this.formBuilder.group({
    name: ['', Validators.required],
    speciesId: (null as number | null),
    birthDate: ['', Validators.required],
    sex: ('Unknown' as Sex),
    status: ('Available' as AnimalStatus),
    description: ['']
  });

  // formularz pojedynczego (nowego) wpisu medycznego
  medicalRecordForm = this.formBuilder.nonNullable.group({
    type: new FormControl<MedicalRecordType>('Vaccination', { nonNullable: true }),
    date: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    nextDueDate: new FormControl<string | null>(null),
    vet: new FormControl<string | null>(null),
    notes: new FormControl<string | null>(null),
  });

  ngOnInit(): void {
    this.animalId = Number(this.route.snapshot.paramMap.get('id'));
    if (!this.animalId) {
      this.error = 'Brak identyfikatora zwierzaka w adresie URL.';
      this.loading = false;
      return;
    }

    // pobieranie gatunków i danych zwierzaka równolegle
    Promise.all([
      firstValueFrom(this.speciesService.getAllSpecies()),
      firstValueFrom(this.animalService.getAnimalById(this.animalId))
    ])
    .then(([species, animal]) =>  {
      this.speciesList = species;
      this.currentAnimal = animal;

      this.form.patchValue({
        name: animal.name,
        speciesId: animal.speciesId,
        birthDate: animal.birthDate,
        sex: animal.sex as Sex,
        status: animal.status as AnimalStatus,
        description: animal.description ?? '',
      });

      // po załadowaniu danych – wczytaj wpisy medyczne
      this.loadMedicalRecords();
    })
    .catch(() => {
      this.error = 'Nie udało się pobrać danych zwierzaka lub listy gatunków.';
    })
    .finally(() => {
      this.loading = false;
    });

    // wyłącz „następny termin” dla Sterilization (dla formularza dodawania)
    this.medicalRecordForm.controls.type.valueChanges.subscribe(t => {
      this.disableNextDueDate = t === 'Sterilization';
      if (this.disableNextDueDate) {
        this.medicalRecordForm.controls.nextDueDate.setValue(null);
      }
    });
  }

  // === Istniejące wpisy medyczne ===
  loadMedicalRecords() {
    this.recLoading = true;
    this.recError = '';
    firstValueFrom(this.medicalService.getMedicalRecordByAnimal(this.animalId))
      .then(list => {
        // sort DESC po dacie + tie-break po id
        this.records = [...list].sort((a, b) => {
          const da = new Date(a.date).getTime();
          const db = new Date(b.date).getTime();
          if (db !== da) return db - da;
          return b.id - a.id;
        });

        this.recState.clear();
        for (const r of this.records) {
          this.recState.set(r.id, {
            editing: false,
            saving: false,
            deleting: false,
            confirmDelete: false,
            confirmText: '',
            disableNext: r.type === 'Sterilization',
            draft: {
              date: r.date,
              nextDueDate: r.type === 'Sterilization' ? null : (r.nextDueDate ?? null),
              vet: r.vet ?? null,
              notes: r.notes ?? null,
            }
          });
        }
      })
      .catch(() => this.recError = 'Nie udało się pobrać wpisów medycznych.')
      .finally(() => this.recLoading = false);
  }

  startEditRec(r: MedicalRecord) {
    const st = this.recState.get(r.id);
    if (!st) return;
    st.draft = {
      date: r.date,
      nextDueDate: r.type === 'Sterilization' ? null : (r.nextDueDate ?? null),
      vet: r.vet ?? null,
      notes: r.notes ?? null,
    };
    st.disableNext = r.type === 'Sterilization';
    st.editing = true;
  }

  cancelEditRec(r: MedicalRecord) {
    const st = this.recState.get(r.id);
    if (!st) return;
    st.editing = false;
  }

  async saveEditRec(r: MedicalRecord) {
    const st = this.recState.get(r.id);
    if (!st) return;

    st.saving = true;
    try {
      // Backend NIE pozwala zmieniać Type ani AnimalId
      const payload: CreateMedicalRecord = {
        animalId: r.animalId,
        type: r.type,
        date: st.draft.date,
        nextDueDate: st.disableNext ? null : (st.draft.nextDueDate || null),
        vet: (st.draft.vet ?? '').trim() || null,
        notes: (st.draft.notes ?? '').trim() || null
      };

      await firstValueFrom(this.medicalService.updateMedicalRecord(r.id, payload));

      // uaktualnij lokalny rekord
      r.date = payload.date;
      r.nextDueDate = payload.nextDueDate ?? null;
      r.vet = payload.vet ?? null;
      r.notes = payload.notes ?? null;

      st.editing = false;

      // ⇦ REFRESH NAVBAR COUNTER (np. dla zakresu 7 dni)
      this.medicalService.refreshDueCount(7);

    } catch {
      alert('Nie udało się zapisać zmian wpisu medycznego.');
    } finally {
      st.saving = false;
    }
  }

  openDeleteRec(r: MedicalRecord) {
    const st = this.recState.get(r.id);
    if (!st) return;
    st.confirmDelete = true;
    st.confirmText = '';
  }

  cancelDeleteRec(r: MedicalRecord) {
    const st = this.recState.get(r.id);
    if (!st) return;
    st.confirmDelete = false;
    st.confirmText = '';
  }

  async confirmDeleteRec(r: MedicalRecord) {
    const st = this.recState.get(r.id);
    if (!st) return;
    // proste potwierdzenie: przepisz typ
    if (st.confirmText !== r.type) return;

    st.deleting = true;
    try {
      await firstValueFrom(this.medicalService.deleteMedicalRecord(r.id));
      this.records = this.records.filter(x => x.id !== r.id);
      this.recState.delete(r.id);

      // ⇦ REFRESH NAVBAR COUNTER
      this.medicalService.refreshDueCount(7);

    } catch {
      alert('Nie udało się usunąć wpisu medycznego.');
    } finally {
      st.deleting = false;
    }
  }

  // === Dodawanie nowych wpisów (pending) ===
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

  // === Upload ===
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
    this.selectedFileName = file.name;
    this.pickedFile = file;
  }

  async submit() {
    if (this.form.invalid || !this.form.value.speciesId || !this.currentAnimal) return;

    this.saving = true;
    try {
      // DTO zwierzaka
      const dto = {
        name: (this.form.value.name ?? '').trim(),
        speciesId: this.form.value.speciesId!,
        birthDate: this.form.value.birthDate!,
        sex: this.form.value.sex!,
        status: this.form.value.status!,
        description: (this.form.value.description ?? '').trim(),
        photoUrl: this.currentAnimal.photoUrl ?? ''
      };

      // upload zdjęcia (opcjonalnie)
      if (this.pickedFile) {
        const speciesName = this.speciesList.find(s => s.id === dto.speciesId)?.name;
        if (!speciesName) throw new Error('Nie znaleziono nazwy gatunku.');
        const url = await this.uploadPhotoInternal(speciesName, this.animalId);
        dto.photoUrl = url;
      }

      // update zwierzaka
      await firstValueFrom(this.animalService.updateAnimal(this.animalId, dto));

      // utworzenie pending wpisów medycznych
      for (const rec of this.pendingMedical) {
        const payload: CreateMedicalRecord = { animalId: this.animalId, ...rec };
        await firstValueFrom(this.medicalService.createMedicalRecord(payload));
      }

       // ⇦ REFRESH NAVBAR COUNTER (po zakończeniu wszystkich create)
      this.medicalService.refreshDueCount(7);

      this.router.navigateByUrl('/animals');
    } catch {
      this.error = 'Nie udało się zapisać zmian.';
    } finally {
      this.saving = false;
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

  // === Delete animal ===
  openDeleteConfirm(){
    this.confirmOpen = true;
    this.confirmText = '';
  }

  cancelDeleteConfirm() {
    this.confirmOpen = false;
    this.confirmText = '';
  }

  async deleteAnimal(){
    if (!this.currentAnimal) return;
    if (this.confirmText !== this.currentAnimal.name) return;

    this.deleting = true;
    try {
      await firstValueFrom(this.animalService.deleteAnimal(this.animalId));
      this.router.navigateByUrl('/animals');
    } catch {
      this.error = 'Nie udało się usunąć zwierzaka.';
    } finally {
      this.deleting = false;
    }
  }
}
