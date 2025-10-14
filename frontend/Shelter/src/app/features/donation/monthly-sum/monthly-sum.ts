import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DonationService } from '../../../core/services/donation.service';
import { MonthlySumDto } from '../../../core/models/donation';
import { firstValueFrom } from 'rxjs';

/** Helper: month labels */
const MONTHS_PL = ['I', 'II', 'III', 'IV', 'V', 'VI', 'VII', 'VIII', 'IX', 'X', 'XI', 'XII'];

/** View model per currency (pivoted by month) */
interface CurrencyMonthlyVM {
  currency: string;
  /** amountMinor by month index 0..11 */
  totals: number[];
  /** count by month index 0..11 */
  counts: number[];
  /** yearly sums */
  yearTotalMinor: number;
  yearCount: number;
}

@Component({
  selector: 'app-monthly-sum',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './monthly-sum.html',
  styleUrl: './monthly-sum.css'
})
export class MonthlySum implements OnInit {
  private donationService = inject(DonationService);

  year = new Date().getFullYear();
  years: number[] = [];

  loading = false;
  error = '';

  raw: MonthlySumDto[] = [];
  view: CurrencyMonthlyVM[] = [];

  ngOnInit(): void {
    const thisYear = new Date().getFullYear();
    this.years = Array.from({ length: 5 }, (_, i) => thisYear - i);
    this.load();
  }

  async load() {
    this.error = '';
    this.loading = true;
    try {
      const data = await firstValueFrom(this.donationService.getMonthlySummary(this.year));
      this.raw = data ?? [];
      this.view = this.buildView(this.raw);
    } catch (e: any) {
      this.error = e?.error?.message || e?.message || 'Nie udało się pobrać danych.';
      this.raw = [];
      this.view = [];
    } finally {
      this.loading = false;
    }
  }

  /** Zbuduj widok: pivot miesięcy per waluta */
  private buildView(rows: MonthlySumDto[]): CurrencyMonthlyVM[] {
    const map = new Map<string, CurrencyMonthlyVM>();

    for (const r of rows) {
      const idx = (r.month ?? 0) - 1; // API daje 1..12
      if (idx < 0 || idx > 11) continue;

      if (!map.has(r.currency)) {
        map.set(r.currency, {
          currency: r.currency,
          totals: Array(12).fill(0),
          counts: Array(12).fill(0),
          yearTotalMinor: 0,
          yearCount: 0
        });
      }

      const bucket = map.get(r.currency)!;
      bucket.totals[idx] += r.amountMinor || 0;
      bucket.counts[idx] += r.count || 0;
      bucket.yearTotalMinor += r.amountMinor || 0;
      bucket.yearCount += r.count || 0;
    }

    return Array.from(map.values()).sort((a, b) => a.currency.localeCompare(b.currency));
  }

  fmtMajor(amountMinor: number, currency: string) {
    const major = (amountMinor || 0) / 100;
    try {
      return new Intl.NumberFormat('pl-PL', { style: 'currency', currency }).format(major);
    } catch {
      return `${major.toFixed(2)} ${currency}`;
    }
  }

  monthLabel(i: number) {
    return MONTHS_PL[i] ?? String(i + 1);
  }
}
