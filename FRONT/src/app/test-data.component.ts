import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from './modules/user-management/services/user.service';

@Component({
  selector: 'app-test-data',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div style="padding: 20px;">
      <h2>Teste de Dados - Debug</h2>
      
      <div *ngIf="loading">Carregando...</div>
      <div *ngIf="error" style="color: red;">Erro: {{ error }}</div>
      
      <div *ngIf="!loading && !error">
        <h3>Total de usuários: {{ users.length }}</h3>
        
        <div *ngIf="users.length > 0" style="margin-top: 20px;">
          <h4>Primeiro usuário:</h4>
          <pre>{{ firstUserDebug | json }}</pre>
          
          <h4>Propriedades do primeiro usuário:</h4>
          <ul>
            <li>ID: {{ users[0].id }}</li>
            <li>Username: {{ users[0].username }}</li>
            <li>Email: {{ users[0].email }}</li>
            <li>FirstName: {{ users[0].firstName }}</li>
            <li>LastName: {{ users[0].lastName }}</li>
            <li>IsActive: {{ users[0].isActive }}</li>
            <li>UserRoles: {{ users[0].userRoles | json }}</li>
          </ul>
        </div>
      </div>
    </div>
  `
})
export class TestDataComponent implements OnInit {
  users: any[] = [];
  loading = false;
  error: string | null = null;
  firstUserDebug: any = null;

  constructor(private userService: UserService) {}

  ngOnInit() {
    console.log('=== TEST DATA COMPONENT INICIADO ===');
    console.log('Serviço UserService disponível:', !!this.userService);
    console.log('Método getUsers disponível:', typeof this.userService.getUsers === 'function');
    
    // Teste simples para ver se o console funciona
    console.log('Console.log está funcionando!');
    window.alert('Componente de teste carregado! Verifique o console do navegador (F12)');
    
    this.loadUsers();
  }

  loadUsers() {
    this.loading = true;
    console.log('=== INICIANDO CARREGAMENTO DE USUÁRIOS ===');
    console.log('Timestamp:', new Date().toISOString());
    
    this.userService.getUsers().subscribe({
      next: (data) => {
        console.log('=== DADOS BRUTOS RECEBIDOS ===');
        console.log('Total de usuários:', data.length);
        console.log('Tipo dos dados:', typeof data);
        console.log('Dados completos:', JSON.stringify(data, null, 2));
        
        if (data.length > 0) {
          console.log('=== ANÁLISE DO PRIMEIRO USUÁRIO ===');
          console.log('Primeiro usuário completo:', data[0]);
          console.log('Todas as chaves do primeiro usuário:', Object.keys(data[0]));
          console.log('Propriedades do primeiro usuário:');
          console.log('- id:', data[0].id, '(tipo:', typeof data[0].id + ')');
          console.log('- username:', data[0].username, '(tipo:', typeof data[0].username + ')');
          console.log('- email:', data[0].email, '(tipo:', typeof data[0].email + ')');
          console.log('- firstName:', data[0].firstName, '(tipo:', typeof data[0].firstName + ')');
          console.log('- lastName:', data[0].lastName, '(tipo:', typeof data[0].lastName + ')');
          console.log('- isActive:', data[0].isActive, '(tipo:', typeof data[0].isActive + ')');
          console.log('- userRoles:', data[0].userRoles, '(tipo:', typeof data[0].userRoles + ')');
          
          this.firstUserDebug = {
            ...data[0],
            propertyTypes: {
              id: typeof data[0].id,
              username: typeof data[0].username,
              email: typeof data[0].email,
              firstName: typeof data[0].firstName,
              lastName: typeof data[0].lastName,
              isActive: typeof data[0].isActive,
              userRoles: typeof data[0].userRoles
            }
          };
        }
        
        this.users = data;
        this.loading = false;
        console.log('=== CARREGAMENTO CONCLUÍDO COM SUCESSO ===');
      },
      error: (err) => {
        console.error('=== ERRO AO CARREGAR USUÁRIOS ===');
        console.error('Erro completo:', err);
        console.error('Mensagem do erro:', err.message);
        console.error('Status do erro:', err.status);
        console.error('URL da requisição:', err.url);
        this.error = err.message;
        this.loading = false;
      }
    });
  }
}