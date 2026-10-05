import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { WorkspaceLayout } from '../../shared/components/workspace-layout/workspace-layout';
import { PrimaryButton } from '../../shared/components/primary-button/primary-button';
import { InterviewService, FinalInterviewEvaluation } from '../../core/services/interview.service';

@Component({
  selector: 'app-interview-report-page',
  standalone: true,
  imports: [CommonModule, WorkspaceLayout, PrimaryButton],
  templateUrl: './interview-report-page.html',
  styleUrl: './interview-flow.scss'
})
export class InterviewReportPage {
  readonly interview = inject(InterviewService);
  private readonly router = inject(Router);

  readonly finalEvaluation = computed<FinalInterviewEvaluation | null>(() => this.interview.finalEvaluation());
  readonly completing = computed<boolean>(() => this.interview.completing());
  readonly error = computed<string>(() => this.interview.error());
  readonly session = computed(() => this.interview.session());

  readonly targetRole = computed<string>(() => {
    return this.interview.configuration()?.targetRole || 'Target Role';
  });

  generateFinalEvaluation(): void {
    if (this.completing() || this.finalEvaluation()) return;
    this.interview.complete();
  }

  formatIndex(index: number): string {
    const num = index + 1;
    return num < 10 ? `0${num}` : `${num}`;
  }

  getReadinessClass(readiness: string): string {
    const normalized = (readiness || '').toUpperCase();
    if (normalized.includes('READY_WITH') || normalized.includes('WITH_PREPARATION')) {
      return 'readiness-prep';
    }
    if (normalized.includes('READY')) {
      return 'readiness-ready';
    }
    return 'readiness-needs-prep';
  }

  startNewSession(): void {
    this.interview.reset();
    this.router.navigate(['/mock-interview']);
  }

  goToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  goToSetup(): void {
    this.router.navigate(['/mock-interview']);
  }
}
