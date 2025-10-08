import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService)
  const router = inject(Router)
  const token = localStorage.getItem('auth_token')

  //Jesli uzytkownik jest zalogowany (ma token),to klonujesz zadanie i dodajesz do niego naglowek
  const authReq = token
    ? req.clone({setHeaders: { Authorization: `Bearer ${token}` }})
    : req

  //next(authReq) przekazuje zadanie dalej — do backendu
  return next(authReq).pipe(
    // optional: auto-logout przy 401
    catchError((err) => {
      if (err.status === 401) {
        auth.logout();
        router.navigateByUrl('/login');
      }
      return throwError(() => err);
    })
  );
};
