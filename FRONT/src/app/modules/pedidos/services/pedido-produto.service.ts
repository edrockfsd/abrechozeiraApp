import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface PedidoProduto {
  id?: number;
  pedidoId: number;
  produtoId: number;
  descricao: string;
  quantidade: number;
  descontoValor: number;
  valorFinalProduto: number;
  dataAlteracao?: Date;
  usuarioModificacaoId: number;
}

@Injectable({
  providedIn: 'root'
})
export class PedidoProdutoService {
  private apiUrl = `${environment.apiUrl}/PedidoProduto`;

  constructor(private http: HttpClient) { }

  adicionar(produto: PedidoProduto): Observable<PedidoProduto> {
    console.log('Adicionando produto:', produto);
    return this.http.post<PedidoProduto>(this.apiUrl, produto);
  }

  listarPorPedido(pedidoId: number): Observable<PedidoProduto[]> {
    return this.http.get<PedidoProduto[]>(`${environment.apiUrl}/PedidoProduto/GetDadosPedidoProdutoGrid?pedidoId=${pedidoId}`);
  }

  excluir(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}