import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AnimalService } from '../../../core/services/animal.service';
import { SpeciesService } from '../../../core/services/species.service';
import { HttpClient } from '@angular/common/http';
import { SpeciesDto } from '../../../core/models/species';
import { AnimalDto, AnimalStatus, Sex } from '../../../core/models/animal';
import { firstValueFrom } from 'rxjs';
import { enviroment } from '../../../../environments/environment';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-animal-edit-delete-form',
  imports: [
    ReactiveFormsModule, 
    RouterLink,
    NgIf,
    FormsModule
  ],
  templateUrl: './animal-edit-delete-form.html',
  styleUrl: './animal-edit-delete-form.css'
})
export class AnimalEditDeleteForm implements OnInit {
  private formBuilder = inject(FormBuilder)
  private router = inject(Router)
  private route = inject(ActivatedRoute)
  private animalService = inject(AnimalService)
  private speciesService = inject(SpeciesService)
  private http = inject(HttpClient)

  speciesList: SpeciesDto[] = []
  loading = true
  saving = false
  error: string | null = null

  animalId!: number
  currentAnimal?: AnimalDto

  //upload
  private pickedFile: File | null = null
  uploadError: string | null = null
  selectedFileName: string | null = null

  //delete
  deleting = false
  confirmOpen = false
  confirmText = ''

  form = this.formBuilder.group({
    name: ['', Validators.required],
    speciesId: (null as number | null),
    birthDate: ['', Validators.required],
    sex: ('Unknown' as Sex),
    status: ('Available' as AnimalStatus),
    description: [''],
    vaccinated: [false],
    neutered: [false],
  })

  ngOnInit(): void {
    this.animalId = Number(this.route.snapshot.paramMap.get('id'))
    if(!this.animalId){
      this.error = 'Brak identyfikatora zwierzaka w adresie URL.'
      this.loading = false
      return
    }

    //pobieranie gatunkow i dane zwierzat  rownolegle
    Promise.all([
      firstValueFrom(this.speciesService.getAllSpecies()),
      firstValueFrom(this.animalService.getAnimalById(this.animalId))
    ]).then(([species, animal]) =>  {
      this.speciesList = species
      this.currentAnimal = animal

      this.form.patchValue({
        name: animal.name,
        speciesId: animal.speciesId,
        birthDate: animal.birthDate,
        sex: animal.sex as Sex,
        status: animal.status as AnimalStatus,
        description: animal.description ?? '',
        vaccinated: !!animal.vaccinated,
        neutered: !!animal.neutered,
      })
    })
    .catch(() => {
      this.error = 'Nie udało się pobrać danych zwierzaka lub listy gatunków.'
    })
    .finally(() => {
      this.loading = false;
    })
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
    this.selectedFileName = file.name;
    this.pickedFile = file
  }

  async submit() {
    if (this.form.invalid || !this.form.value.speciesId || !this.currentAnimal) return;

    this.saving = true;
    try {
      // baza DTO z formularza
      const dto = {
        name: (this.form.value.name ?? '').trim(),
        speciesId: this.form.value.speciesId!,
        birthDate: this.form.value.birthDate!,
        sex: this.form.value.sex!,
        status: this.form.value.status!,
        description: (this.form.value.description ?? '').trim(),
        vaccinated: !!this.form.value.vaccinated,
        neutered: !!this.form.value.neutered,
        photoUrl: this.currentAnimal.photoUrl ?? ''
      };

      // jeśli wybrano nowy plik → upload po speciesName (zaktualizowanego gatunku) i id, potem update z url
      if (this.pickedFile) {
        const speciesName = this.speciesList.find(s => s.id === dto.speciesId)?.name;
        if (!speciesName) throw new Error('Nie znaleziono nazwy gatunku.');
        const url = await this.uploadPhotoInternal(speciesName, this.animalId);
        dto.photoUrl = url;
      }

      await firstValueFrom(this.animalService.updateAnimal(this.animalId, dto));
      this.router.navigateByUrl('/animals');
    } catch (err) {
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

  openDeleteConfirm(){
    this.confirmOpen = true;
    this.confirmText = '';
  }

  cancelDeleteConfirm() {
    this.confirmOpen = false;
    this.confirmText = '';
  }

  async deleteAnimal(){
    if(!this.currentAnimal) return

    //dodatkowe zabezpieczenie
    if(this.confirmText !== this.currentAnimal.name) return

    this.deleting = true
    try{
      await firstValueFrom(this.animalService.deleteAnimal(this.animalId));
      this.router.navigateByUrl('/animals');
    }catch {
      this.error = 'Nie udało się usunąć zwierzaka.';
    }
    finally {
      this.deleting = false;
    }
  }
}
