import { Component, HostListener, input, signal } from '@angular/core';
import { ScoreCard } from '../score-card/score-card';
import { ResumeAnalysisResponse } from '../../models/resume-analysis-response';

type InsightType =
  | 'keyword'
  | 'gaps'
  | 'tailoring'
  | 'humanization'
  | 'recommendation';

@Component({
  selector: 'app-analysis-dashboard',
  standalone: true,
  imports: [ScoreCard],
  templateUrl: './analysis-dashboard.html',
  styleUrl: './analysis-dashboard.scss',
})
export class AnalysisDashboard {
  // Backend response passed from the parent.
  analysis = input.required<ResumeAnalysisResponse>();

  activeInsight = signal<InsightType | null>(null);

  openInsight(type: InsightType): void {
    this.activeInsight.set(type);
  }

  closeInsight(): void {
    this.activeInsight.set(null);
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.closeInsight();
  }
}
