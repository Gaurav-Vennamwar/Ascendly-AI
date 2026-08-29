import { Routes } from '@angular/router';
import { Landing } from './pages/landing/landing';
import { ResumeAnalyzer } from './features/resume-analyzer/resume-analyzer-page';
import { DashboardPage } from './features/dashboard/dashboard-page';
import { MockInterviewPage } from './features/mock-interview/mock-interview-page';
import { InterviewSessionPage } from './features/mock-interview/interview-session-page';
import { TopicEvaluationPage } from './features/mock-interview/topic-evaluation-page';
import { InterviewReportPage } from './features/mock-interview/interview-report-page';
import { LearningRoadmapPage } from './features/learning-roadmap/learning-roadmap-page';
import { ProfilePage } from './features/profile/profile-page';
import { LoginPage } from './features/auth/login-page';
import { RegisterPage } from './features/auth/register-page';
import { VerifyEmailPage } from './features/auth/verify-email-page';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    component: Landing
  },
  {
    path: 'resume-analyzer',
    component: ResumeAnalyzer
  },
  { path: 'dashboard', component: DashboardPage, canActivate: [authGuard] },
  { path: 'mock-interview', component: MockInterviewPage, canActivate: [authGuard]},
  { path: 'mock-interview/session', component: InterviewSessionPage, canActivate: [authGuard] },
  { path: 'mock-interview/topic-evaluation', component: TopicEvaluationPage, canActivate: [authGuard] },
  { path: 'mock-interview/report', component: InterviewReportPage, canActivate: [authGuard] },
  { path: 'learning-roadmap', component: LearningRoadmapPage, canActivate: [authGuard] },
  { path: 'profile', component: ProfilePage, canActivate: [authGuard] },
  { path: 'login', component: LoginPage },
  { path: 'verify-email', component: VerifyEmailPage},
  { path: 'register', component: RegisterPage },
  {
    path: '**',
    redirectTo: ''
  }
];
