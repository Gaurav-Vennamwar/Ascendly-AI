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

  ngOnInit() {
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!token) {
      this.status = 'Invalid verification link.';
      return;
    }

    this.authService.verifyEmail(token).subscribe({
      next: () => {
        this.success = true;

        // Preserve verification state across the email link's browser tab.
        localStorage.setItem('ascendlyEmailVerified', 'true');

        this.status = 'Email verified successfully!';

        setTimeout(() => {
          this.router.navigate(['/register']);
        }, 1000);
      },
      error: () => {
        this.status = 'This verification link is invalid or expired.';
      }
    });
  }
}
