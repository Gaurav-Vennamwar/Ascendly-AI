import { Component, input } from '@angular/core';
import { WorkspaceLayout } from '../../shared/components/workspace-layout/workspace-layout';

export interface InterviewReport { overallSummary: string; recommendation: string; nextSteps: string[]; }
@Component({ selector: 'app-interview-report-page', standalone: true, imports: [WorkspaceLayout], templateUrl: './interview-report-page.html', styleUrl: './interview-flow.scss' })
export class InterviewReportPage { report = input<InterviewReport | null>(null); }
