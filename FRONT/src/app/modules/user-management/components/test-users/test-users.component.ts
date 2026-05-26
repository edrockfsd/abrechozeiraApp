import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-test-users',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="test-container">
      <h2>Teste de Dados de Usuǭrios</h2>
      <button (click)="loadUsers()">Carregar Usuǭrios</button>
      <button (click)="clearData()">Limpar Dados</button>
      
      <div *ngIf="loading" class="loading">
        Carregando...
      </div>
      
      <div *ngIf="error" class="error">
        <h3>Erro:</h3>
        <pre>{{ error }}</pre>
      </div>
      
      <div *ngIf="users.length > 0" class="users-data">
        <h3>Total de usuǭrios: {{ users.length }}</h3>
        <div *ngFor="let user of users; let i = index" class="user-card">
          <h4>Usuǭrio {{ i + 1 }} (ID: {{ user.id }})</h4>
          <div class="user-fields">
            <div><strong>Username:</strong> {{ user.username || 'Vazio' }} ({{ user.username === '' ? 'string vazia' : typeof user.username }})</div>
            <div><strong>Email:</strong> {{ user.email || 'Vazio' }} ({{ user.email === '' ? 'string vazia' : typeof user.email }})</div>
            <div><strong>FirstName:</strong> {{ user.firstName || 'Vazio' }} ({{ user.firstName === '' ? 'string vazia' : typeof user.firstName }})</div>
            <div><strong>LastName:</strong> {{ user.lastName || 'Vazio' }} ({{ user.lastName === '' ? 'string vazia' : typeof user.lastName }})</div>
            <div><strong>IsActive:</strong> {{ user.isActive }} ({{ typeof user.isActive }})</div>
          </div>
          <div class="raw-data">
            <strong>Dados brutos:</strong>
            <pre>{{ user }}</pre>
          </div>
        </div>
      </div>
      
      <div *ngIf="rawResponse" class="raw-response">
        <h3>Resposta bruta do backend:</h3>
        <pre>{{ rawResponse }}</pre>
      </div>
    </div>
  `,
  styles: [`
    .test-container {
      padding: 20px;
      background: #f5f5f5;
      min-height: 100vh;
    }
    
    .loading {
      background: #fff3cd;
      padding: 10px;
      margin: 10px 0;
      border-radius: 4px;
    }
    
    .error {
      background: #f8d7da;
      padding: 10px;
      margin: 10px 0;
      border-radius: 4px;
    }
    
    .user-card {
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 15px;
      margin: 10px 0;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    
    .user-fields {
      margin: 10px 0;
    }
    
    .user-fields div {
      margin: 5px 0;
      padding: 5px;
      background: #f8f9fa;
      border-radius: 3px;
    }
    
    .raw-data, .raw-response {
      margin-top: 15px;
      padding: 10px;
      background: #f8f9fa;
      border-left: 4px solid #007bff;
      border-radius: 4px;
    }
    
    pre {
      margin: 0;
      white-space: pre-wrap;
      word-break: break-all;
      font-size: 12px;
    }
    
    button {
      margin: 5px;
      padding: 10px 20px;
      background: #007bff;
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
    }
    
    button:hover {
      background: #0056b3;
    }
  `]
})
export class TestUsersComponent implements OnInit {
  users: any[] = [];
  loading = false;
  error: any = null;
  rawResponse: any = null;

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.error = null;
    
    this.userService.getUsers().subscribe({
      next: (data) => {
        this.users = data;
        this.rawResponse = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err;
        this.loading = false;
      }
    });
  }

  clearData(): void {
    this.users = [];
    this.error = null;
    this.rawResponse = null;
  }
}

