import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface PedidoStatus {
  id: number;
  descricao: string;
}

@Injectable({
  providedIn: 'root'
})
export class PedidoStatusService {
  private apiUrl = `${environment.apiUrl}/PedidoStatus`;

  constructor(private http: HttpClient) { }

  listar(): Observable<PedidoStatus[]> {
    return this.http.get<PedidoStatus[]>(this.apiUrl);
  }
} 