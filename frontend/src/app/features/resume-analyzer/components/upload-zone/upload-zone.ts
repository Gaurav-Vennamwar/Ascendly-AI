import { Component, input, output } from '@angular/core';
import { PrimaryButton } from '../../../../shared/components/primary-button/primary-button';

@Component({
  selector: 'app-upload-zone',
  standalone: true,
  imports: [PrimaryButton],
  templateUrl: './upload-zone.html',
  styleUrl: './upload-zone.scss',
})
export class UploadZone {
  locked = input(false);
  fileSelected = output<File>();

  onFileSelected(event: Event): void {
    if (this.locked()) {
      return;
    }
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) {
      return;
    }

    // Send the selected PDF to ResumeUploadCard.
    this.fileSelected.emit(file);

    console.log('PDF selected:', file.name);
  }
}
