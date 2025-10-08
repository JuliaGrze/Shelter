import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  loading = false;
  error: string | null = null;

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    firstName: [''],
    lastName: ['']
  });

  submit() {
    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.error = null;

    this.authService.register(this.form.value as any).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/animals';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Rejestracja nieudana.';
        this.loading = false;
      }
    });
  }

  get f() { return this.form.controls; }

  showEmailError() {
    const c = this.form.controls.email;
    return (c.dirty || c.touched) && c.invalid;
  }
  showPasswordError() {
    const c = this.form.controls.password;
    return (c.dirty || c.touched) && c.invalid;
  }
}
