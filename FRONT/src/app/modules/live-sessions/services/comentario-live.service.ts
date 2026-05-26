import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface ComentarioLive {
  id: number;
  username: string;
  commentText: string;
  commentTimestamp: string;
  createdAt: string;
  liveSessionId: number;
}

@Injectable({ providedIn: 'root' })
export class ComentarioLiveService {
  listarPorLiveSession(liveSessionId: number): Observable<ComentarioLive[]> {
    return of([
      {
        id: 1,
        username: 'usuario1',
        commentText: 'Primeiro comentário!',
        commentTimestamp: '2025-06-25T17:28:00',
        createdAt: '2025-06-25T17:28:00',
        liveSessionId
      }
    ]);
  }
} 