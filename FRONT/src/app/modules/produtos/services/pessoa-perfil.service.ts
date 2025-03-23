import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PessoaPerfil } from '../models/pessoa-perfil';

@Injectable({
  providedIn: 'root'
})
export class PessoaPerfilService {
  private apiUrl = 'https://localhost:7194/api/PessoaPerfil';

  constructor(private http: HttpClient) { }

  listar(): Observable<PessoaPerfil[]> {
    return this.http.get<PessoaPerfil[]>(this.apiUrl);
  }
} 