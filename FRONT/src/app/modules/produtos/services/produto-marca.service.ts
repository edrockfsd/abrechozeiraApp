import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProdutoMarca } from '../models/produto-marca';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ProdutoMarcaService {
  private apiUrl = `${environment.apiUrl}/ProdutoMarca`;

  constructor(private http: HttpClient) { }

  listar(): Observable<ProdutoMarca[]> {
    return this.http.get<ProdutoMarca[]>(this.apiUrl);
  }

  criar(marca: ProdutoMarca): Observable<ProdutoMarca> {
    return this.http.post<ProdutoMarca>(this.apiUrl, marca);
  }
}
