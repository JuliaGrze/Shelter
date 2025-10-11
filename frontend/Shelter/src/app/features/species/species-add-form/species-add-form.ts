import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NgIf } from '@angular/common';
import { SpeciesService } from '../../../core/services/species.service';
import { CreateSpeciesDto } from '../../../core/models/species';

@Component({
  selector: 'app-species-add-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './species-add-form.html',
  styleUrls: ['./species-add-form.css']
})
export class SpeciesAddForm implements OnInit {
  private fb = inject(FormBuilder);
  private speciesService = inject(SpeciesService);
  private router = inject(Router);

  loading = false;
  submitted = false;
  apiError = '';

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    requiresPermit: [false],
    permitName: [{ value: '', disabled: true }],
    permitAuthority: [{ value: '', disabled: true }],
    permitNotes: ['']
  });

  ngOnInit(): void {
    // Walidacja warunkowa: gdy requiresPermit=true, wymagaj permitName i permitAuthority
    this.form.get('requiresPermit')!.valueChanges.subscribe(req => {
      const permitName = this.form.get('permitName')!;
      const permitAuthority = this.form.get('permitAuthority')!;

      if (req) {
        permitName.enable();
        permitAuthority.enable();
        permitName.addValidators([Validators.required, Validators.maxLength(200)]);
        permitAuthority.addValidators([Validators.required, Validators.maxLength(200)]);
      } else {
        permitName.setValue('');
        permitAuthority.setValue('');
        permitName.clearValidators();
        permitAuthority.clearValidators();
        permitName.disable();
        permitAuthority.disable();
      }
      permitName.updateValueAndValidity({ emitEvent: false });
      permitAuthority.updateValueAndValidity({ emitEvent: false });
    });
  }

  get f() { return this.form.controls; }

  submit(): void {
    this.submitted = true;
    this.apiError = '';
    if (this.form.invalid) return;
    this.loading = true;

    const v = this.form.getRawValue(); // getRawValue, bo część pól może być disabled
    const dto: CreateSpeciesDto = {
      name: v.name.trim(),
      requiresPermit: !!v.requiresPermit,
      permitName: v.requiresPermit ? (v.permitName?.trim() || null) : null,
      permitAuthority: v.requiresPermit ? (v.permitAuthority?.trim() || null) : null,
      permitNotes: (v.permitNotes?.trim() || '') || null
    };

    this.speciesService.addSpecies(dto).subscribe({
      next: () => this.router.navigateByUrl('/animals'), // lub '/species' jeśli masz listę gatunków
      error: (err) => {
        // backend zwraca { message: "..."} dla 400/409
        this.apiError = err?.error?.message || 'Wystąpił błąd podczas zapisu.';
        this.loading = false;
      },
      complete: () => this.loading = false
    });
  }
}
