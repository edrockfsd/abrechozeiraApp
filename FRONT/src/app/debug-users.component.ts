import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-debug-users',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div style="padding: 20px; font-family: Arial, sans-serif;">
      <h2>Debug de Usuários - Teste Direto</h2>
      
      <div style="margin-bottom: 20px;">
        <button (click)="testDirectApi()" style="padding: 10px; margin-right: 10px;">Testar API Direto</button>
        <button (click)="clearLogs()" style="padding: 10px;">Limpar Logs</button>
      </div>
      
      <div *ngIf="loading" style="color: blue;">Carregando...</div>
      <div *ngIf="error" class="error-message">
        <strong>Erro:</strong> {{ error }}
      </div>
      
      <div *ngIf="logs.length > 0" class="logs-container">
        <h3>Logs de Execução:</h3>
        <div style="max-height: 400px; overflow-y: auto;">
          <div *ngFor="let log of logs" class="log-item">
            <span style="color: #666;">{{ log.timestamp }}</span> - 
            <span [style.color]="log.color">{{ log.message }}</span>
          </div>
        </div>
      </div>
      
      <div *ngIf="success" style="color: green; background: #eeffee; padding: 10px; margin: 10px 0;">
        <strong>Sucesso!</strong> Dados carregados.
      </div>
      
      <div *ngIf="users.length > 0" style="margin-top: 20px;">
        <h3>Dados dos Usuários:</h3>
        <p><strong>Total de usuários:</strong> {{ users.length }}</p>
        
        <div *ngIf="users.length > 0" style="background: #f9f9f9; padding: 15px; border: 1px solid #ddd; margin-top: 10px;">
          <h4>Primeiro usuário:</h4>
          <pre style="background: white; padding: 10px; border: 1px solid #ccc; overflow-x: auto;">{{ users[0] | json }}</pre>
          
          <h4>Análise das propriedades:</h4>
          <ul>
            <li><strong>ID:</strong> {{ users[0].id }} (tipo: {{ getPropertyType(users[0], 'id') }})</li>
            <li><strong>Username:</strong> {{ users[0].username }} (tipo: {{ getPropertyType(users[0], 'username') }})</li>
            <li><strong>Email:</strong> {{ users[0].email }} (tipo: {{ getPropertyType(users[0], 'email') }})</li>
            <li><strong>FirstName:</strong> {{ users[0].firstName }} (tipo: {{ getPropertyType(users[0], 'firstName') }})</li>
            <li><strong>LastName:</strong> {{ users[0].lastName }} (tipo: {{ getPropertyType(users[0], 'lastName') }})</li>
            <li><strong>IsActive:</strong> {{ users[0].isActive }} (tipo: {{ getPropertyType(users[0], 'isActive') }})</li>
            <li><strong>UserRoles:</strong> {{ users[0].userRoles | json }}</li>
          </ul>
          
          <h4>Todas as chaves do objeto:</h4>
          <code>{{ getObjectKeys(users[0]) }}</code>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .error-message {
      color: red;
      background: #ffeeee;
      padding: 10px;
      margin: 10px 0;
      border: 1px solid #ffcccc;
    }
    .logs-container {
      margin-top: 20px;
      background: #f5f5f5;
      padding: 15px;
      border: 1px solid #ddd;
      border-radius: 4px;
    }
    .log-item {
      margin: 5px 0;
      font-family: monospace;
      font-size: 12px;
      padding: 5px;
      background: white;
      border-left: 3px solid #007bff;
    }
  `]
})
export class DebugUsersComponent implements OnInit {
  users: any[] = [];
  loading = false;
  error: string | null = null;
  success = false;
  apiUrl = 'https://localhost:7194/api/users';
  logs: Array<{timestamp: string, message: string, color: string}> = [];

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.addLog('Componente de debug iniciado', 'blue');
    this.addLog('HttpClient disponível: ' + (!!this.http), 'blue');
  }

  testDirectApi() {
    this.loading = true;
    this.error = null;
    this.users = [];
    this.logs = [];
    
    this.addLog(`Iniciando requisição para: ${this.apiUrl}`, 'blue');
    
    this.http.get<any[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.addLog('✅ Dados recebidos com sucesso!', 'green');
        this.addLog(`📊 Total de usuários: ${data.length}`, 'blue');
        
        if (data.length > 0) {
          this.addLog(`👤 Primeiro usuário: ${JSON.stringify(data[0], null, 2)}`, 'blue');
        }
        
        this.users = data;
        this.loading = false;
      },
      error: (error) => {
        this.addLog('❌ Erro na requisição!', 'red');
        this.addLog(`📄 Status: ${error.status}`, 'red');
        this.addLog(`📄 Mensagem: ${error.message}`, 'red');
        this.addLog(`📄 URL: ${error.url || 'N/A'}`, 'red');
        this.addLog(`📄 Erro completo: ${JSON.stringify(error, null, 2)}`, 'red');
        
        this.error = `Erro ${error.status}: ${error.message}`;
        this.loading = false;
      }
    });
  }

  clearLogs() {
    this.logs = [];
    this.addLog('Logs limpos', 'gray');
  }

  addLog(message: string, color: string = 'black') {
    const timestamp = new Date().toLocaleTimeString();
    this.logs.push({ timestamp, message, color });
  }

  getPropertyType(obj: any, prop: string): string {
    return typeof obj[prop];
  }

  getObjectKeys(obj: any): string {
    return Object.keys(obj).join(', ');
  }
}