import { Component, input } from '@angular/core';
import { AnalyzerSidebar } from '../../../features/resume-analyzer/components/sidebar/sidebar';
import { AnalyzerTopbar } from '../../../features/resume-analyzer/components/topbar/topbar';

@Component({ selector: 'app-workspace-layout', standalone: true, imports: [AnalyzerSidebar, AnalyzerTopbar], templateUrl: './workspace-layout.html', styleUrl: './workspace-layout.scss' })
export class WorkspaceLayout {
  title = input.required<string>();
  subtitle = input.required<string>();
  eyebrow = input('Career intelligence workspace');
}
