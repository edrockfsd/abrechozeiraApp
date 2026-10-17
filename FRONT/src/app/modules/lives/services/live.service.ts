import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Live, LiveCreate } from '../models/live';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class LiveService {
  private apiUrl = `${environment.apiUrl}/Lives`;

  constructor(private http: HttpClient) { }

  listar(): Observable<Live[]> {
    return this.http.get<Live[]>(this.apiUrl);
  }

  buscarPorId(id: number): Observable<Live> {
    return this.http.get<Live>(`${this.apiUrl}/${id}`);
  }

  criar(live: LiveCreate): Observable<Live> {
    return this.http.post<Live>(this.apiUrl, live);
  }

  atualizar(id: number, live: Live): Observable<Live> {
    return this.http.put<Live>(`${this.apiUrl}/${id}`, live);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /** Clientes ativas com telefone que recebem o aviso de live por WhatsApp. */
  destinatariosAvisoLive(): Observable<{ id: number; nome: string; telefone: string }[]> {
    return this.http.get<{ id: number; nome: string; telefone: string }[]>(`${this.apiUrl}/AvisoLive/Destinatarios`);
  }

  /** Envia o aviso de live (template aviso_live_v1) para uma cliente. */
  enviarAvisoLive(pessoaId: number, linkLive: string): Observable<{ enviado: boolean; erro: string | null }> {
    return this.http.post<{ enviado: boolean; erro: string | null }>(`${this.apiUrl}/AvisoLive/Enviar`, { pessoaId, linkLive });
  }

  toggleStatus(id: number): Observable<{ id: number; ativo: boolean }> {
    return this.http.patch<{ id: number; ativo: boolean }>(`${this.apiUrl}/${id}/toggle-status`, {});
  }

  sincronizarPlanilha(liveId: number): Observable<any> {
    // Aponta para a nova rota de sincronização que criamos no ClientPortalController
    return this.http.post<any>(`${environment.apiUrl}/ClientPortal/lives/${liveId}/sync-sheet`, null);
  }
}