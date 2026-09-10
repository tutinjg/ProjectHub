import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment.development';

export interface CreateCompanyRequest {
  name: string;
  information?: string;
}

@Injectable({ providedIn: 'root' })
export class CompanyService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/companies`;

  isLoading = signal(false);

  createCompany(request: CreateCompanyRequest) {
    return this.http.post<string>(this.apiUrl, request);
  }
}