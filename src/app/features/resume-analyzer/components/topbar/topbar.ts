import { Component, input } from '@angular/core';

@Component({ selector: 'app-analyzer-topbar', standalone: true, templateUrl: './topbar.html', styleUrl: './topbar.scss' })
export class AnalyzerTopbar {
  title = input('Resume Analyzer');
  subtitle = input('Optimize your resume with AI.');
  eyebrow = input('Career intelligence workspace');
}
