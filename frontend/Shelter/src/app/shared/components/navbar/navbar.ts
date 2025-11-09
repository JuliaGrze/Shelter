import { Component, computed, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { MedicalRecordService } from '../../../core/services/medical-record.service';


@Component({
  selector: 'app-navbar',
  imports: [RouterLink],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar implements OnInit {
  private authService = inject(AuthService)
  private medicalRecordService = inject(MedicalRecordService)
  private router = inject(Router)

  //sygnal z AuthService - true gdy mamy token
  isLoggedIn = this.authService.isLoggedIn

  //licznik zwierzat, ktora potrzebuje opieki medycznej
  dueCount = this.medicalRecordService.dueCount;

  canManage = computed(() => {
    const p = this.authService.profile();        // signal → odświeży się sam
    return !!p?.roles?.some(r => r === 'Admin' || r === 'Worker');
  });

  isLoginIn = computed(() => {
    const p = this.authService.profile();        // signal → odświeży się sam
    return !!p?.roles?.some(r => r === 'Client');
  })

  ngOnInit(): void {
    // Odśwież licznik przy starcie
    this.medicalRecordService.refreshDueCount(7);
  }


  //ladna etykieta uzytkownika do chipa
  userLabel = computed(() => {
    const p = this.authService.profile(); // sygnał/computed z AuthService
    if (!p) return '';
    return p.firstName ? `${p.firstName} ${p.lastName ?? ''}`.trim() : p.email;
  });

  logout()     
  { 
    this.authService.logout();
    this.router.navigateByUrl('/animals')
  }

}
