import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

// Espelha o LiveSessionResumoDto retornado por API/Controllers/LiveSessionsController.cs
export interface LiveSession {
  id: number;
  liveVideoId: number;
  status: string;
  startedAt: string;
  endedAt: string | null;
  totalComentarios: number;
  primeiroComentarioEm: string | null;
  ultimoComentarioEm: string | null;
}

@Injectable({ providedIn: 'root' })
export class LiveSessionService {
  private apiUrl = `${environment.apiUrl}/LiveSessions`;

  constructor(private http: HttpClient) {}

  listar(): Observable<LiveSession[]> {
    return this.http.get<LiveSession[]>(this.apiUrl);
  }

  buscarPorId(id: number): Observable<LiveSession> {
    return this.http.get<LiveSession>(`${this.apiUrl}/${id}`);
  }
}
