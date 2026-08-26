import { Component, input, output } from '@angular/core';
import { PrimaryButton } from '../../../../shared/components/primary-button/primary-button';
@Component({
  selector: 'app-job-description-card',
  standalone: true,
  imports: [PrimaryButton],
  templateUrl: './job-description-card.html',
  styleUrl: './job-description-card.scss',
})
export class JobDescriptionCard {
  locked = input(false);
  jobDescriptionChanged = output<string>();
  analyzeClicked = output<string>();

  jobDescription = '';

  onJobDescriptionChange(value: string): void {
    this.jobDescription = value;
    this.jobDescriptionChanged.emit(value);
  }

  analyze(): void {
    this.analyzeClicked.emit(this.jobDescription);
  }
}
