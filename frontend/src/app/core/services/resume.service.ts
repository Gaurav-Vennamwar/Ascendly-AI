import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ResumeAnalysisResponse } from '../../features/resume-analyzer/models/resume-analysis-response';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ResumeService {
  private readonly apiUrl = `${environment.apiBaseUrl}/Resume`;
  constructor(private http: HttpClient) {}

  analyzeResume(
    resume: File,
    jobDescription: string
  ): Observable<ResumeAnalysisResponse> {

    // FormData is required because the API receives a PDF file.
    const formData = new FormData();

    formData.append('resume', resume);
    formData.append('jobDescription', jobDescription);

    return this.http.post<ResumeAnalysisResponse>(
       `${this.apiUrl}/analyze`,
      formData
    );
  }
}
