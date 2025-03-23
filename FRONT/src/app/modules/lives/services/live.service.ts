import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Live, LiveCreate } from '../models/live';

@Injectable({
  providedIn: 'root'
})
export class LiveService {
  private apiUrl = 'https://localhost:7194/api/Lives';

  constructor(private http: HttpClient) { }

  listar(): Observable<Live[]> {
    return this.http.get<Live[]>(this.apiUrl);
  }

  buscarPorId(id: number): Observable<Live> {
    return this.http.get<Live>(`${this.apiUrl}/${id}`);
  }

  criar(live: LiveCreate): Observable<Live> {
    return this.http.post<Live>(this.apiUrl, live);
  }

  atualizar(id: number, live: Live): Observable<Live> {
    return this.http.put<Live>(`${this.apiUrl}/${id}`, live);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
} 