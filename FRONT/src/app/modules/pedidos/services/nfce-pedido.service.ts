import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface Nfce {
    id: number;
    numero: number;
    serie: number;
    chaveAcesso?: string;
    protocolo?: string;
    status: 'Pendente' | 'Autorizada' | 'Cancelada' | 'Denegada' | 'Rejeitada';
    codigoRetorno?: number;
    mensagemRetorno?: string;
    valorProdutos: number;
    valorDesconto?: number;
    valorTotal: number;
    vendaPdvId?: number;
    pedidoId?: number;
    clienteId?: number;
    dataEmissao: string;
    dataAutorizacao?: string;
    ambiente: number;
}

@Injectable({ providedIn: 'root' })
export class NfcePedidoService {
    private api = `${environment.apiUrl}/Nfce`;

    constructor(private http: HttpClient) { }

    emitirPedido(pedidoId: number): Observable<Nfce> {
        return this.http.post<Nfce>(`${this.api}/emitir/pedido/${pedidoId}`, {});
    }

    listarPorPedido(pedidoId: number): Observable<Nfce[]> {
        return this.http.get<Nfce[]>(`${this.api}?pedidoId=${pedidoId}`);
    }

    cancelar(id: number, justificativa: string): Observable<Nfce> {
        return this.http.post<Nfce>(`${this.api}/${id}/cancelar`, { justificativa });
    }

    getById(id: number): Observable<Nfce> {
        return this.http.get<Nfce>(`${this.api}/${id}`);
    }
}
