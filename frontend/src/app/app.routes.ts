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
    component: Landing,
    data: {
      title: 'AI Resume Analyzer and Mock Interview Practice',
      description: 'Optimize your developer resume for ATS, prepare with AI-powered mock interviews, and plan your next career move with Ascendly AI.'
    }
  },
  {
    path: 'resume-analyzer',
    component: ResumeAnalyzer,
    data: {
      title: 'AI Resume Analyzer',
      description: 'Upload your resume and job description to review ATS compatibility, keywords, skills gaps, and practical improvements.'
    }
  },
  { path: 'dashboard', component: DashboardPage, canActivate: [authGuard], data: { title: 'Dashboard', robots: 'noindex, nofollow' } },
  { path: 'mock-interview', component: MockInterviewPage, canActivate: [authGuard], data: { title: 'Mock Interview', robots: 'noindex, nofollow' }},
  { path: 'mock-interview/session', component: InterviewSessionPage, canActivate: [authGuard], data: { title: 'Interview Session', robots: 'noindex, nofollow' } },
  { path: 'mock-interview/topic-evaluation', component: TopicEvaluationPage, canActivate: [authGuard], data: { title: 'Topic Evaluation', robots: 'noindex, nofollow' } },
  { path: 'mock-interview/report', component: InterviewReportPage, canActivate: [authGuard], data: { title: 'Interview Report', robots: 'noindex, nofollow' } },
  { path: 'learning-roadmap', component: LearningRoadmapPage, canActivate: [authGuard], data: { title: 'Learning Roadmap', robots: 'noindex, nofollow' } },
  { path: 'profile', component: ProfilePage, canActivate: [authGuard], data: { title: 'Profile', robots: 'noindex, nofollow' } },
  { path: 'login', component: LoginPage, data: { title: 'Log in', robots: 'noindex, nofollow' } },
  { path: 'verify-email', component: VerifyEmailPage, data: { title: 'Verify Email', robots: 'noindex, nofollow' } },
  { path: 'register', component: RegisterPage, data: { title: 'Create an Account', robots: 'noindex, nofollow' } },
  {
    path: '**',
    redirectTo: ''
  }
];
