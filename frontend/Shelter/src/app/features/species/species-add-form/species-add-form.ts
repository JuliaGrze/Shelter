import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { SpeciesService } from '../../../core/services/species.service';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-species-add-form',
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './species-add-form.html',
  styleUrl: './species-add-form.css'
})
export class SpeciesAddForm {
  private formBuilder = inject(FormBuilder)
  private speciesService = inject(SpeciesService)
  private router = inject(Router)

  loading = false
  submitted = false

  form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]]
  })

  get f() {return this.form.controls}

  submit(){
    this.submitted = true
    if (this.form.invalid) return
    this.loading = true

    this.speciesService.addSpecies(this.form.value.name!).subscribe({
      next: () => this.router.navigateByUrl('/animals'),
      error: () => this.loading = false,
      complete: () => this.loading = false
    })
  }
}
