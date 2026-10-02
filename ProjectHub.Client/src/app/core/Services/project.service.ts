import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { CreateProjectRequest, ProjectDto } from '../models/project.models';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/projects`;

  // Estado reactivo con Signals
  private projectsSignal = signal<ProjectDto[]>([]);
  readonly projects = this.projectsSignal.asReadonly();
  isLoading = signal(false);

  loadProjects(): Observable<ProjectDto[]> {
    this.isLoading.set(true);
    return this.http.get<ProjectDto[]>(this.apiUrl).pipe(
      tap({
        next: (data) => {
          this.projectsSignal.set(data);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      })
    );
  }

  createProject(request: CreateProjectRequest): Observable<ProjectDto> {
    return this.http.post<ProjectDto>(this.apiUrl, request).pipe(
      tap((newProject) => {
        this.projectsSignal.update((projects) => [newProject, ...projects]);
      })
    );
  }
}