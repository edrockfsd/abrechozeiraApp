import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface EmpresaFiscal {
    id?: number;
    cnpj: string;
    inscricaoEstadual: string;
    razaoSocial: string;
    nomeFantasia?: string;
    logradouro?: string;
    numero?: string;
    complemento?: string;
    bairro?: string;
    codigoMunicipio?: string;
    municipio?: string;
    uf: string;
    cep?: string;
    telefone?: string;
    ambiente: number; // 1=Produção, 2=Homologação
    crt: number; // 1=Simples Nacional, 4=MEI
    serie: number;
    proximoNumero?: number;
    tipoEmissao: number;
    csc?: string;
    cscId?: string;
    certificadoPath?: string;
    certificadoSenha?: string;
    certificadoValidade?: string;
    ativo?: boolean;
}

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
    clienteCpfCnpj?: string;
    clienteNome?: string;
    dataEmissao: string;
    dataAutorizacao?: string;
    dataCancelamento?: string;
    justificativaCancelamento?: string;
    ambiente: number;
}

export interface NfceValidacao {
    valido: boolean;
    erro?: string;
}

@Injectable({ providedIn: 'root' })
export class NfceService {
    private api = `${environment.apiUrl}/Nfce`;

    constructor(private http: HttpClient) { }

    // ==================== CONFIGURAÇÃO ====================

    getConfig(): Observable<EmpresaFiscal> {
        return this.http.get<EmpresaFiscal>(`${this.api}/config`);
    }

    saveConfig(config: EmpresaFiscal): Observable<EmpresaFiscal> {
        return this.http.post<EmpresaFiscal>(`${this.api}/config`, config);
    }

    validarConfig(): Observable<NfceValidacao> {
        return this.http.get<NfceValidacao>(`${this.api}/config/validar`);
    }

    // ==================== EMISSÃO ====================

    emitirVendaPdv(vendaPdvId: number): Observable<Nfce> {
        return this.http.post<Nfce>(`${this.api}/emitir/venda/${vendaPdvId}`, {});
    }

    emitirPedido(pedidoId: number): Observable<Nfce> {
        return this.http.post<Nfce>(`${this.api}/emitir/pedido/${pedidoId}`, {});
    }

    // ==================== CONSULTA ====================

    listar(params?: { inicio?: string; fim?: string; status?: string; limite?: number }): Observable<Nfce[]> {
        const queryParams = new URLSearchParams();
        if (params?.inicio) queryParams.set('inicio', params.inicio);
        if (params?.fim) queryParams.set('fim', params.fim);
        if (params?.status) queryParams.set('status', params.status);
        if (params?.limite) queryParams.set('limite', String(params.limite));
        const qs = queryParams.toString();
        return this.http.get<Nfce[]>(`${this.api}${qs ? '?' + qs : ''}`);
    }

    getById(id: number): Observable<Nfce> {
        return this.http.get<Nfce>(`${this.api}/${id}`);
    }

    getByChave(chaveAcesso: string): Observable<Nfce> {
        return this.http.get<Nfce>(`${this.api}/chave/${chaveAcesso}`);
    }

    // ==================== CANCELAMENTO ====================

    cancelar(id: number, justificativa: string): Observable<Nfce> {
        return this.http.post<Nfce>(`${this.api}/${id}/cancelar`, { justificativa });
    }

    // ==================== DANFE ====================

    getDanfe(id: number): Observable<any> {
        return this.http.get<any>(`${this.api}/${id}/danfe`);
    }
}
