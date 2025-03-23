import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

interface LiveCombo {
  id: number;
  titulo: string;
}

interface Produto {
  id: number;
  descricao: string;
  precoVenda: number;
}

export interface Arremate {
  id: number;
  codigoEstoque: string;
  codigoLive: number;
  produtoDescricao: string;
  valorArremate: number;
  arrematante: string;
  dataArremate: string;
}

export interface ArremateRequest {
  liveId: number;
  codigoLive: number;
  produtoId: number;
  arrematante: string;
  valorArremate: number;
  observacoes: string;
  dataArremate: string;
  dataAlteracao: string;
  usuarioModificacaoId: number;
}

@Injectable({
  providedIn: 'root'
})
export class ArremateService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

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
    return this.http.delete(`https://localhost:7194/api/Arremates/${id}`);
  }
}