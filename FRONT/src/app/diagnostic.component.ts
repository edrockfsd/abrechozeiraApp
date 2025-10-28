import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule, HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-diagnostic',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  template: `
    <div style="padding: 20px; font-family: Arial, sans-serif;">
      <h2>Diagnóstico de Dados de Usuário</h2>
      
      <div style="background: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; margin: 10px 0; border-radius: 4px;">
        <strong>📋 Status:</strong> {{ diagnosticStatus }}
      </div>
      
      <div style="margin: 20px 0;">
        <button (click)="runDiagnostic()" [disabled]="loading" style="background: #007bff; color: white; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; margin-right: 10px;">
          {{ loading ? 'Executando...' : 'Executar Diagnóstico' }}
        </button>
        
        <button (click)="clearResults()" style="background: #6c757d; color: white; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer;">
          Limpar Resultados
        </button>
      </div>
      
      <div *ngIf="loading" style="text-align: center; padding: 20px;">
        <div style="border: 4px solid #f3f3f3; border-top: 4px solid #007bff; border-radius: 50%; width: 40px; height: 40px; animation: spin 1s linear infinite; margin: 0 auto;"></div>
        <p>Executando diagnóstico...</p>
      </div>
      
      <div *ngIf="results.length > 0" style="margin-top: 20px;">
        <h3>Resultados do Diagnóstico:</h3>
        <div *ngFor="let result of results" style="margin: 10px 0; padding: 15px; border-radius: 4px;" [ngStyle]="{'background-color': getResultColor(result.type), 'border': '1px solid ' + getBorderColor(result.type)}">
          <div style="font-weight: bold; margin-bottom: 5px;">
            <span *ngIf="result.type === 'success'">✅</span>
            <span *ngIf="result.type === 'error'">❌</span>
            <span *ngIf="result.type === 'info'">ℹ️</span>
            <span *ngIf="result.type === 'warning'">⚠️</span>
            {{ result.title }}
          </div>
          <div style="font-size: 14px; margin-left: 25px;">{{ result.message }}</div>
          <div *ngIf="result.data" style="font-size: 12px; margin-left: 25px; margin-top: 5px; font-family: monospace; background: #f8f9fa; padding: 5px; border-radius: 2px;">
            {{ result.data }}
          </div>
        </div>
      </div>
      
      <div *ngIf="userData" style="margin-top: 20px; background: #d4edda; border: 1px solid #c3e6cb; padding: 15px; border-radius: 4px;">
        <h3>📊 Dados de Usuário Encontrados:</h3>
        <p><strong>Total de usuários:</strong> {{ userData.length }}</p>
        <div *ngIf="userData.length > 0">
          <p><strong>Primeiro usuário:</strong></p>
          <pre style="background: white; padding: 10px; border-radius: 4px; overflow-x: auto; font-size: 12px;">{{ userData[0] | json }}</pre>
        </div>
      </div>
      
      <div *ngIf="errorDetails" style="margin-top: 20px; background: #f8d7da; border: 1px solid #f5c6cb; padding: 15px; border-radius: 4px;">
        <h3>🔍 Detalhes do Erro:</h3>
        <pre style="background: white; padding: 10px; border-radius: 4px; overflow-x: auto; font-size: 12px;">{{ errorDetails | json }}</pre>
      </div>
    </div>
    
    <style>
      @keyframes spin {
        0% { transform: rotate(0deg); }
        100% { transform: rotate(360deg); }
      }
    </style>
  `
})
export class DiagnosticComponent {
  loading = false;
  results: Array<{type: string, title: string, message: string, data?: string}> = [];
  userData: any[] | null = null;
  errorDetails: any = null;
  diagnosticStatus = 'Aguardando execução do diagnóstico...';
  
  constructor(private http: HttpClient) {}
  
  addResult(type: string, title: string, message: string, data?: any) {
    this.results.push({
      type,
      title,
      message,
      data: data ? JSON.stringify(data, null, 2) : undefined
    });
  }
  
  getResultColor(type: string): string {
    switch (type) {
      case 'success': return '#d4edda';
      case 'error': return '#f8d7da';
      case 'warning': return '#fff3cd';
      case 'info': return '#d1ecf1';
      default: return '#f8f9fa';
    }
  }
  
  getBorderColor(type: string): string {
    switch (type) {
      case 'success': return '#c3e6cb';
      case 'error': return '#f5c6cb';
      case 'warning': return '#ffeaa7';
      case 'info': return '#bee5eb';
      default: return '#dee2e6';
    }
  }
  
  runDiagnostic() {
    this.loading = true;
    this.results = [];
    this.userData = null;
    this.errorDetails = null;
    this.diagnosticStatus = 'Iniciando diagnóstico...';
    
    // Teste 1: Verificar se o serviço está configurado
    this.diagnosticStatus = 'Verificando configuração do serviço...';
    this.addResult('info', 'Configuração do HTTP Client', 'Verificando se HttpClient está configurado corretamente');
    
    // Teste 2: Tentar conectar ao backend
    this.diagnosticStatus = 'Tentando conectar ao backend...';
    const apiUrl = 'https://localhost:7194/api/users';
    
    this.addResult('info', 'URL da API', `Tentando acessar: ${apiUrl}`);
    
    this.http.get<any[]>(apiUrl).subscribe({
      next: (data) => {
        this.addResult('success', 'Conexão Estabelecida', 'Conexão com o backend realizada com sucesso');
        this.addResult('info', 'Dados Recebidos', `Total de usuários encontrados: ${data.length}`);
        
        if (data.length > 0) {
          this.addResult('success', 'Usuários Encontrados', 'Dados de usuários recuperados com sucesso');
          this.addResult('info', 'Primeiro Usuário', 'Detalhes do primeiro usuário no sistema', data[0]);
          
          // Verificar estrutura dos dados
          const firstUser = data[0];
          const properties = Object.keys(firstUser);
          this.addResult('info', 'Propriedades do Usuário', `Propriedades encontradas: ${properties.join(', ')}`);
          
          // Verificar roles
          if (firstUser.userRoles) {
            this.addResult('info', 'Roles do Usuário', `Número de roles: ${firstUser.userRoles.length}`);
          } else {
            this.addResult('warning', 'Roles não encontradas', 'O usuário não possui roles ou a propriedade está com nome diferente');
          }
        } else {
          this.addResult('warning', 'Nenhum Usuário Encontrado', 'A API retornou um array vazio de usuários');
        }
        
        this.userData = data;
        this.diagnosticStatus = 'Diagnóstico concluído com sucesso!';
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.addResult('error', 'Erro na Conexão', 'Não foi possível conectar ao backend');
        this.addResult('error', 'Mensagem de Erro', error.message);
        this.addResult('error', 'Status HTTP', `Código: ${error.status}`);
        this.addResult('error', 'Status Text', error.statusText);
        this.addResult('error', 'URL da Requisição', error.url || 'Não disponível');
        
        if (error.status === 0) {
          this.addResult('warning', 'Possível Causa', 'Erro de CORS ou servidor não está rodando');
          this.addResult('info', 'Solução Sugerida', 'Verifique se o backend está rodando e se as configurações de CORS estão corretas');
        } else if (error.status === 404) {
          this.addResult('warning', 'Possível Causa', 'Endpoint não encontrado');
          this.addResult('info', 'Solução Sugerida', 'Verifique se a URL da API está correta');
        } else if (error.status >= 500) {
          this.addResult('warning', 'Possível Causa', 'Erro no servidor');
          this.addResult('info', 'Solução Sugerida', 'Verifique os logs do servidor backend');
        }
        
        this.errorDetails = {
          message: error.message,
          status: error.status,
          statusText: error.statusText,
          url: error.url,
          error: error.error
        };
        
        this.diagnosticStatus = 'Diagnóstico concluído com erros';
        this.loading = false;
      }
    });
  }
  
  clearResults() {
    this.results = [];
    this.userData = null;
    this.errorDetails = null;
    this.diagnosticStatus = 'Aguardando execução do diagnóstico...';
  }
}