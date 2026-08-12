import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { timeout } from 'rxjs';

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
  private token = '';

  ngOnInit() {
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!token) {
      this.status = 'Invalid verification link.';
      this.verifying = false;
      return;
    }

    this.token = token;
    this.verify();
  }

  verify(): void {
    if (!this.token) {
      return;
    }

    this.verifying = true;
    this.success = false;
    this.status = 'Verifying your email...';

    this.authService.verifyEmail(this.token).pipe(timeout(20000)).subscribe({
      next: () => {
        this.success = true;
        this.verifying = false;

        // Preserve verification state across the email link's browser tab.
        localStorage.setItem('ascendlyEmailVerified', 'true');

        this.status = 'Email verified successfully!';

        setTimeout(() => {
          this.router.navigate(['/register']);
        }, 1000);
      },
      error: (error) => {
        this.verifying = false;
        this.status = error.name === 'TimeoutError'
          ? 'Verification is taking longer than expected. Please try again.'
          : 'This verification link is invalid, expired, or could not be processed.';
      }
    });
  }
}
