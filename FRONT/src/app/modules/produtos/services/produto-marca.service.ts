import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProdutoMarca } from '../models/produto-marca';

@Injectable({
  providedIn: 'root'
})
export class ProdutoMarcaService {
  private apiUrl = 'https://localhost:7194/api/ProdutoMarca';

  constructor(private http: HttpClient) { }

  listar(): Observable<ProdutoMarca[]> {
    return this.http.get<ProdutoMarca[]>(this.apiUrl);
  }
} 