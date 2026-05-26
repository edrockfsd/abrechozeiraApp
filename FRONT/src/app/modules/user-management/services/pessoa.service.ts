import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Pessoa } from '../../../models/pessoa';

@Injectable({
  providedIn: 'root'
})
export class PessoaService {
  private apiUrl = `${environment.apiUrl}/pessoas`;

  constructor(private http: HttpClient) { }

  getPessoas(): Observable<Pessoa[]> {
    return this.http.get<Pessoa[]>(this.apiUrl);
  }

  // Add a method to get pessoas not yet associated with a user
  getPessoasSemUsuario(): Observable<Pessoa[]> {
    return this.http.get<Pessoa[]>(`${this.apiUrl}/sem-usuario`); // Assuming this endpoint exists
  }
}
