import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface FormaPagamento {
  id: number;
  descricao: string;
}

@Injectable({
  providedIn: 'root'
})
export class FormaPagamentoService {
  private apiUrl = `${environment.apiUrl}/FormaPagamento`;

  constructor(private http: HttpClient) { }

  listar(): Observable<FormaPagamento[]> {
    return this.http.get<FormaPagamento[]>(this.apiUrl);
  }

  obterPorId(id: number): Observable<FormaPagamento> {
    return this.http.get<FormaPagamento>(`${this.apiUrl}/${id}`);
  }
} 