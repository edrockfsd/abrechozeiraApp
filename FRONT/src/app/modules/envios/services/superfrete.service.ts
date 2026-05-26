import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CotarFreteInput,
  CotacaoFreteResultado,
  CriarEtiquetaInput,
  CriarEtiquetaResultado,
  EtiquetaInfo,
  SUPERFRETE_STATUS_MAP
} from '../interfaces/superfrete.interface';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class SuperfreteService {
  private apiUrl = `${environment.apiUrl}/Superfrete`;

  constructor(private http: HttpClient) {}

  /** Calcula cotações de frete para diferentes transportadoras. */
  cotarFrete(input: CotarFreteInput): Observable<CotacaoFreteResultado[]> {
    return this.http.post<CotacaoFreteResultado[]>(`${this.apiUrl}/CotarFrete`, input);
  }

  /** Cria uma etiqueta de envio no Superfrete (status inicial: pending). */
  criarEtiqueta(input: CriarEtiquetaInput): Observable<CriarEtiquetaResultado> {
    return this.http.post<CriarEtiquetaResultado>(`${this.apiUrl}/CriarEtiqueta`, input);
  }

  /** Lista todas as etiquetas geradas na conta Superfrete. */
  listarEtiquetas(): Observable<EtiquetaInfo[]> {
    return this.http.get<EtiquetaInfo[]>(`${this.apiUrl}/Etiquetas`).pipe(
      map(etiquetas => etiquetas.map(e => this.enriquecerEtiqueta(e)))
    );
  }

  /** Retorna detalhes de uma etiqueta pelo ID. */
  obterEtiqueta(id: string): Observable<EtiquetaInfo> {
    return this.http.get<EtiquetaInfo>(`${this.apiUrl}/Etiqueta/${id}`).pipe(
      map(e => this.enriquecerEtiqueta(e))
    );
  }

  private enriquecerEtiqueta(e: EtiquetaInfo): EtiquetaInfo {
    const statusInfo = SUPERFRETE_STATUS_MAP[e.status] ?? { label: e.status, cssClass: '' };
    return { ...e, statusLabel: statusInfo.label, statusClass: statusInfo.cssClass };
  }
}
