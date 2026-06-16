import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ParseTextoInput,
  ParseTextoResultado,
  CotarLoteInput,
  CotarLoteResultado,
  GerarEtiquetasLoteInput,
  GerarEtiquetasLoteResultado,
  CotarEGerarInput,
  CotarEGerarResultado
} from '../interfaces/envio-lote.interface';

@Injectable({
  providedIn: 'root'
})
export class EnvioLoteService {
  private apiUrl = `${environment.apiUrl}/EnvioLote`;

  constructor(private http: HttpClient) {}

  /** Envia texto do WhatsApp para parsing automático. */
  parseTexto(input: ParseTextoInput): Observable<ParseTextoResultado> {
    return this.http.post<ParseTextoResultado>(`${this.apiUrl}/ParseTexto`, input);
  }

  /** Cota frete (PAC + SEDEX) SEM gerar etiqueta. Funciona individual ou em lote. */
  cotarLote(input: CotarLoteInput): Observable<CotarLoteResultado> {
    return this.http.post<CotarLoteResultado>(`${this.apiUrl}/CotarLote`, input);
  }

  /** Gera etiquetas em lote. Requer que já tenha cotado e definido o servicoId. */
  gerarEtiquetasLote(input: GerarEtiquetasLoteInput): Observable<GerarEtiquetasLoteResultado> {
    return this.http.post<GerarEtiquetasLoteResultado>(`${this.apiUrl}/GerarEtiquetasLote`, input);
  }

  /** (Legacy) Cota frete e gera etiquetas de uma vez. */
  cotarEGerarLote(input: CotarEGerarInput): Observable<CotarEGerarResultado> {
    return this.http.post<CotarEGerarResultado>(`${this.apiUrl}/CotarEGerarLote`, input);
  }

  /** Reenvia e-mail de rastreio manualmente. */
  enviarRastreio(etiquetaId: string, email: string, nome?: string): Observable<{ message: string }> {
    const params: any = { email };
    if (nome) params.nome = nome;
    return this.http.post<{ message: string }>(`${this.apiUrl}/EnviarRastreio/${etiquetaId}`, null, { params });
  }

  /** Verifica status de pagamento e superfrete em lote. */
  verificarStatus(transacaoIds: string[]): Observable<any[]> {
    return this.http.post<any[]>(`${this.apiUrl}/VerificarStatus`, transacaoIds);
  }

  /** Envia e-mails de rastreio em lote. */
  enviarRastreioLote(input: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/EnviarRastreioLote`, input);
  }

  /** Obtém todos os mapeamentos de pagamento */
  getMapeamentos(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Mapeamentos`);
  }
}
