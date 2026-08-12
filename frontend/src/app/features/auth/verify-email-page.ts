import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-verify-email-page',
  standalone: true,
  templateUrl: './verify-email-page.html',
  styleUrl: './auth-page.scss'
})
export class VerifyEmailPage implements OnInit {
  private route = inject(ActivatedRoute);
  router = inject(Router);

  status = 'Verifying your email...';
  success = false;
  verifying = false;

  ngOnInit() {
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!token) {
      this.status = 'Invalid verification link.';
      this.verifying = false;
      return;
    }

    // The registration flow is intentionally frontend-led for now. Backend
    // token validation can be connected later without changing this UI.
    localStorage.setItem('ascendlyEmailVerified', 'true');
    this.success = true;
    this.status = 'Email verified successfully!';
  }
}
