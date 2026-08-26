import { Component, input } from '@angular/core';

@Component({
  selector: 'app-insight-card',
  standalone: true,
  templateUrl: './insight-card.html',
  styleUrl: './insight-card.scss',
})
export class InsightCard {
  completed = input(false);
  isAnalyzing = input(false);
  durationSeconds = input<number | null>(null);
}
