import { Component, inject, OnInit, signal } from '@angular/core';
import { CompanyService } from './services/company.service';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-companies',
  standalone: true,
  imports: [MatButtonModule],
  template: `
    <h2>Gestión de Empresas (Tenant)</h2>
    <button mat-raised-button color="primary" (click)="testCreateCompany()">
      Probar Petición con JWT
    </button>
    @if (statusMessage()) {
      <p>{{ statusMessage() }}</p>
    }
  `
})
export class Companies {
  private companyService = inject(CompanyService);
  statusMessage = signal<string | null>(null);

  testCreateCompany() {
    this.companyService.createCompany({
      name: 'Empresa Test ' + Math.floor(Math.random() * 1000),
      information: 'Prueba de autenticación con Interceptor'
    }).subscribe({
      next: (res) => this.statusMessage.set('Empresa creada con éxito ID: ' + res),
      error: (err) => this.statusMessage.set('Error en la petición: ' + err.status)
    });
  }
}