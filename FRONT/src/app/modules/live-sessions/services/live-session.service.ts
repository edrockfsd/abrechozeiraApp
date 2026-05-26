import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface LiveSession {
  id: number;
  liveVideoId: number;
  status: string;
  startedAt: string;
  endedAt: string;
  user: string;
  avatarUrl: string;
  createdAt: string;
  messages: number;
  firstMessage: string;
}

@Injectable({ providedIn: 'root' })
export class LiveSessionService {
  listar(): Observable<LiveSession[]> {
    return of([
      {
        id: 1,
        liveVideoId: 123,
        status: 'Finalizada',
        startedAt: '2025-06-25T17:27:00',
        endedAt: '2025-06-25T18:30:00',
        user: 'setterbrazil',
        avatarUrl: 'https://randomuser.me/api/portraits/men/1.jpg',
        createdAt: '2025-06-19T19:35:10',
        messages: 0,
        firstMessage: '05:27 PM'
      }
    ]);
  }
  getResumo(): Observable<any> {
    return of({
      integracoes: 2,
      relatorios: 10,
      mensagens: 57,
      usuarios: 4
    });
  }
} 