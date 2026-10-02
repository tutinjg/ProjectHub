import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ProjectService } from '../../../../core/services/project.service';

@Component({
  selector: 'app-project-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './project-dialog.html',
  styleUrl: './project-dialog.scss'
})
export class ProjectDialog {
  private fb = inject(FormBuilder);
  private projectService = inject(ProjectService);
  private dialogRef = inject(MatDialogRef<ProjectDialog>);

  isSubmitting = signal(false);
  errorMessage = signal<string | null>(null);

  form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    description: ['', [Validators.maxLength(1000)]],
    startDate: [null as Date | null],
    targetEndDate: [null as Date | null]
  });

  onSubmit(): void {
    if (this.form.invalid) return;

    const values = this.form.value;
    if (values.startDate && values.targetEndDate && values.targetEndDate < values.startDate) {
      this.errorMessage.set('La fecha final debe ser igual o posterior a la fecha inicial.');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.projectService.createProject({
      name: values.name!,
      description: values.description || undefined,
      startDate: values.startDate ? values.startDate.toISOString() : undefined,
      targetEndDate: values.targetEndDate ? values.targetEndDate.toISOString() : undefined
    }).subscribe({
      next: (created) => {
        this.isSubmitting.set(false);
        this.dialogRef.close(created);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.detail || 'Error al crear el proyecto.');
      }
    });
  }
}