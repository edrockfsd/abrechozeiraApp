import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProdutoGrupo } from '../models/produto-grupo';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ProdutoGrupoService {
  private apiUrl = `${environment.apiUrl}/ProdutoGrupos`;

  constructor(private http: HttpClient) { }

  listar(): Observable<ProdutoGrupo[]> {
    return this.http.get<ProdutoGrupo[]>(this.apiUrl);
  }
} 