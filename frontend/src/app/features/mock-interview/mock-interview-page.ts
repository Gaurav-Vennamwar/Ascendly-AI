import { Component, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { WorkspaceLayout } from '../../shared/components/workspace-layout/workspace-layout';
import { PrimaryButton } from '../../shared/components/primary-button/primary-button';
import { ResumeUploadCard } from '../resume-analyzer/components/resume-upload-card/resume-upload-card';

@Component({
  selector: 'app-mock-interview-page', standalone: true,
  imports: [WorkspaceLayout, PrimaryButton, FormsModule, ResumeUploadCard],
  templateUrl: './mock-interview-page.html', styleUrl: './mock-interview-page.scss'
})
export class MockInterviewPage {
  readonly interviewTypes = ['Technical', 'HR', 'Behavioral', 'Mixed'];
  readonly difficulties = ['Beginner', 'Intermediate', 'Advanced'];
  readonly durations = [10, 20, 30, 45, 60];
  readonly styles = ['Friendly', 'Professional', 'Strict', 'Startup', 'Enterprise', 'FAANG Style', 'Founder Mode'];
  readonly personas = ['Technical Interviewer', 'Engineering Manager', 'HR Recruiter', 'Tech Lead', 'CTO', 'Founder', 'Product Manager', 'Recruiter'];
  selectedType = signal('Technical'); selectedRole = signal(''); selectedDifficulty = signal('Intermediate'); selectedDuration = signal(20); selectedStyle = signal('Professional'); selectedPersona = signal('Technical Interviewer');
  selectedResume = signal<File | null>(null);
  jobDescription = signal('');
  topics = signal<string[]>([]);
  topicInput = '';
  interviewerContext = '';

  constructor(private readonly router: Router) {}
  onResumeSelected(file: File): void { this.selectedResume.set(file); }
  startInterview(): void { this.router.navigate(['/mock-interview/session']); }
  addTopic(): void { const topic = this.topicInput.trim(); if (topic && !this.topics().includes(topic)) this.topics.update(items => [...items, topic]); this.topicInput = ''; }
  removeTopic(topic: string): void { this.topics.update(items => items.filter(item => item !== topic)); }
}
