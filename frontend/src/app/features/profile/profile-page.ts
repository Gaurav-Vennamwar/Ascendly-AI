import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { WorkspaceLayout } from '../../shared/components/workspace-layout/workspace-layout';
import { MetricCard } from '../../shared/components/metric-card/metric-card';
import { PrimaryButton } from '../../shared/components/primary-button/primary-button';
import {  OnInit } from '@angular/core';
import { AccountStateService } from '../../core/services/account-state.service';

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [WorkspaceLayout, MetricCard],
  templateUrl: './profile-page.html',
  styleUrl: './profile-page.scss',
})
export class ProfilePage {
  private authService = inject(AuthService);
  private router = inject(Router);
  readonly account = inject(AccountStateService);

   ngOnInit(): void {
    this.account.load();
  }

  logout(): void {
    // Backend revokes the refresh token.
    this.authService.logout().subscribe({
      next: () => {
        // Remove the access token from browser storage.
        this.authService.clearTokens();
        this.account.clear();

        // Redirect user to login.
        this.router.navigate(['/login']);
      },
      error: () => {
        // Even if backend logout fails, clear local session.
        this.authService.clearTokens();
        this.account.clear();
        this.router.navigate(['/login']);
      }
    });
  }
}
