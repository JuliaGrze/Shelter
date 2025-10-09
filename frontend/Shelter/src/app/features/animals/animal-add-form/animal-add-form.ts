import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { AnimalService } from '../../../core/services/animal.service';
import { SpeciesService } from '../../../core/services/species.service';
import { SpeciesDto } from '../../../core/models/species';
import { AnimalStatus, Sex } from '../../../core/models/animal';
import { enviroment } from '../../../../environments/environment';

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
  private http = inject(HttpClient);
  private router = inject(Router);

  speciesList: SpeciesDto[] = [];
  loading = false;
  submitted = false;

  // upload
  private pickedFile: File | null = null;
  uploadError: string | null = null;

  form = this.fb.group({
    name: ['', Validators.required],
    speciesId: (null as number | null),
    birthDate: ['', Validators.required],
    sex: ('Unknown' as Sex),
    status: ('Available' as AnimalStatus),
    description: [''],
    vaccinated: [false],
    neutered: [false],
  });

  ngOnInit(): void {
    this.speciesService.getAllSpecies().subscribe(list => (this.speciesList = list));
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
      // 1) CREATE → backend zwraca id
      const dto = {
        name: (this.form.value.name ?? '').trim(),
        speciesId: this.form.value.speciesId!,
        birthDate: this.form.value.birthDate!,
        sex: this.form.value.sex!,
        status: this.form.value.status!,
        description: (this.form.value.description ?? '').trim(),
        vaccinated: !!this.form.value.vaccinated,
        neutered: !!this.form.value.neutered,
        photoUrl: '' // uzupełnimy po uploadzie, jeśli jest plik
      };
      const id = await firstValueFrom(this.animalService.addAnimal(dto));

      // 2) Jeśli wybrano plik → upload po speciesName + id → update photoUrl
      if (this.pickedFile) {
        const speciesName = this.speciesList.find(s => s.id === dto.speciesId)?.name;
        if (!speciesName) throw new Error('Nie znaleziono nazwy gatunku.');
        const url = await this.uploadPhotoInternal(speciesName, id);
        await firstValueFrom(this.animalService.updateAnimal(id, { ...dto, photoUrl: url }));
      }

      // 3) powrót
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
