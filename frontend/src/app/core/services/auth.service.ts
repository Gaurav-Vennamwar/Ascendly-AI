import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);

  private apiUrl = `${environment.apiBaseUrl}/Auth`;
    //email request verification service
    requestEmailVerification(data: {
    fullName: string;
    email: string;
  }) {
    return this.http.post(
      `${this.apiUrl}/request-email-verification`,
      data,
      { responseType: 'text' }
    );
  }
  //verify the email with the token 
  verifyEmail(token: string) {
    return this.http.post(
      `${this.apiUrl}/verify-email`,
      { token },
      { responseType: 'text' }
    );
  }
  //register method
  register(data: {
    fullName: string;
    email: string;
    password: string;
    confirmPassword: string;
  }) {
    return this.http.post(`${this.apiUrl}/register`, data, {
      responseType: 'text'
    });
  }
  //login method
  login(data: {
    email: string;
    password: string;
  }) {
    return this.http.post(`${this.apiUrl}/login`, data, {
      withCredentials: true
    });
  }

}