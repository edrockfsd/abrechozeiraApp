import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-simple-test',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  template: `
    <div style="padding: 20px;">
      <h2>Teste Simples de API</h2>
      
      <button (click)="testHttp()" [disabled]="loading" style="margin: 5px;">
        {{ loading ? 'Testando...' : 'Testar HTTP' }}
      </button>
      
      <button (click)="testHttps()" [disabled]="loading" style="margin: 5px;">
        {{ loading ? 'Testando...' : 'Testar HTTPS' }}
      </button>
      
      <button (click)="clearResults()" style="margin: 5px;">
        Limpar
      </button>
      
      <div *ngIf="loading" style="color: blue; margin: 10px 0;">
        Carregando...
      </div>
      
      <div *ngIf="result" style="margin-top: 20px;">
        <h3>Resultado:</h3>
        <pre style="background: #f5f5f5; padding: 10px; border: 1px solid #ddd; overflow-x: auto;">{{ result | json }}</pre>
      </div>
      
      <div *ngIf="error" style="color: red; background: #ffeeee; padding: 10px; margin: 10px 0; border: 1px solid #ffcccc;">
        <strong>Erro:</strong> {{ error }}
      </div>
    </div>
  `
})
export class SimpleTestComponent {
  loading = false;
  error: string | null = null;
  result: any = null;
  
  constructor(private http: HttpClient) {}
  
  testHttp() {
    this.loading = true;
    this.error = null;
    this.result = null;
    
    // Testar com HTTP sem HTTPS
    this.http.get('http://localhost:7194/api/users').subscribe({
      next: (data) => {
        this.result = { success: true, data: data, protocol: 'HTTP' };
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message;
        this.loading = false;
      }
    });
  }
  
  testHttps() {
    this.loading = true;
    this.error = null;
    this.result = null;
    
    // Testar com HTTPS
    this.http.get('https://localhost:7194/api/users').subscribe({
      next: (data) => {
        this.result = { success: true, data: data, protocol: 'HTTPS' };
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message;
        this.loading = false;
      }
    });
  }
  
  clearResults() {
    this.error = null;
    this.result = null;
  }
}