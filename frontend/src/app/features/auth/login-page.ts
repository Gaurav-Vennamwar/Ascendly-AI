import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './login-page.html',
  styleUrl: './auth-page.scss',
})
export class LoginPage {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';

  loading = false;
  errorMessage = '';

  login() {
    if (!this.email || !this.password) {
      this.errorMessage = 'Please enter your email and password.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authService
      .login({
        email: this.email,
        password: this.password,
      })
      .subscribe({
        next: (response) => {
          console.log('Login successful:', response);

          this.loading = false;

          // this.authService.setTokens(response);
          // Store only the access token.
          // Refresh token is already stored in the HttpOnly cookie by the backend.
          this.authService.setAccessToken(response.accessToken);

          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          console.error('Login failed:', error);

          this.loading = false;
          this.errorMessage = error.error || 'Invalid email or password.';
        },
      });
  }
}
