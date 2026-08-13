import { Routes } from '@angular/router';
import { Landing } from './pages/landing/landing';
import { ResumeAnalyzerPage } from './features/resume-analyzer/resume-analyzer-page';
import { DashboardPage } from './features/dashboard/dashboard-page';
import { MockInterviewPage } from './features/mock-interview/mock-interview-page';
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
    component: ResumeAnalyzerPage
  },
  { path: 'dashboard', component: DashboardPage, canActivate: [authGuard] },
  { path: 'mock-interview', component: MockInterviewPage, canActivate: [authGuard]},
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
