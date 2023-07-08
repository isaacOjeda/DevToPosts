import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, of, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {

  constructor(private http: HttpClient) { }


  saveUser(user: any) {
    localStorage.setItem('user', JSON.stringify(user));
  }

  getUser() {
    return JSON.parse(localStorage.getItem('user')!);
  }

  isAuthenticated() {
    return !!this.getUser();
  }

  isInRole(role: string) {
    return this.isAuthenticated() && this.getUser().roles.includes(role);
  }

  login(username: string, password: string): Observable<any> {
    return this.http.post('local-login', { username, password })
      .pipe(
        catchError(error => {
          console.error('Error en la solicitud de inicio de sesión:', error);
          throw error;          
        }),
        tap((response: any) => {
          this.saveUser(response);
        })
      );
  }

  logout() {
    this.http.post('local-logout', {}).subscribe(result => {
      localStorage.removeItem('user');
      window.location.href = '/login';
    });
  }
}
