import { Component, input } from '@angular/core';
import { WorkspaceLayout } from '../../shared/components/workspace-layout/workspace-layout';

export interface TopicEvaluation { topic: string; summary: string; strengths: string[]; improvements: string[]; }
@Component({ selector: 'app-topic-evaluation-page', standalone: true, imports: [WorkspaceLayout], templateUrl: './topic-evaluation-page.html', styleUrl: './interview-flow.scss' })
export class TopicEvaluationPage { evaluation = input<TopicEvaluation | null>(null); }
