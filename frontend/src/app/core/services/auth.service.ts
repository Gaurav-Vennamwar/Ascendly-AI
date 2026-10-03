import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpErrorResponse } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface AuthResponse {
  accessToken: string;
//   refreshToken: string;
  expiresAt: string;
}

export interface CurrentUserResponse {
  userId: string;
  fullName: string;
  email: string;
  role: string;
  interviewSessionCount: number;
  completedInterviewSessionCount: number;
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
  getCurrentUser() {
  // Calls the protected /me endpoint.
  // The HTTP interceptor automatically adds:
  // Authorization: Bearer <accessToken>
  return this.http.get<CurrentUserResponse>(`${this.apiUrl}/me`);
}

  getAuthErrorMessage(error: unknown, operation: 'login' | 'register' | 'verification'): string {
    if (!(error instanceof HttpErrorResponse)) {
      return 'Something went wrong. Please try again.';
    }

    if (error.status === 0) {
      return 'We could not reach Ascendly AI. Check your connection and try again.';
    }

    if (operation === 'login' && (error.status === 400 || error.status === 401)) {
      return 'Incorrect email or password.';
    }

    if (operation === 'verification' && error.status === 400) {
      return 'An account may already exist for this email. Sign in or use a different email.';
    }

    if (operation === 'register' && error.status === 400) {
      return 'We could not create your account. Verify your email first, then try again.';
    }

    return 'Something went wrong. Please try again.';
  }
refreshAccessToken() {
  // The refresh token is NOT read here.
  // Browser automatically sends the HttpOnly cookie.
  return this.http.post<AuthResponse>(
    `${this.apiUrl}/refresh`,
    {},
    {
      withCredentials: true,
    }
  );
}

}
