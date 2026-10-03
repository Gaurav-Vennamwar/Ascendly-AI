import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { WorkspaceLayout } from '../../shared/components/workspace-layout/workspace-layout';
import { MetricCard } from '../../shared/components/metric-card/metric-card';
import { PrimaryButton } from '../../shared/components/primary-button/primary-button';
import { AccountStateService } from '../../core/services/account-state.service';
@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [WorkspaceLayout, MetricCard, PrimaryButton, RouterLink],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss',
})
export class DashboardPage {
  readonly account = inject(AccountStateService);
  constructor() { this.account.load(); }
  get careerIdentity(): string { const role = this.account.user()?.role; return role && role !== 'User' ? role : 'Not provided'; }
}
