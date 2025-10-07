import { Component, inject } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    RouterLink
],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  private authService = inject(AuthService)
  private fb = inject(FormBuilder)
  private router = inject(Router)
  private route = inject(ActivatedRoute)

  loading = false
  error: string | null = null
  submitted = false;

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  })

  submit(){
    if(this.form.invalid || this.loading){
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true
    this.error = null

    this.authService.login(this.form.value as any).subscribe({
      next: () =>{
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/animals'
        this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Nieudane logowanie.';
        this.loading = false;
      }
    })
  }

  // pomocniczo do walidacji w szablonie
  get f() { return this.form.controls; }

  // helpery do klas/komunikatów
  showEmailError() {
    const c = this.form.controls.email;
    return (this.submitted || c.dirty || c.touched) && c.invalid;
  }
  showPasswordError() {
    const c = this.form.controls.password;
    return (this.submitted || c.dirty || c.touched) && c.invalid;
  }

}
