import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService)
  const router = inject(Router)
  const required = (route.data?.['roles'] as string[] | undefined) ?? [] 

  //brak wymagan => wpuszczamy
  if(!required.length) return true

  if(authService.hasAnyRole(required)) return true

  //brak uprawnien => przekieruj i anuluj
  router.navigateByUrl('/animals')
  return false
  
};
