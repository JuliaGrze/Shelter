import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';


@Component({
  selector: 'app-navbar',
  imports: [RouterLink],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar {
  private authService = inject(AuthService)
  private router = inject(Router)

  //sygnal z AuthService - true gdy mamy token
  isLoggedIn = this.authService.isLoggedIn

  canManage = computed(() => {
    const p = this.authService.profile();        // signal → odświeży się sam
    return !!p?.roles?.some(r => r === 'Admin' || r === 'Worker');
  });


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
