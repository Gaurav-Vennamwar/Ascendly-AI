import { Component } from '@angular/core';
import { ScoreCard } from '../score-card/score-card';

@Component({ selector: 'app-analysis-dashboard', standalone: true, imports: [ScoreCard], templateUrl: './analysis-dashboard.html', styleUrl: './analysis-dashboard.scss' })
export class AnalysisDashboard {}
