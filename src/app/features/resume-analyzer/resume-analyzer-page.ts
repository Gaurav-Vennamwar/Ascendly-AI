import { Component } from '@angular/core';
import { AnalyzerSidebar } from './components/sidebar/sidebar';
import { AnalyzerTopbar } from './components/topbar/topbar';
import { ResumeUploadCard } from './components/resume-upload-card/resume-upload-card';
import { JobDescriptionCard } from './components/job-description-card/job-description-card';
import { AnalysisDashboard } from './components/analysis-dashboard/analysis-dashboard';
import { InsightCard } from './components/insight-card/insight-card';

@Component({ selector: 'app-resume-analyzer-page', standalone: true, imports: [AnalyzerSidebar, AnalyzerTopbar, ResumeUploadCard, JobDescriptionCard, AnalysisDashboard, InsightCard], templateUrl: './resume-analyzer-page.html', styleUrl: './resume-analyzer-page.scss' })
export class ResumeAnalyzerPage {}
