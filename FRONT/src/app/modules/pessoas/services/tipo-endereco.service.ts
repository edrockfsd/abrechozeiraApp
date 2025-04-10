import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface TipoEndereco {
  id: number;
  descricao: string;
}

@Injectable({
  providedIn: 'root'
})
export class TipoEnderecoService {
  private apiUrl = `${environment.apiUrl}/TipoEndereco`;

  constructor(private http: HttpClient) { }

  listar(): Observable<TipoEndereco[]> {
    return this.http.get<TipoEndereco[]>(this.apiUrl);
  }
} 