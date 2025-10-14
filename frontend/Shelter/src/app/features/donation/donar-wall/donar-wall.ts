import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { DonationService } from '../../../core/services/donation.service';
import { DonorWallItemDto } from '../../../core/models/donation';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-donar-wall',
  imports: [
    DatePipe,
    FormsModule
  ],
  templateUrl: './donar-wall.html',
  styleUrl: './donar-wall.css'
})
export class DonarWall implements OnInit {
  private donationService = inject(DonationService)

  loading = false
  error = ''
  donors: DonorWallItemDto[] = []
  

  readonly defaultTake = 50;
  take = this.defaultTake;
  takeOptions: number[] = [10, 20, 30, 50, 100];

  ngOnInit(): void {
    this.load(this.take)
  }

  load(take: number = 50){
    /** jeśli wywolane bez argumentu -> reset do 50 */
    this.take = typeof take === 'number' ? take : this.defaultTake;
    this.loading = true
    this.error = ''

    this.donationService.getPublicLatest(take).subscribe({
      next: (res) => {
        this.donors = res
        this.loading = false
      },
      error: (err) =>{
        this.error = 'Nie udało się załadować listy darczyńców.';
        console.error(err);
        this.loading = false
      }
    })
  }

  toMoney(minor: number) {
    return (minor / 100).toFixed(2);
  }

  onTakeChange() {
    this.load(this.take);
  }

}
