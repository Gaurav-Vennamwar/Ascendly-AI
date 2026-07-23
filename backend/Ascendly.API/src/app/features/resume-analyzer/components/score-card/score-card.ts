import { Component, input } from '@angular/core';

@Component({ selector: 'app-score-card', standalone: true, templateUrl: './score-card.html', styleUrl: './score-card.scss' })
export class ScoreCard { title=input.required<string>(); detail=input.required<string>(); tone=input<'cyan'|'purple'|'rose'>('purple'); }
