import { Component, signal } from '@angular/core';
import { ResumeService } from '../../core/services/resume.service';

import { WorkspaceLayout } from '../../shared/components/workspace-layout/workspace-layout';
import { ResumeUploadCard } from './components/resume-upload-card/resume-upload-card';
import { JobDescriptionCard } from './components/job-description-card/job-description-card';
import { AnalysisDashboard } from './components/analysis-dashboard/analysis-dashboard';
import { InsightCard } from './components/insight-card/insight-card';
import { ResumeAnalysisResponse } from './models/resume-analysis-response';

@Component({
  selector: 'app-resume-analyzer',
  standalone: true,
  imports: [WorkspaceLayout, ResumeUploadCard, JobDescriptionCard, AnalysisDashboard, InsightCard],
  templateUrl: './resume-analyzer-page.html',
  styleUrl: './resume-analyzer-page.scss',
})
export class ResumeAnalyzer {
  selectedResume: File | null = null;
  jobDescription = '';

  analysis = signal<ResumeAnalysisResponse | null>(null);
  isAnalyzing = signal(false);
  errorMessage = signal('');
  analysisDurationSeconds = signal<number | null>(null);

  constructor(private readonly resumeService: ResumeService) {}

  onResumeSelected(file: File): void {
    console.log('Parent received resume:', file.name);

    this.selectedResume = file;
    this.errorMessage.set('');
  }

  onJobDescriptionChanged(value: string): void {
    this.jobDescription = value;
    this.errorMessage.set('');
  }

  analyzeResume(): void {
    if (!this.selectedResume) {
      this.errorMessage.set('Please upload a PDF resume.');
      return;
    }

    if (!this.jobDescription.trim()) {
      this.errorMessage.set('Please provide a job description.');
      return;
    }

    this.errorMessage.set('');
    this.isAnalyzing.set(true);
    const startedAt = performance.now();

    this.resumeService.analyzeResume(this.selectedResume, this.jobDescription).subscribe({
      next: (response) => {
        console.log('Analysis response received:', response);

        this.analysis.set(response);
        this.analysisDurationSeconds.set(Math.max(1, Math.round((performance.now() - startedAt) / 1000)));
        this.isAnalyzing.set(false);

        console.log('Analysis stored:', this.analysis());
      },
      error: (error) => {
        console.error('Resume analysis failed:', error);

        this.errorMessage.set('Unable to analyze the resume. Please try again.');

        this.isAnalyzing.set(false);
      },
    });
  }
}
