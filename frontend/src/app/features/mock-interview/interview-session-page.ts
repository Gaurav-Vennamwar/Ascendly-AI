import { Component, input, signal } from '@angular/core';
import { WorkspaceLayout } from '../../shared/components/workspace-layout/workspace-layout';
import { PrimaryButton } from '../../shared/components/primary-button/primary-button';

export interface InterviewQuestion { id: string; topic: string; prompt: string; }

@Component({ selector: 'app-interview-session-page', standalone: true, imports: [WorkspaceLayout, PrimaryButton], templateUrl: './interview-session-page.html', styleUrl: './interview-flow.scss' })
export class InterviewSessionPage {
  // Future session API binds the active question here; no question content is fabricated in the UI.
  question = input<InterviewQuestion | null>(null);
  questionNumber = input(0);
  totalQuestions = input(0);
  answer = signal('');
}
