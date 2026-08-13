import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-verify-email-page',
  standalone: true,
  templateUrl: './verify-email-page.html',
  styleUrl: './auth-page.scss'
})
export class VerifyEmailPage implements OnInit {
  private route = inject(ActivatedRoute);
  router = inject(Router);
  private authService = inject(AuthService);

  status = 'Verifying your email...';
  success = false;
  verifying = true;

  ngOnInit() {
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!token) {
      this.status = 'Invalid verification link.';
      this.verifying = false;
      return;
    }

    this.authService.verifyEmail(token).subscribe({
      next: (response) => {
        console.log('VERIFY EMAIL SUCCESS:', response);

        localStorage.setItem('ascendlyEmailVerified', 'true');

        this.success = true;
        this.verifying = false;
        this.status = 'Email verified successfully!';
      },
      error: (error) => {
        console.error('VERIFY EMAIL ERROR:', error);

        this.success = false;
        this.verifying = false;
        this.status = 'This verification link is invalid or expired.';
      }
    });
  }
} 