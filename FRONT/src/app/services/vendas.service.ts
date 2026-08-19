import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface VendaListItem {
  id: number;
  pedidoId: number;
  status: string;
  valorBruto: number;
  desconto?: number | null;
  valorTotal: number;
  dataVenda: string;
  dataPagamento?: string | null;
  liveId?: number | null;
  formaPagamentoId?: number | null;
  clienteNome?: string | null;
  clienteNick?: string | null;
  liveTitulo?: string | null;
  formaPagamento?: string | null;
  temNfce: boolean;
  nfceStatus?: string | null;
  nfceChave?: string | null;
  nfceNumero?: number | null;
  nfceProtocolo?: string | null;
  qtdItens: number;
}

export interface VendaFiltros {
  liveId?: number | null;
  status?: string | null;
  inicio?: string | null;
  fim?: string | null;
  clienteId?: number | null;
  limite?: number;
}

export interface EmitirNfceLoteResultado {
  mensagem: string;
  sucessos: number;
  totalErros: number;
  resultados: Array<{
    vendaId: number;
    nfceId: number;
    numero: number;
    chaveAcesso: string;
    status: string;
  }>;
  erros: Array<{
    vendaId: number;
    erro: string;
  }>;
}

@Injectable({
  providedIn: 'root'
})
export class VendasService {
  private apiUrl = `${environment.apiUrl}/Vendas`;

  constructor(private http: HttpClient) {}

  getListagem(filtros?: VendaFiltros): Observable<VendaListItem[]> {
    let params = new HttpParams();

    if (filtros?.liveId) params = params.set('liveId', filtros.liveId.toString());
    if (filtros?.status) params = params.set('status', filtros.status);
    if (filtros?.inicio) params = params.set('inicio', filtros.inicio);
    if (filtros?.fim) params = params.set('fim', filtros.fim);
    if (filtros?.clienteId) params = params.set('clienteId', filtros.clienteId.toString());
    if (filtros?.limite) params = params.set('limite', filtros.limite.toString());

    return this.http.get<VendaListItem[]>(`${this.apiUrl}/listagem`, { params });
  }

  getVenda(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  gerarDePedido(pedidoId: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/gerar-de-pedido/${pedidoId}`, {});
  }

  emitirNfceLote(vendaIds: number[]): Observable<EmitirNfceLoteResultado> {
    return this.http.post<EmitirNfceLoteResultado>(`${this.apiUrl}/emitir-nfce-lote`, {
      vendaIds
    });
  }
}
