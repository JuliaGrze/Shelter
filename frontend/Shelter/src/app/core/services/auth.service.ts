import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { enviroment } from '../../../environments/environment';
import { AuthResponse, LoginDto, RegisterDto, UserProfile } from '../models/auth';
import { tap } from 'rxjs';

const TOKEN_KEY = 'auth_token';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient)
  private baseUrl = `${enviroment.apiUrl}`

  //stan uzytkownika
  private _token = signal<string | null>(this.getStoredToken())
  private _profile = signal<UserProfile | null>(null)

  readonly isLoggedIn = computed(() => !!this._token())
  //sposob na udostępnienie _profile swiatu na zewnatrz,ale bez pozwolenia na modyfikacje
  readonly profile = this._profile.asReadonly();

  getStoredToken() : string | null {
    return localStorage.getItem(TOKEN_KEY)
  }

  private storeToken(token: string){
    this._token.set(token)
    localStorage.setItem(TOKEN_KEY, token)
  }

  private clearToken(){
    this._token.set(null);
    localStorage.removeItem(TOKEN_KEY)
  }

  get token(): string | null {
    return this._token();
  }

  register(dto: RegisterDto){
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/register`, dto).pipe(
      tap(res => {
        this.storeToken(res.token)
        this._profile.set({
          userId: res.userId,
          email: res.email,
          firstName: res.firstName,
          lastName: res.lastName,
          roles: res.roles
        })
      })
    )
  }

  login(dto: LoginDto){
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/login`, dto).pipe(
      tap(res => {
        this.storeToken(res.token);
        this._profile.set({
          userId: res.userId,
          email: res.email,
          roles: res.roles,
          firstName: res.firstName,
          lastName: res.lastName
        });
      })
    )
  }

  /** pobranie /me aby odświeżyć dane profilu (np. po F5) */
  fetchMe(){
    return this.http.get<AuthResponse>(`${this.baseUrl}/auth/me`).pipe(
      tap(res =>{
        // /me nie zwraca nowego tokena w backendzie — aktualizujemy tylko profil
        this._profile.set({
          userId: res.userId,
          email: res.email,
          roles: res.roles,
          firstName: res.firstName,
          lastName: res.lastName
        });
      })
    )
  }

  logout(){
    this.clearToken()
    this._profile.set(null)
  }
}
