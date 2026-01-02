import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Pessoa, PessoaCreate } from '../models/pessoa';
import { environment } from '../../../../environments/environment';

export interface PessoaGrid {
  id: number;
  nome: string;
  dataNascimento: string;
  email: string;
  telefone: string;
  nickName: string;
  generoId: number;
  generoDescricao: string;
  categoriaId: number;
  categoriaDescricao: string;
  statusId: number;
  statusDescricao: string;
}

@Injectable({
  providedIn: 'root'
})
export class PessoaService {
  private apiUrl = `${environment.apiUrl}/Pessoas`;
  private apiUrlCategorias = `${environment.apiUrl}/PessoaCategorias`;
  private apiUrlTipos = `${environment.apiUrl}/PessoaTipos`;
  private apiUrlGeneros = `${environment.apiUrl}/PessoaGenero`;
  private apiUrlStatus = `${environment.apiUrl}/PessoaStatus`;

  constructor(private http: HttpClient) { }

  listar(): Observable<Pessoa[]> {
    return this.http.get<Pessoa[]>(this.apiUrl);
  }

  buscarPorId(id: number): Observable<Pessoa> {
    return this.http.get<Pessoa>(`${this.apiUrl}/${id}`);
  }

  criar(pessoa: PessoaCreate): Observable<Pessoa> {
    return this.http.post<Pessoa>(this.apiUrl, pessoa);
  }

  atualizar(id: number, pessoa: Pessoa): Observable<Pessoa> {
    return this.http.put<Pessoa>(`${this.apiUrl}/${id}`, pessoa);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  listarCategorias(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrlCategorias);
  }

  listarTipos(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrlTipos);
  }

  listarGeneros(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrlGeneros);
  }

  listarStatus(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrlStatus);
  }

  listarClientesCombo(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/GetClientesCombo`);
  }

  listarGrid(): Observable<PessoaGrid[]> {
    return this.http.get<PessoaGrid[]>(`${this.apiUrl}/GetPessoasGrid`);
  }
}