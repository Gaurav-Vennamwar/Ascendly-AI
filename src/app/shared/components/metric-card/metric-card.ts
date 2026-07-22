import { Component, input } from '@angular/core';

@Component({ selector: 'app-metric-card', standalone: true, templateUrl: './metric-card.html', styleUrl: './metric-card.scss' })
export class MetricCard { label=input.required<string>(); value=input.required<string>(); detail=input.required<string>(); tone=input<'cyan'|'purple'|'mint'>('purple'); }
