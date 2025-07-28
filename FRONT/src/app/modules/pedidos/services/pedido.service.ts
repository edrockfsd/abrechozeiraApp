import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, switchMap } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface Pedido {
  id?: number;
  pessoaId: number;
  enderecoId: number;
  dataPedido: Date;
  observacao?: string;
  valorTotal: number;
  statusPedidoId: number;
  usuarioModificacaoId: number;
  numeroPedido: string;
  formaPagamento: string;
  parcelas: number;
  valorSubtotal: number;
  valorFrete: number;
  itens: any[];
  dataLancamento?: string;
  DataLancamento?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PedidoService {
  private apiUrl = `${environment.apiUrl}/Pedido`;

  constructor(private http: HttpClient) { }

  criar(pedido: Pedido): Observable<Pedido> {
    console.log('pedido', pedido);
    return this.http.post<Pedido>(this.apiUrl, pedido);
  }

  atualizar(id: number, pedido: Pedido): Observable<Pedido> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, pedido).pipe(
      switchMap(() => this.obterPorId(id))
    );
  }

  obterPorId(id: number): Observable<Pedido> {
    return this.http.get<Pedido>(`${this.apiUrl}/${id}`);
  }

  listar(): Observable<Pedido[]> {
    return this.http.get<Pedido[]>(`${environment.apiUrl}/Pedido/GetListaPedido`);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  gerarNovoPedidoCodigo(): Observable<string> {
    return this.http.get<string>(`${environment.apiUrl}/Pedido/GerarNovoPedidoCodigo`);
  }
}