import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';

export interface CaixaStatus {
  id: number;
  usuarioId: number;
  dataAbertura: string;
  saldoInicial: number;
  dataFechamento?: string | null;
  saldoFechamento?: number | null;
  observacao?: string | null;
}

export interface CaixaMovimentoDto {
  id: number;
  caixaId: number;
  tipo: string;
  valor: number;
  referenciaId?: number | null;
  observacao?: string | null;
  dataRegistro: string;
}

@Injectable({ providedIn: 'root' })
export class CaixaService {
  private api = `${environment.apiUrl}/Caixa`;
  private STORAGE_KEY = 'current_caixa';

  private currentSubject = new BehaviorSubject<CaixaStatus | null>(null);
  current$ = this.currentSubject.asObservable();

  private movimentosSubject = new BehaviorSubject<CaixaMovimentoDto[]>([]);
  movimentos$ = this.movimentosSubject.asObservable();

  constructor(private http: HttpClient) {
    const saved = localStorage.getItem(this.STORAGE_KEY);
    if (saved) {
      try { this.currentSubject.next(JSON.parse(saved)); } catch { localStorage.removeItem(this.STORAGE_KEY); }
    }
    this.refreshAberto().subscribe();
  }

  get currentValue(): CaixaStatus | null { return this.currentSubject.value; }
  get currentCaixaId(): number | null { return this.currentSubject.value?.id ?? null; }

  refreshAberto(): Observable<CaixaStatus | null> {
    return this.http.get<CaixaStatus>(`${this.api}/aberto`).pipe(
      tap(caixa => this.setCurrent(caixa)),
      catchError(() => { this.setCurrent(null); return of(null); })
    );
  }

  abrirCaixa(payload: { saldoInicial: number; observacao?: string | null }): Observable<CaixaStatus> {
    const body = { saldoInicial: payload.saldoInicial, observacao: payload.observacao };
    return this.http.post<number>(`${this.api}/abrir`, body).pipe(
      switchMap(id => this.status(id)),
      tap(caixa => this.setCurrent(caixa))
    );
  }

  status(id: number): Observable<CaixaStatus> {
    return this.http.get<CaixaStatus>(`${this.api}/status/${id}`);
  }

  fecharCaixa(saldoFechamento: number): Observable<void> {
    const caixa = this.currentSubject.value;
    if (!caixa) return throwError(() => new Error('Nenhum caixa aberto'));
    return this.http.post<void>(`${this.api}/fechar/${caixa.id}`, saldoFechamento).pipe(
      tap(() => this.setCurrent(null))
    );
  }

  registrarSuprimento(valor: number, observacao?: string): Observable<void> {
    const caixa = this.currentSubject.value;
    if (!caixa) return throwError(() => new Error('Nenhum caixa aberto'));
    const body = { caixaId: caixa.id, valor, observacao };
    return this.http.post<void>(`${this.api}/suprimento`, body).pipe(
      tap(() => this.loadMovimentos(caixa.id).subscribe())
    );
  }

  registrarSangria(valor: number, observacao?: string): Observable<void> {
    const caixa = this.currentSubject.value;
    if (!caixa) return throwError(() => new Error('Nenhum caixa aberto'));
    const body = { caixaId: caixa.id, valor, observacao };
    return this.http.post<void>(`${this.api}/sangria`, body).pipe(
      tap(() => this.loadMovimentos(caixa.id).subscribe())
    );
  }

  loadMovimentos(caixaId?: number): Observable<CaixaMovimentoDto[]> {
    const id = caixaId ?? this.currentSubject.value?.id;
    if (!id) { this.movimentosSubject.next([]); return of([]); }
    return this.http.get<CaixaMovimentoDto[]>(`${this.api}/${id}/movimentos`).pipe(
      tap(list => this.movimentosSubject.next(list)),
      catchError(() => { this.movimentosSubject.next([]); return of([]); })
    );
  }

  private setCurrent(caixa: CaixaStatus | null) {
    this.currentSubject.next(caixa);
    if (caixa) localStorage.setItem(this.STORAGE_KEY, JSON.stringify(caixa));
    else localStorage.removeItem(this.STORAGE_KEY);
  }
}
