import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PessoaGenero } from '../models/pessoa-genero';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PessoaGeneroService {
  private apiUrl = `${environment.apiUrl}/PessoaGenero`;

  constructor(private http: HttpClient) { }

  listar(): Observable<PessoaGenero[]> {
    return this.http.get<PessoaGenero[]>(this.apiUrl);
  }
}