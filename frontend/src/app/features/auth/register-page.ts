import { Component, inject, signal, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';


@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './register-page.html',
  styleUrl: './auth-page.scss'
})
export class RegisterPage implements OnInit{
  private authService = inject(AuthService);
  private router = inject(Router);

  fullName = '';
  email = '';
  password = '';
  confirmPassword = '';

  private _verificationRequested = signal(false);
  private _verificationNotice = signal('');

  emailVerified = false;
  loading = false;

  verificationRequested = this._verificationRequested.asReadonly();
  verificationNotice = this._verificationNotice.asReadonly();

  private readonly pendingRegistrationKey = 'ascendlyPendingRegistration';
  private readonly emailVerifiedKey = 'ascendlyEmailVerified';

  onEmailChange() {
    this.emailVerified = false;
    localStorage.removeItem(this.emailVerifiedKey);
    this._verificationRequested.set(false);
    this._verificationNotice.set('');
  }

  requestVerification() {
    if (!this.fullName || !this.email) {
      this._verificationNotice.set('Enter your full name and email first.');
      return;
    }

    localStorage.setItem(
      this.pendingRegistrationKey,
      JSON.stringify({ fullName: this.fullName.trim(), email: this.email.trim() })
    );

    this.loading = true;

    this.authService.requestEmailVerification({
      fullName: this.fullName,
      email: this.email
    }).subscribe({
      next: () => {
        this._verificationRequested.set(true);
        this._verificationNotice.set(
          'Verification email sent. Check your inbox and click the verification link.'
        );
        this.loading = false;
      },
      error: (error) => {
        console.error('Verification email failed:', error);
        this._verificationNotice.set(
          'Unable to send verification email. Please try again.'
        );
        this.loading = false;
      }
    });
  }

  register() {
    if (!this.emailVerified) {
      return;
    }

    this.authService.register({
      fullName: this.fullName,
      email: this.email,
      password: this.password,
      confirmPassword: this.confirmPassword
    }).subscribe({
      next: () => {
        localStorage.removeItem(this.pendingRegistrationKey);
        localStorage.removeItem(this.emailVerifiedKey);
        this.router.navigate(['/login']);
      },
      error: (error) => {
        console.error('Registration failed:', error);
      }
    });
  }
  ngOnInit() {
  const pendingRegistration = localStorage.getItem(this.pendingRegistrationKey);

  if (pendingRegistration) {
    try {
      const saved = JSON.parse(pendingRegistration) as { fullName?: string; email?: string };
      this.fullName = saved.fullName ?? '';
      this.email = saved.email ?? '';
    } catch {
      localStorage.removeItem(this.pendingRegistrationKey);
    }
  }

  this.emailVerified = localStorage.getItem(this.emailVerifiedKey) === 'true';

  if (this.emailVerified) {
    this._verificationRequested.set(true);
    this._verificationNotice.set('Email verified successfully.');
  }
}
}
