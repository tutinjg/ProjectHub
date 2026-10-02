import { Component, inject, OnInit, signal, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ProjectService } from '../../../../core/services/project.service';
import { ProjectDto, ProjectStatus } from '../../../../core/models/project.models';
import { ProjectDialog } from '../../components/project-dialog/project-dialog';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    MatTableModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule,
    MatProgressBarModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './project-list.html',
  styleUrl: './project-list.scss'
})
export class ProjectList implements OnInit, AfterViewInit {
  projectService = inject(ProjectService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private router = inject(Router);

  displayedColumns: string[] = ['name', 'status', 'taskCount', 'startDate', 'targetEndDate', 'actions'];
  dataSource = new MatTableDataSource<ProjectDto>([]);

  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void {
    this.fetchProjects();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }

  fetchProjects(): void {
    this.projectService.loadProjects().subscribe({
      next: (data) => {
        this.dataSource.data = data;
      },
      error: () => {
        this.snackBar.open('Error al cargar la lista de proyectos', 'Cerrar', { duration: 4000 });
      }
    });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(ProjectDialog, {
      width: '520px',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((result: ProjectDto | undefined) => {
      if (result) {
        this.dataSource.data = this.projectService.projects();
        this.snackBar.open(`Proyecto "${result.name}" creado con éxito`, 'OK', { duration: 3500 });
      }
    });
  }

  goToBoard(project: ProjectDto): void {
    this.router.navigate(['/projects', project.id, 'board']);
  }

  getStatusLabel(status: ProjectStatus): string {
    switch (status) {
      case ProjectStatus.Planning: return 'Planificación';
      case ProjectStatus.Active: return 'Activo';
      case ProjectStatus.OnHold: return 'En Pausa';
      case ProjectStatus.Completed: return 'Completado';
      case ProjectStatus.Archived: return 'Archivado';
      default: return 'Desconocido';
    }
  }

  getStatusClass(status: ProjectStatus): string {
    switch (status) {
      case ProjectStatus.Planning: return 'status-planning';
      case ProjectStatus.Active: return 'status-active';
      case ProjectStatus.OnHold: return 'status-onhold';
      case ProjectStatus.Completed: return 'status-completed';
      default: return 'status-default';
    }
  }
}