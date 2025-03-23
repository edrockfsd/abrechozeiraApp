import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PessoaGenero } from '../models/pessoa-genero';

@Injectable({
  providedIn: 'root'
})
export class PessoaGeneroService {
  private apiUrl = 'https://localhost:7194/api/PessoaGenero';

  constructor(private http: HttpClient) { }

  listar(): Observable<PessoaGenero[]> {
    return this.http.get<PessoaGenero[]>(this.apiUrl);
  }
} 