import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface CondicaoPagamento {
  id: number;
  descricao: string;
}

@Injectable({
  providedIn: 'root'
})
export class CondicaoPagamentoService {
  private apiUrl = `${environment.apiUrl}/CondicaoPagamento`;

  constructor(private http: HttpClient) { }

  listar(): Observable<CondicaoPagamento[]> {
    return this.http.get<CondicaoPagamento[]>(this.apiUrl);
  }

  obterPorId(id: number): Observable<CondicaoPagamento> {
    return this.http.get<CondicaoPagamento>(`${this.apiUrl}/${id}`);
  }
} 