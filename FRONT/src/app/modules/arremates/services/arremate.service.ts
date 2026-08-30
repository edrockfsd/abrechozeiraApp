import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface LiveCombo {
  id: number;
  titulo: string;
}

export interface Produto {
  id: number;
  descricao: string;
  precoVenda: number;
}

export interface Arremate {
  id: number;
  codigoEstoque: string | null;
  codigoLive: number;
  produtoDescricao: string;
  valorArremate: number;
  arrematante: string;
  dataArremate: string;
  valorPago?: number;
  dataPagamento?: string;
  fila?: string;
}

export interface ArremateRequest {
  liveId: number;
  codigoLive: number;
  produtoId: number | null;
  descricaoManual?: string | null;
  arrematante: string;
  valorArremate: number;
  observacoes: string;
  dataArremate: string;
  dataAlteracao: string;
  usuarioModificacaoId: number;
  fila?: string;
}

export interface LinhaPreview {
  codigoLive?: number | null;
  descricao: string;
  valor: number;
  comprador: string;
  fila?: string;
  linhaOriginal: number;
}

export interface PreviewResultado {
  totalLinhas: number;
  compradores: number;
  linhas: LinhaPreview[];
}

export interface DetalhePedidoLive {
  cliente: string;
  pessoaCriada: boolean;
  pessoaId: number;
  pedidoId: number;
  pedidoCodigo: number;
  vendaId: number;
  itens: number;
  valorTotal: number;
  descricaoItens: string[];
}

export interface StatusLiveImportacao {
  liveId: number;
  tituloLive: string;
  totalVendas: number;
  totalNfceAutorizadas: number;
  totalArrematesProvisorios: number;
  totalArrematesOficiais: number;
  bloqueadoParaImportacao: boolean;
  motivoBloqueio?: string;
}

export interface ResultadoImportacaoLive {
  mensagem: string;
  produtosCadastrados: number;
  arrematesImportados: number;
  pedidosGerados: number;
  vendasGeradas: number;
  detalhesPedidos: DetalhePedidoLive[];
  erros: any[];
  avisos: string[];
}

@Injectable({
  providedIn: 'root'
})
export class ArremateService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getStatusLive(liveId: number): Observable<StatusLiveImportacao> {
    return this.http.get<StatusLiveImportacao>(`${this.apiUrl}/LiveImport/status-live/${liveId}`);
  }

  getLivesCombo(): Observable<LiveCombo[]> {
    return this.http.get<LiveCombo[]>(`${this.apiUrl}/Lives/GetLivesCombo`);
  }

  getArrematesByLiveId(liveId: number): Observable<Arremate[]> {
    return this.http.get<Arremate[]>(`${this.apiUrl}/Arremates/GetArrematesByLiveID?liveID=${liveId}`);
  }

  getProdutoByCodigoEstoque(codigoEstoque: string): Observable<Produto> {
    return this.http.get<Produto>(`${this.apiUrl}/Produtos/GetProdutoByCodigoEstoque?codigoEstoque=${codigoEstoque}`);
  }

  criar(arremate: Arremate): Observable<Arremate> {
    return this.http.post<Arremate>(`${this.apiUrl}/Arremates`, arremate);
  }

  createArremate(arremate: ArremateRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/Arremates`, arremate);
  }

  deleteArremate(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Arremates/${id}`);
  }

  // ==================== IMPORTAÇÃO DE LIVES ====================

  previewUrl(url: string, sheet: string = 'vendas'): Observable<PreviewResultado> {
    return this.http.get<PreviewResultado>(
      `${this.apiUrl}/LiveImport/preview-url?url=${encodeURIComponent(url)}&sheet=${encodeURIComponent(sheet)}`
    );
  }

  importarPlanilhaUrl(liveId: number, googleSheetUrl: string, sheetName: string = 'vendas'): Observable<ResultadoImportacaoLive> {
    return this.http.post<ResultadoImportacaoLive>(`${this.apiUrl}/LiveImport/importar-url`, {
      liveId,
      googleSheetUrl,
      sheetName
    });
  }

  importarPlanilhaXlsx(arquivo: File, liveId: number): Observable<ResultadoImportacaoLive> {
    const formData = new FormData();
    formData.append('arquivo', arquivo);
    formData.append('liveId', liveId.toString());
    return this.http.post<ResultadoImportacaoLive>(`${this.apiUrl}/LiveImport/importar-xlsx`, formData);
  }
}
