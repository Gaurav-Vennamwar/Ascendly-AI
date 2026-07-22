import { Routes } from '@angular/router';
import { Landing } from './pages/landing/landing';
import { ResumeAnalyzerPage } from './features/resume-analyzer/resume-analyzer-page';

export const routes: Routes = [
  {
    path: '',
    component: Landing
  },
  {
    path: 'resume-analyzer',
    component: ResumeAnalyzerPage
  },
  {
    path: '**',
    redirectTo: ''
  }
];
