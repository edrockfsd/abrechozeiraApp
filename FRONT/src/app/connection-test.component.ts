import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-connection-test',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  template: `
    <div style="padding: 20px;">
      <h2>Teste de Conexão com Backend</h2>
      
      <button (click)="testConnection()" [disabled]="loading" style="margin: 5px;">
        {{ loading ? 'Testando...' : 'Testar Conexão' }}
      </button>
      
      <button (click)="clearResults()" style="margin: 5px;">
        Limpar Resultados
      </button>
      
      <div *ngIf="loading" style="color: blue; margin: 10px 0;">
        Testando conexão com o backend...
      </div>
      
      <div *ngIf="error" style="color: red; background: #ffeeee; padding: 10px; margin: 10px 0; border: 1px solid #ffcccc;">
        <strong>❌ Erro:</strong> {{ error }}
      </div>
      
      <div *ngIf="success" style="color: green; background: #eeffee; padding: 10px; margin: 10px 0; border: 1px solid #ccffcc;">
        <strong>✅ Sucesso!</strong> Conexão estabelecida com sucesso!
      </div>
      
      <div *ngIf="response" style="margin-top: 20px;">
        <h3>Resposta do Servidor:</h3>
        <pre style="background: #f5f5f5; padding: 10px; border: 1px solid #ddd; overflow-x: auto;">{{ response | json }}</pre>
      </div>
      
      <div *ngIf="details" style="margin-top: 20px;">
        <h3>Detalhes da Requisição:</h3>
        <div style="background: #f5f5f5; padding: 10px; border: 1px solid #ddd;">
          <p><strong>URL:</strong> {{ details.url }}</p>
          <p><strong>Status:</strong> {{ details.status }}</p>
          <p><strong>Tempo de resposta:</strong> {{ details.time }}ms</p>
        </div>
      </div>
    </div>
  `
})
export class ConnectionTestComponent {
  loading = false;
  error: string | null = null;
  success = false;
  response: any = null;
  details: any = null;
  
  constructor(private http: HttpClient) {}
  
  testConnection() {
    this.loading = true;
    this.error = null;
    this.success = false;
    this.response = null;
    this.details = null;
    
    const startTime = Date.now();
    const testUrl = 'https://localhost:7194/api/users';
    
    this.http.get(testUrl, { observe: 'response' }).subscribe({
      next: (response) => {
        const endTime = Date.now();
        this.success = true;
        this.response = response.body;
        this.details = {
          url: testUrl,
          status: response.status,
          statusText: response.statusText,
          time: endTime - startTime
        };
        this.loading = false;
      },
      error: (error) => {
        const endTime = Date.now();
        this.error = `Erro ${error.status}: ${error.message}`;
        this.details = {
          url: testUrl,
          status: error.status,
          statusText: error.statusText,
          time: endTime - startTime
        };
        this.loading = false;
      }
    });
  }
  
  clearResults() {
    this.error = null;
    this.success = false;
    this.response = null;
    this.details = null;
  }
}