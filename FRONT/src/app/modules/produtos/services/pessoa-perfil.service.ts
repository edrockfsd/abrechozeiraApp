import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PessoaPerfil } from '../models/pessoa-perfil';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PessoaPerfilService {
  private apiUrl = `${environment.apiUrl}/PessoaPerfil`;

  constructor(private http: HttpClient) { }

  listar(): Observable<PessoaPerfil[]> {
    return this.http.get<PessoaPerfil[]>(this.apiUrl);
  }
} 