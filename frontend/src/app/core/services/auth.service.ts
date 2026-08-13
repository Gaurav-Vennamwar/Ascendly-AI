import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface AuthResponse {
  accessToken: string;
//   refreshToken: string;
  expiresAt: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);

  private apiUrl = `${environment.apiBaseUrl}/Auth`;

  requestEmailVerification(data: { fullName: string; email: string }) {
    return this.http.post(`${this.apiUrl}/request-email-verification`, data, {
      responseType: 'text',
    });
  }

  verifyEmail(token: string) {
    return this.http.post(
      `${this.apiUrl}/verify-email`,
      { token },
      { responseType: 'text' }
    );
  }

  register(data: {
    fullName: string;
    email: string;
    password: string;
    confirmPassword: string;
  }) {
    return this.http.post(`${this.apiUrl}/register`, data, {
      responseType: 'text',
    });
  }

  login(data: { email: string; password: string }) {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data, {
      withCredentials: true,
    });
  }
//   //Save both tokens in one central place.
//   // Login component doesn't need to know how storage works.
//   setTokens(response: AuthResponse): void {
//     localStorage.setItem('accessToken', response.accessToken);
//     localStorage.setItem('refreshToken', response.refreshToken);
//   }
// Storing  only the access token.
// Refresh token is handled securely by the HttpOnly cookie.
setAccessToken(token: string): void {
  localStorage.setItem('accessToken', token);
}

  getAccessToken(): string | null {
      // Anyone who needs the access token asks AuthService.
    return localStorage.getItem('accessToken');
  }

//   getRefreshToken(): string | null {
//     return localStorage.getItem('refreshToken');
//   }

logout() {
  // Backend reads the refresh token from the HttpOnly cookie.
  // Angular does NOT send or read the refresh token manually.
  return this.http.post(
    `${this.apiUrl}/logout`,
    {},
    { withCredentials: true, responseType: 'text' }
  );
}
  clearTokens(): void {
    // Remove authentication data during logout.
    localStorage.removeItem('accessToken');
    // localStorage.removeItem('refreshToken');
  }

}