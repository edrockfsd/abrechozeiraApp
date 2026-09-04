import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

// Espelha o ComentarioLiveDto retornado por API/Controllers/LiveSessionsController.cs
export interface ComentarioLive {
  id: number;
  username: string;
  commentText: string;
  commentTimestamp: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class ComentarioLiveService {
  private apiUrl = `${environment.apiUrl}/LiveSessions`;

  constructor(private http: HttpClient) {}

  listarPorLiveSession(liveSessionId: number): Observable<ComentarioLive[]> {
    return this.http.get<ComentarioLive[]>(`${this.apiUrl}/${liveSessionId}/comentarios`);
  }
}
