import { Component, input, output, signal } from '@angular/core';
import { UploadZone } from '../upload-zone/upload-zone';
import { DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-resume-upload-card',
  standalone: true,
  imports: [UploadZone, DecimalPipe],
  templateUrl: './resume-upload-card.html',
  styleUrl: './resume-upload-card.scss',
})
export class ResumeUploadCard {
  locked = input(false);
  selectedFile = signal<File | null>(null);
  uploadProgress = signal(0);

  // Sends the PDF to the parent page.
  resumeSelected = output<File>();

  onFileSelected(file: File): void {
    console.log('ResumeUploadCard received:', file.name);

    this.selectedFile.set(file);
    this.uploadProgress.set(100);

    this.resumeSelected.emit(file);
  }
}
