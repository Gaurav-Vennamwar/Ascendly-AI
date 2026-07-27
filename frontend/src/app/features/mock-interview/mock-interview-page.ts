import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WorkspaceLayout } from '../../shared/components/workspace-layout/workspace-layout';
import { PrimaryButton } from '../../shared/components/primary-button/primary-button';

@Component({
  selector: 'app-mock-interview-page', standalone: true,
  imports: [WorkspaceLayout, PrimaryButton, FormsModule],
  templateUrl: './mock-interview-page.html', styleUrl: './mock-interview-page.scss'
})
export class MockInterviewPage {
  readonly interviewTypes = ['Technical', 'HR', 'Behavioral', 'Mixed'];
  readonly roles = ['Senior .NET Developer', 'Angular Developer', 'Backend Engineer', 'Cloud Engineer', 'Full Stack Developer'];
  readonly difficulties = ['Beginner', 'Intermediate', 'Advanced'];
  readonly durations = [10, 20, 30, 45, 60];
  readonly styles = ['Friendly', 'Professional', 'Strict', 'Startup', 'Enterprise', 'FAANG Style', 'Founder Mode'];
  readonly personas = ['Technical Interviewer', 'Engineering Manager', 'HR Recruiter', 'Tech Lead', 'CTO', 'Founder', 'Product Manager', 'Recruiter'];
  selectedType = signal('Technical'); selectedRole = signal('Senior .NET Developer'); selectedDifficulty = signal('Intermediate'); selectedDuration = signal(20); selectedStyle = signal('Professional'); selectedPersona = signal('Technical Interviewer');
  topics = signal(['ASP.NET Core', 'Angular', 'Entity Framework', 'SQL', 'REST APIs']);
  topicInput = '';
  addTopic(): void { const topic = this.topicInput.trim(); if (topic && !this.topics().includes(topic)) this.topics.update(items => [...items, topic]); this.topicInput = ''; }
  removeTopic(topic: string): void { this.topics.update(items => items.filter(item => item !== topic)); }
}
