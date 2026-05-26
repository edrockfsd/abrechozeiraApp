import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface ProdutoEstoque {
  id: number;
  produtoId: string;
  codigoEstoque: string;
  descricao: string;
  precoVenda: number;
  quantidadeDisponivel: number;
  localizacao: string;
}

@Injectable({
  providedIn: 'root'
})
export class EstoqueService {
  private apiUrl = `${environment.apiUrl}/Estoque`;

  constructor(private http: HttpClient) { }

  buscarPorCodigo(codigoEstoque: string): Observable<ProdutoEstoque> {
    return this.http.get<ProdutoEstoque>(`${this.apiUrl}/GetEstoqueByCodigoEstoque?codigoEstoque=${codigoEstoque}`);
  }
} 