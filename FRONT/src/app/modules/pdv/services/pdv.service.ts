import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface VendaPdv {
  id: number;
  codigo?: string;
  clienteId?: number | null;
  status: 'Aberta' | 'Finalizada' | 'Cancelada';
  dataVenda: string;
  valorBruto: number;
  desconto?: number | null;
  valorLiquido: number;
  observacao?: string | null;
  usuarioId?: number | null;
  caixaId?: number | null;
  dataAlteracao: string;
}

export interface VendaPdvItem {
  id: number;
  vendaPdvId: number;
  produtoId?: number | null;
  descricaoItem: string;
  quantidade: number;
  precoUnitario: number;
  descontoValor?: number | null;
  descontoPerc?: number | null;
  total: number;
  codigoEstoque?: string | null;
}

export interface VendaPdvPagamento {
  id: number;
  vendaPdvId: number;
  formaPagamentoId: number;
  condicaoPagamentoId?: number | null;
  valor: number;
  observacao?: string | null;
  transacaoId?: string | null;
  dataRegistro: string;
}

export interface PdvConfig {
  formasPagamento: { id:number; descricao:string; exibirNoPDV:boolean; permiteParcelamento:boolean; maxParcelas?:number|null; taxaAdmPerc?:number|null }[];
  condicaoPagamento: { id:number; descricao:string }[];
}

@Injectable({ providedIn: 'root' })
export class PdvService {
  private api = environment.apiUrl;
  private currentVenda$ = new BehaviorSubject<{ venda: VendaPdv|null, itens: VendaPdvItem[], pagamentos: VendaPdvPagamento[] }>({ venda: null, itens: [], pagamentos: [] });

  constructor(private http: HttpClient) {}

  get state$() { return this.currentVenda$.asObservable(); }

  novaVenda(): Observable<number> {
    const payload = { status: 'Aberta' } as any;
    return this.http.post<{id:number}>(`${this.api}/VendasPdv`, payload).pipe(
      tap((res) => this.carregar(res.id).subscribe()) ,
      map(res => res.id)
    );
  }

  carregar(id: number): Observable<void> {
    return this.http.get<{ venda: VendaPdv, itens: VendaPdvItem[], pagamentos: VendaPdvPagamento[] }>(`${this.api}/VendasPdv/${id}`).pipe(
      tap(data => this.currentVenda$.next(data as any)),
      map(() => void 0)
    );
  }

  addItem(vendaId: number, item: Partial<VendaPdvItem>): Observable<void> {
    return this.http.post(`${this.api}/VendasPdv/${vendaId}/itens`, item).pipe(map(() => void 0));
  }

  updateItem(vendaId: number, itemId: number, item: Partial<VendaPdvItem>): Observable<void> {
    return this.http.put(`${this.api}/VendasPdv/${vendaId}/itens/${itemId}`, item).pipe(map(() => void 0));
  }

  deleteItem(vendaId: number, itemId: number): Observable<void> {
    return this.http.delete(`${this.api}/VendasPdv/${vendaId}/itens/${itemId}`).pipe(map(() => void 0));
  }

  addPagamento(vendaId: number, pagamento: Partial<VendaPdvPagamento>): Observable<void> {
    return this.http.post(`${this.api}/VendasPdv/${vendaId}/pagamentos`, pagamento).pipe(map(() => void 0));
  }

  deletePagamento(vendaId: number, pgId: number): Observable<void> {
    return this.http.delete(`${this.api}/VendasPdv/${vendaId}/pagamentos/${pgId}`).pipe(map(() => void 0));
  }

  finalizar(vendaId: number): Observable<void> {
    return this.http.post(`${this.api}/VendasPdv/${vendaId}/finalizar`, {}).pipe(map(() => void 0));
  }

  cancelar(vendaId: number): Observable<void> {
    return this.http.post(`${this.api}/VendasPdv/${vendaId}/cancelar`, {}).pipe(map(() => void 0));
  }

  getConfig(): Observable<PdvConfig> {
    return this.http.get<PdvConfig>(`${this.api}/VendasPdv/config`);
  }
}

