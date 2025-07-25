import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
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
    console.log('Atualizando pedido:', pedido);
    return this.http.put<Pedido>(`${this.apiUrl}/${id}`, pedido);
  }

  obterPorId(id: number): Observable<Pedido> {
    return this.http.get<Pedido>(`${this.apiUrl}/${id}`);
  }

  listar(): Observable<Pedido[]> {
    return this.http.get<Pedido[]>(this.apiUrl);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  gerarNovoPedidoCodigo(): Observable<string> {
    return this.http.get<string>(`${environment.apiUrl}/Pedido/GerarNovoPedidoCodigo`);
  }
} 