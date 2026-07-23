import { Routes } from '@angular/router';
import { Landing } from './pages/landing/landing';
import { ResumeAnalyzerPage } from './features/resume-analyzer/resume-analyzer-page';
import { DashboardPage } from './features/dashboard/dashboard-page';
import { MockInterviewPage } from './features/mock-interview/mock-interview-page';
import { LearningRoadmapPage } from './features/learning-roadmap/learning-roadmap-page';
import { ProfilePage } from './features/profile/profile-page';
import { LoginPage } from './features/auth/login-page';
import { RegisterPage } from './features/auth/register-page';

export const routes: Routes = [
  {
    path: '',
    component: Landing
  },
  {
    path: 'resume-analyzer',
    component: ResumeAnalyzerPage
  },
  { path: 'dashboard', component: DashboardPage },
  { path: 'mock-interview', component: MockInterviewPage },
  { path: 'learning-roadmap', component: LearningRoadmapPage },
  { path: 'profile', component: ProfilePage },
  { path: 'login', component: LoginPage },
  { path: 'register', component: RegisterPage },
  {
    path: '**',
    redirectTo: ''
  }
];
