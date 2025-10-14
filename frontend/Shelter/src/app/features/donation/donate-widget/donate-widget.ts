import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DonationService } from '../../../core/services/donation.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-donate-widget',
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './donate-widget.html',
  styleUrl: './donate-widget.css'
})
export class DonateWidget {
  private fb = inject(FormBuilder)
  private donationService = inject(DonationService)

  mode = signal<'ONE_TIME' | 'RECURRING'>('ONE_TIME')

  oneTimeForm = this.fb.group({
    amount: [5, [Validators.required, Validators.min(1)]],
    currency: ['PLN', Validators.required],
    donorPublicName: [''],
    isPublic: [true],
    message: ['']
  })
  
  recurringForm = this.fb.group({
    amount: [25, [Validators.required, Validators.min(1)]],
    currency: ['PLN', Validators.required],
    donorPublicName: [''],
    isPublic: [true],
    message: ['']
  });

  submitOneTime() {
    if (this.oneTimeForm.invalid) return;
    const v = this.oneTimeForm.value;

    this.donationService.createOneTime({
      amountMinor: Math.round((v.amount ?? 0) * 100),
      currency: v.currency ?? 'PLN',
      donorPublicName: v.donorPublicName ?? undefined,
      isPublic: !!v.isPublic,
      message: v.message ?? undefined
    }).subscribe(({ sessionUrl }) => {
      window.location.href = sessionUrl; // redirect do Stripe Checkout
    });
  }

  submitRecurring() {
  if (this.recurringForm.invalid) return;
  const v = this.recurringForm.value;
  this.donationService.createRecurring({
    amountMinor: Math.round((v.amount ?? 0) * 100),
    currency: v.currency!,
    donorPublicName: v.donorPublicName ?? undefined,
    isPublic: !!v.isPublic,
    message: v.message ?? undefined
  }).subscribe(({ sessionUrl }) => window.location.href = sessionUrl);
}

}
