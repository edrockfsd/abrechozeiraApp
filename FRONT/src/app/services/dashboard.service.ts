import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface VendaDia {
  data: string;
  diaSemana: string;
  total: number;
}

export interface ProdutoMaisVendido {
  produtoId: number;
  descricao: string;
  quantidade: number;
  totalVendido: number;
}

export interface EstoqueBaixo {
  id: number;
  produtoId: number;
  descricao: string;
  quantidade: number;
  codigoEstoque: number;
}

export interface ProximaLive {
  titulo: string;
  data: string;
}

export interface DashboardData {
  vendasHoje: number;
  vendasSemana: number;
  vendasMes: number;
  vendasHojeCount: number;
  vendasUltimos7Dias: VendaDia[];
  produtosMaisVendidos: ProdutoMaisVendido[];
  estoqueBaixo: EstoqueBaixo[];
  proximaLive: ProximaLive | null;
  totalProdutos: number;
  totalClientes: number;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) { }

  getDashboard(): Observable<DashboardData> {
    return this.http.get<DashboardData>(this.apiUrl);
  }
}
